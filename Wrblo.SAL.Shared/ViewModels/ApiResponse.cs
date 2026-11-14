namespace Wrblo.SAL.Shared.ViewModels;

public static class ApiResponseCode
{
    public const int NotFound = 404;
    public const int BadRequest = 400;
    public const int Error = 500;
    public const int Success = 200;
    public const int Created = 201;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int Conflict = 409;

}

public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool HasErrors { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null) =>
        new() { Code = ApiResponseCode.Success, Data = data, Message = message, HasErrors = false };

    public static ApiResponse<T> Created(T data, string? message = null) =>
        new() { Code = ApiResponseCode.Created, Data = data, Message = message, HasErrors = false };

    public static ApiResponse<T> Fail(string message, int errorCode = ApiResponseCode.Error) =>
        new() { Code = errorCode, Message = message, HasErrors = true };

    public static ApiResponse<T> NotFound(string message) =>
        new() { Code = ApiResponseCode.NotFound, Message = message, HasErrors = true };

    public static ApiResponse<T> BadRequest(string message) =>
        new() { Code = ApiResponseCode.BadRequest, Message = message, HasErrors = true };

    public static ApiResponse<T> Unauthorized(string message) =>
       new() { Code = ApiResponseCode.Unauthorized, Message = message, HasErrors = true };

    public static ApiResponse<T> Forbidden(string message) =>
       new() { Code = ApiResponseCode.Forbidden, Message = message, HasErrors = true };

    public static ApiResponse<T> Conflict(string message) =>
        new() { Code = ApiResponseCode.Conflict, Message = message, HasErrors = true };
}

