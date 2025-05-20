using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectStructure.Share.DTOs;

namespace ProjectStructure.Share.Filter
{
    /// <summary>
    /// 全局異常過濾器
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        /// <summary>
        /// 初始化異常過濾器
        /// </summary>
        /// <param name="logger">記錄器</param>
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 處理異常
        /// </summary>
        /// <param name="context">異常上下文</param>
        public void OnException(ExceptionContext context)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // 預設為 500 錯誤
            string errorMessage = "發生了未處理的錯誤";

            // 獲取請求信息，用於日誌記錄
            var request = context.HttpContext.Request;
            var requestPath = request.Path;
            var requestMethod = request.Method;

            // 根據異常類型設置不同的 HTTP 狀態碼和錯誤消息
            switch (context.Exception)
            {
                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = argEx.Message;
                    _logger.LogWarning(argEx, "參數錯誤: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        errorMessage, requestPath, requestMethod);
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = invOpEx.Message;
                    _logger.LogWarning(invOpEx, "操作無效: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        errorMessage, requestPath, requestMethod);
                    break;

                case UnauthorizedAccessException authEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorMessage = authEx.Message ?? "未授權的訪問";
                    _logger.LogWarning(authEx, "未授權: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        errorMessage, requestPath, requestMethod);
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = keyNotFoundEx.Message;
                    _logger.LogWarning(keyNotFoundEx, "未找到: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        errorMessage, requestPath, requestMethod);
                    break;

                case NotImplementedException notImplEx:
                    statusCode = HttpStatusCode.NotImplemented;
                    errorMessage = notImplEx.Message;
                    _logger.LogWarning(notImplEx, "未實現: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        errorMessage, requestPath, requestMethod);
                    break;

                default:
                    // 對於未明確處理的異常類型，記錄詳細錯誤信息
                    _logger.LogError(context.Exception, "未處理的異常: {ErrorMessage}，路徑: {RequestPath}，方法: {RequestMethod}",
                        context.Exception.Message, requestPath, requestMethod);
                    errorMessage = context.Exception.Message;
                    break;
            }

            // 增加額外的上下文信息到日誌
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["User"] = context.HttpContext.User.Identity?.Name ?? "匿名",
                ["IPAddress"] = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "未知"
            }))
            {
                // 在範圍中記錄一條額外的細節日誌
                _logger.LogInformation("處理異常完成，將返回狀態碼：{StatusCode}", (int)statusCode);
            }

            // 創建 API 響應
            var apiResult = new ApiResult(statusCode, null, false, errorMessage);

            // 設置響應
            context.Result = new ObjectResult(apiResult)
            {
                StatusCode = (int)statusCode
            };

            // 標記異常已處理
            context.ExceptionHandled = true;
        }
    }
}