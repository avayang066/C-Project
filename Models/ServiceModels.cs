namespace MyApp.Models
{
    /// <summary>
    /// 服務狀態模型
    /// </summary>
    public class ServiceStatus
    {
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 聊天處理回應模型
    /// </summary>
    public class ChatProcessResponse
    {
        public bool Success { get; set; }
        public string? Reply { get; set; }
        public string? Error { get; set; }
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    }
}