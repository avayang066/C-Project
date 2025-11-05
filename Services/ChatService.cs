using System.Net.Http;
using System.Text;
using System.Text.Json;
using MyApp.Models;

namespace MyApp.Services
{
    /// <summary>
    /// 聊天服務 - 包含所有業務邏輯
    /// </summary>
    public class ChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:11434");
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
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
        /// 處理聊天請求 - 包含所有業務邏輯
        /// </summary>
        public async Task<ChatProcessResponse> ProcessChatAsync(string message, string history)
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

                // 2. 呼叫 AI 服務
                var reply = await SendMessageAsync(message, history);
                
                // 3. 成功回應處理
                return new ChatProcessResponse
                {
                    Success = true,
                    Reply = reply,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                };
            }
            catch (Exception ex)
            {
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
                return response.IsSuccessStatusCode;
            }
            catch
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
    }
}