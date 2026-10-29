using ChatPlatform.Application.CommonResponses;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ChatPlatform.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            (int statusCode, string message) = MapException(exception);

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Never leak internals (stack trace, SQL, file paths) outside Development
            string responseMessage = statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment()
                ? "An unexpected error occurred."
                : message;

            List<string> errors = new(1) { responseMessage };

            APIResponse<string> response = APIResponse<string>.Fail(errors);
            await context.Response.WriteAsJsonAsync(response);
        }

        private static (int StatusCode, string Message) MapException(Exception exception) => exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, exception.Message)
        };

    }
}
