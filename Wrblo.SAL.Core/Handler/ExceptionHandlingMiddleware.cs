using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Wrblo.SAL.Core.Exception;
using Wrblo.SAL.Shared.ViewModels;

namespace Wrblo.SAL.Core.Handler;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        int statusCode;
        string message;
        int errorCode;

        switch (exception)
        {
            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                message = exception.Message;
                errorCode = ApiResponseCode.NotFound;
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                message = "Unauthorized access.";
                errorCode = ApiResponseCode.Unauthorized;
                break;

            case ValidationException validationEx:
                statusCode = StatusCodes.Status400BadRequest;
                message = validationEx.Message;
                errorCode = ApiResponseCode.BadRequest;
                break;

            case SqlException sqlEx:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "A database error occurred.";
                errorCode = ApiResponseCode.Error;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                errorCode = ApiResponseCode.Error;
                break;
        }

        var response = new
        {
            message,
            errorCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}