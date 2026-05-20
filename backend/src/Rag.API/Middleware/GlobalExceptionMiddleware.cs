using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Rag.Application.Exceptions;

namespace Rag.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        var status = ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var details = new ProblemDetails
        {
            Status = status,
            Title = ex.GetType().Name,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }
}
