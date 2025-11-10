using MyApp.Models;

namespace MyApp.Services
{
    /// <summary>
    /// 聊天服務介面
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// 取得服務狀態
        /// </summary>
        /// <returns>服務狀態</returns>
        Task<ServiceStatus> GetServiceStatusAsync();

        /// <summary>
        /// 處理聊天請求
        /// </summary>
        /// <param name="message">使用者訊息</param>
        /// <param name="sessionId">會話ID</param>
        /// <returns>聊天處理回應</returns>
        Task<ChatProcessResponse> ProcessChatAsync(string message, string? sessionId = null);

        /// <summary>
        /// 釋放資源
        /// </summary>
        void Dispose();
    }
}