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
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: /Chat - 顯示聊天頁面
        public async Task<IActionResult> Index()
        {
            // 使用 Extension Method 簡化狀態處理
            var status = await _chatService.GetServiceStatusAsync();
            status.ToViewBag(this);
            
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
            // 使用 Extension Method 驗證請求
            var validationError = request.ValidateRequest();
            if (validationError != null) return validationError;

            // 使用 Extension Method 處理回應，支援會話管理
            return await _chatService
                .ProcessChatAsync(request.Message, request.SessionId)
                .ToJsonResponse();
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
