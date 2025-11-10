using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using MyApp.Models;
using System.Text.Json;
using MyApp.Extensions;

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
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: /Chat - 顯示聊天頁面
        public async Task<IActionResult> Index()
        {
            var requestTime = DateTime.Now.ToString("HH:mm:ss.fff");
            var randomNum = new Random().Next(1000, 9999); // 每次都不同

            Console.WriteLine($"📱 [{requestTime}] === ChatController.Index() 被呼叫 - 有人訪問聊天頁面！ (隨機號: {randomNum})");
            
            // 使用 Extension Method 簡化狀態處理
            var status = await _chatService.GetServiceStatusAsync();
            status.ToViewBag(this);
            
            Console.WriteLine($"🏁 [{DateTime.Now:HH:mm:ss.fff}] === ChatController.Index() 完成 (隨機號: {randomNum})");
            return View();
        }

        // POST: /Chat/SendMessage - 處理聊天訊息
        // 支援 JSON 格式，使用 Extension Methods 簡化程式碼
        // POST http://localhost:5000/Chat/SendMessage  
        // Content-Type: application/json
        // Body: { "message": "測試訊息", "history": "" }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var requestTime = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"💬 [{requestTime}] 收到聊天請求: {request?.Message}");
            
            // 使用 Extension Method 驗證請求
            var validationError = request.ValidateRequest();
            if (validationError != null) 
            {
                // Console.WriteLine($"❌ [{DateTime.Now:HH:mm:ss.fff}] 驗證失敗");
                return validationError;
            }

            
            // 使用 Extension Method 處理回應，支援會話管理
            var result = await _chatService
                .ProcessChatAsync(request!.Message, request.SessionId)
                .ToJsonResponse();
                
            return result;
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
