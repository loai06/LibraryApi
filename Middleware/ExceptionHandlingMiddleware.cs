
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;

namespace LibraryManagementSystem.Middleware;

public class ExceptionHandlingMiddleware
{
    
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next,ILogger<ExceptionHandlingMiddleware>logger)
    {
        _next = next;
        _logger= logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Unhandeled Exeption During {Method} {Path}",
            context.Request.Method,context.Request.Path);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        int statusCode;
        string message;

        switch (exception)
        {
            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                message = exception.Message;
                break;
            case BadRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = statusCode;

       var response = ApiResponse<object>.FailResponse(message, statusCode);
      
        await context.Response.WriteAsJsonAsync(response);
    }

}