namespace MyApp.Models
{
    /// <summary>
    /// 聊天請求模型
    /// </summary>
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? History { get; set; }
        public string? SessionId { get; set; }
    }
}