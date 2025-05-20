using System.Net;
using System.Text.Json.Serialization;

namespace ProjectStructure.Share.DTOs;

/// <summary>
/// API回傳訊息類別
/// </summary>
public class ApiResult
{
    public ApiResult()
    {
        IsSuccess = true;
        StatusCode = HttpStatusCode.OK;
    }

    public ApiResult(HttpStatusCode statusCode, object? result = null, bool isSuccess = true, string? error = null)
    {
        StatusCode = statusCode;
        Result = result;
        Error = error;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// 執行狀態
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Http Status Code
    /// </summary>
    [JsonIgnore] // 不序列化這個屬性
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Http Status Code 值
    /// </summary>
    public int Code => (int)StatusCode;

    /// <summary>
    /// 存放結果物件
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? Error { get; set; }
}