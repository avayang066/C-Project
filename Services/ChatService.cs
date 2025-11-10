using System.Net.Http;
using System.Text;
using System.Text.Json;
using MyApp.Models;
using MyApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp.Services
{
    /// <summary>
    /// 聊天服務 - 包含所有業務邏輯
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ApplicationDbContext dbContext, ILogger<ChatService> logger)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:11434");
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 取得服務狀態 - 包含所有邏輯
        /// </summary>
        public async Task<ServiceStatus> GetServiceStatusAsync()
        {            
            var isAvailable = await IsServiceAvailableAsync();


            return new ServiceStatus
            {
                IsAvailable = isAvailable,
                Message = isAvailable ? "AI 服務正常運行中" : null,
                ErrorMessage = isAvailable ? null : "AI 服務目前無法使用，請確認 Ollama 是否正在運行"
            };
        }

        /// <summary>
        /// 處理聊天請求 - 包含資料庫記錄
        /// </summary>
        public async Task<ChatProcessResponse> ProcessChatAsync(string message, string? sessionId = null)
        {
            try
            {

                // 1. 驗證邏輯
                if (string.IsNullOrWhiteSpace(message))
                {
                    return new ChatProcessResponse
                    {
                        Success = false,
                        Error = "訊息不能為空"
                    };
                }

                // 2. 取得或建立會話
                var session = await GetOrCreateSessionAsync(sessionId);
                _logger.LogDebug("使用會話: {SessionId}", session.SessionId);

                // 3. 記錄用戶訊息
                await SaveUserMessageAsync(session.SessionId, message);

                // 4. 取得對話歷史
                var history = await GetChatHistoryAsync(session.SessionId);
                _logger.LogDebug("載入對話歷史，長度: {HistoryLength}", history.Length);

                // 5. 呼叫 AI 服務
                var reply = await SendMessageAsync(message, history);
                _logger.LogInformation("AI 回應成功，長度: {ReplyLength}", reply.Length);

                // 6. 記錄 AI 回應
                await SaveAIMessageAsync(session.SessionId, reply);

                // 7. 更新會話活動時間
                await UpdateSessionActivityAsync(session.SessionId);

                // 8. 成功回應處理
                return new ChatProcessResponse
                {
                    Success = true,
                    Reply = reply,
                    SessionId = session.SessionId,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                };
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "處理聊天請求時發生錯誤: {ErrorMessage}", ex.Message);
                // 4. 錯誤處理邏輯
                return new ChatProcessResponse
                {
                    Success = false,
                    Error = "處理您的訊息時發生錯誤：" + ex.Message
                };
            }
        }

        /// <summary>
        /// 發送訊息到 AI (內部方法)
        /// </summary>
        private async Task<string> SendMessageAsync(string message, string history = "")
        {
            try
            {
                var payload = new
                {
                    model = "gemma3:4b",
                    prompt = BuildPrompt(message, history),
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. HTTP API 呼叫 (Ollama)
                var response = await _httpClient.PostAsync("/api/generate", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(jsonResponse);

                if (doc.RootElement.TryGetProperty("response", out var responseProperty))
                {
                    return responseProperty.GetString() ?? "抱歉，AI 沒有回應";
                }

                return "抱歉，AI 回應格式異常";
            }
            catch (HttpRequestException)
            {
                return "抱歉，無法連接到 AI 服務，請確認 Ollama 是否正在運行";
            }
            catch (TaskCanceledException)
            {
                return "抱歉，AI 服務回應逾時，請稍後再試";
            }
            catch (Exception)
            {
                return "抱歉，處理您的訊息時發生錯誤";
            }
        }

        /// <summary>
        /// 檢查服務是否可用 (內部方法)
        /// </summary>
        private async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags");
                var isAvailable = response.IsSuccessStatusCode;
                return isAvailable;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 建構提示詞
        /// </summary>
        private static string BuildPrompt(string message, string history)
        {
            if (string.IsNullOrEmpty(history))
            {
                return $"請用繁體中文回答：{message}";
            }
            return $"{history}\nUser: {message}\nAI:";
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        /// <summary>
        /// 取得或建立聊天會話
        /// </summary>
        private async Task<ChatSession> GetOrCreateSessionAsync(string? sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                // 1. 資料庫查詢
                var existingSession = await _dbContext.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (existingSession != null)
                {
                    return existingSession;
                }
            }

            // 建立新會話
            var newSession = new ChatSession
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                LastActivity = DateTime.Now,
                IsActive = true
            };

            _dbContext.ChatSessions.Add(newSession);
            // 2. 資料庫儲存
            await _dbContext.SaveChangesAsync();

            return newSession;
        }

        /// <summary>
        /// 儲存用戶訊息
        /// </summary>
        private async Task SaveUserMessageAsync(string sessionId, string message)
        {
            var userMessage = new ChatMessage
            {
                Content = message,
                Sender = "User",
                Timestamp = DateTime.Now,
                SessionId = sessionId
            };

            _dbContext.ChatMessages.Add(userMessage);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 儲存 AI 回應
        /// </summary>
        private async Task SaveAIMessageAsync(string sessionId, string reply)
        {
            var aiMessage = new ChatMessage
            {
                Content = reply,
                Sender = "AI",
                Timestamp = DateTime.Now,
                SessionId = sessionId
            };

            _dbContext.ChatMessages.Add(aiMessage);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 取得聊天歷史
        /// </summary>
        private async Task<string> GetChatHistoryAsync(string sessionId)
        {
            var messages = await _dbContext.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .Take(10) // 只取最近 10 則訊息
                .ToListAsync();

            if (!messages.Any())
                return string.Empty;

            var history = string.Join("\n", messages.Select(m => $"{m.Sender}: {m.Content}"));
            return history;
        }

        /// <summary>
        /// 更新會話活動時間
        /// </summary>
        private async Task UpdateSessionActivityAsync(string sessionId)
        {
            var session = await _dbContext.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
            {
                session.LastActivity = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}