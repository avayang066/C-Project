using Microsoft.AspNetCore.Mvc;
using MyApp.Models;

namespace MyApp.Extensions
{
    /// <summary>
    /// 回應格式化的擴展方法
    /// </summary>
    public static class ResponseExtensions
    {
        /// <summary>
        /// 將 ChatProcessResponse 轉換為 JSON IActionResult
        /// </summary>
        public static async Task<IActionResult> ToJsonResponse(this Task<ChatProcessResponse> responseTask)
        {
            var response = await responseTask;
            
            return new JsonResult(new
            {
                success = response.Success,
                reply = response.Reply,
                error = response.Error,
                timestamp = response.Timestamp
            });
        }

        /// <summary>
        /// 將 ServiceStatus 轉換為 ViewBag 設定
        /// </summary>
        public static void ToViewBag(this ServiceStatus status, Controller controller)
        {
            controller.ViewBag.ServiceAvailable = status.IsAvailable;
            controller.ViewBag.ErrorMessage = status.ErrorMessage;
            controller.ViewBag.SuccessMessage = status.Message;
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        public static IActionResult ToErrorResponse(this string errorMessage)
        {
            return new JsonResult(new
            {
                success = false,
                reply = (string?)null,
                error = errorMessage,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        /// <summary>
        /// 驗證 ChatRequest 並回傳錯誤或 null
        /// </summary>
        public static IActionResult? ValidateRequest(this ChatRequest? request)
        {
            if (request == null)
                return "請求不能為空".ToErrorResponse();
                
            if (string.IsNullOrEmpty(request.Message))
                return "訊息不能為空".ToErrorResponse();
                
            return null; // 驗證通過
        }
    }
}