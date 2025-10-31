using MyApp.Models;

namespace MyApp.Services
{
    /// <summary>
    /// 聊天服務介面
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// 處理聊天請求
        /// </summary>
        /// <param name="request">聊天請求</param>
        /// <returns>聊天回應</returns>
        Task<ChatResponse> ProcessChatAsync(ChatRequest request);
        
        /// <summary>
        /// 檢查 AI 服務是否可用
        /// </summary>
        /// <returns>服務狀態</returns>
        Task<bool> IsServiceAvailableAsync();
        
        /// <summary>
        /// 取得聊天會話
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>聊天會話</returns>
        Task<ChatSession?> GetSessionAsync(string sessionId);
        
        /// <summary>
        /// 儲存聊天訊息
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <param name="message">訊息</param>
        /// <returns>儲存的訊息</returns>
        Task<ChatMessage> SaveMessageAsync(string sessionId, ChatMessage message);
    }
}