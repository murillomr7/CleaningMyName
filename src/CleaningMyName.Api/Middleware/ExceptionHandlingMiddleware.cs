using System.Net;
using System.Text.Json;
using CleaningMyName.Api.Models.Responses;
using CleaningMyName.Application.Common.Exceptions;

namespace CleaningMyName.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        _logger.LogError(exception, "An unhandled exception occurred");

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        var details = _environment.IsDevelopment() ? exception.StackTrace : null;

        if (exception is ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = "Validation error";
            
            var validationDetails = new Dictionary<string, string[]>();
            
            foreach (var error in validationException.Errors)
            {
                validationDetails.Add(error.Key, error.Value);
            }
            
            var response = new
            {
                success = false,
                message,
                errors = validationDetails,
                details = details
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }
        else if (exception is NotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
            message = exception.Message;
        }
        else if (exception is ForbiddenAccessException)
        {
            statusCode = HttpStatusCode.Forbidden;
            message = exception.Message;
        }
        else if (exception is Application.Common.Exceptions.ApplicationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        var errorResponse = new
        {
            success = false,
            message,
            details = details
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
