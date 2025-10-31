using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

namespace MyApp.Controllers
{
    /// <summary>
    /// 聊天控制器 - 簡單直接的方式
    /// </summary>
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public ChatController()
        {
            // 直接建立服務，不用依賴注入
            _chatService = new ChatService();
        }

        // GET: /Chat - 顯示聊天頁面
        public async Task<IActionResult> Index()
        {
            // 檢查 AI 服務是否可用
            var isAvailable = await _chatService.IsServiceAvailableAsync();
            ViewBag.ServiceAvailable = isAvailable;
            
            if (!isAvailable)
            {
                ViewBag.ErrorMessage = "AI 服務目前無法使用，請確認 Ollama 是否正在運行";
            }
            else
            {
                ViewBag.SuccessMessage = "AI 服務正常運行中";
            }
            
            return View();
        }

        // POST: /Chat/SendMessage - 處理聊天訊息
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message, string history)
        {
            try
            {
                // 簡單驗證
                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { 
                        success = false, 
                        error = "訊息不能為空" 
                    });
                }

                // 呼叫服務取得 AI 回應
                var reply = await _chatService.SendMessageAsync(message, history);
                
                // 回傳成功結果
                return Json(new { 
                    success = true, 
                    reply = reply,
                    timestamp = DateTime.Now.ToString("HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                // 簡單的錯誤處理
                return Json(new { 
                    success = false, 
                    error = "處理您的訊息時發生錯誤：" + ex.Message
                });
            }
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
