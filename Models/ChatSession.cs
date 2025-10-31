namespace MyApp.Models
{
    /// <summary>
    /// 聊天會話模型
    /// </summary>
    public class ChatSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public bool IsActive { get; set; } = true;
    }
}