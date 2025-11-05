using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

// http://localhost:5000/Chat
// http://localhost:5000/Chat/Index
// http://localhost:5000/Chat/SendMessage

namespace MyApp.Controllers
{
    /// <summary>
    /// 聊天控制器 - 純粹的 HTTP 處理
    /// </summary>
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public ChatController()
        {
            _chatService = new ChatService();
        }

        // GET: /Chat - 顯示聊天頁面
        public async Task<IActionResult> Index()
        {
            // 只負責取得狀態並傳遞給 View
            var status = await _chatService.GetServiceStatusAsync();

            ViewBag.ServiceAvailable = status.IsAvailable;
            ViewBag.ErrorMessage = status.ErrorMessage;
            ViewBag.SuccessMessage = status.Message;

            return View();
        }

        // POST: /Chat/SendMessage - 處理聊天訊息

        // 路由測試須送的格式
        // POST http://localhost:5000/Chat/SendMessage
        // Content-Type: application/x-www-form-urlencoded
        // message = 測試訊息 & history =
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message, string history)
        {
            var debugInfo = new List<object>();
            
            // 只負責呼叫服務並回傳結果
            var response = await _chatService.ProcessChatAsync(message, history);

            debugInfo.Add(new { stage = "input", message, history });

            return Json(new
            {
                success = response.Success,
                reply = response.Reply,
                error = response.Error,
                timestamp = response.Timestamp
            });


        }


        // 釋放資源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chatService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
