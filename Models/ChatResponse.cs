namespace MyApp.Models
{
    /// <summary>
    /// 聊天回應模型
    /// </summary>
    public class ChatResponse
    {
        public bool Success { get; set; }
        public string? Reply { get; set; }
        public string? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? SessionId { get; set; }
    }
}