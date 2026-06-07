using System.Net;
using System.Text.Json;
using TaskManagement.Application.Errors;
using TaskManagement.Application.Exceptions;
using UnauthorizedException = TaskManagement.Application.Exceptions.UnauthorizedException;

namespace TaskManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedException ex)
        {
            await HandleUnauthorizedAsync(context, ex);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleSystemExceptionAsync(context, ex);
        }
    }

    private static async Task HandleUnauthorizedAsync(HttpContext context, UnauthorizedException exception)
    {
        var error = new ApiError
        {
            Status = StatusCodes.Status401Unauthorized,
            Code = exception.ErrorCode,
            Message = exception.Message,
            TraceId = context.TraceIdentifier
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }

    private static async Task HandleBusinessExceptionAsync(
        HttpContext context,
        BusinessException exception)
    {
        var (status, code) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Code),
            ConflictException => (HttpStatusCode.Conflict, exception.Code),
            ValidationException => (HttpStatusCode.BadRequest, exception.Code),
            _ => (HttpStatusCode.BadRequest, "BusinessError")
        };

        var error = new ApiError
        {
            Status = (int)status,
            Code = code,
            Message = exception.Message,
            TraceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }

    private static async Task HandleSystemExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var isDev = context.RequestServices
            .GetRequiredService<IHostEnvironment>().IsDevelopment();

        var error = new ApiError
        {
            Status = StatusCodes.Status500InternalServerError,
            Code = "InternalServerError",
            Message = isDev ? $"{exception.GetType().Name}: {exception.Message}" : "An unexpected error occurred.",
            TraceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
