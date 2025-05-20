using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProjectStructure.Share.DTOs;

namespace ProjectStructure.Share.Extensions
{
    /// <summary>
    /// 控制器擴展方法
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// 將 ApiResult 轉換為 IActionResult
        /// </summary>
        /// <param name="controller">控制器</param>
        /// <param name="apiResult">API 結果</param>
        /// <returns>對應的 IActionResult</returns>
        public static IActionResult ApiResponse(this ControllerBase controller, ApiResult apiResult)
        {
            return new ObjectResult(apiResult)
            {
                StatusCode = (int)apiResult.StatusCode
            };
        }

        /// <summary>
        /// 創建成功的 ApiResult 並轉換為 IActionResult
        /// </summary>
        /// <param name="controller">控制器</param>
        /// <param name="data">結果數據</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>對應的 IActionResult</returns>
        public static IActionResult ApiSuccess(this ControllerBase controller, object? data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var apiResult = new ApiResult(statusCode, data);
            return controller.ApiResponse(apiResult);
        }

        /// <summary>
        /// 創建失敗的 ApiResult 並轉換為 IActionResult
        /// </summary>
        /// <param name="controller">控制器</param>
        /// <param name="error">錯誤訊息</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>對應的 IActionResult</returns>
        public static IActionResult ApiError(this ControllerBase controller, string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var apiResult = new ApiResult(statusCode, null, false, error);
            return controller.ApiResponse(apiResult);
        }
    }
}
