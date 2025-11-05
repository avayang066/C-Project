namespace MyApp.Models
{
    /// <summary>
    /// 聊天訊息模型
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty; // "User" 或 "AI"
        public DateTime Timestamp { get; set; }
        public string? SessionId { get; set; } // 這是接收前端請求的格式，不是回傳格式
    }
}