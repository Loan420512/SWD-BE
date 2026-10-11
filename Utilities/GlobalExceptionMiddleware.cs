using System.Net;
using System.Text.Json;

namespace StickerRobot.Utilities;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message;
            _logger.LogError(ex, "An unhandled exception occurred: {Message}. InnerException: {InnerMessage}", ex.Message, innerMsg);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var fullErrorMessage = exception.InnerException != null
            ? $"{exception.Message} (Inner: {exception.InnerException.Message})"
            : exception.Message;

        var response = ApiResponse<object>.ErrorResponse(
            "An error occurred while processing your request.",
            new { detail = fullErrorMessage }
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(json);
    }
}
