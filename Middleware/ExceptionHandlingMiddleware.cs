
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Exceptions;
namespace LibraryManagementSystem.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (Exception ex)
        {
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