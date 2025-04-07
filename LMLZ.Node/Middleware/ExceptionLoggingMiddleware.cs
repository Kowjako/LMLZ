using System.Net;
using LMLZ.Node.Exceptions;
using Serilog;

namespace LMLZ.Node.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly Serilog.ILogger _logger = Log.ForContext<ExceptionHandlingMiddleware>();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.Error(ex, ex.Message);
            context.Response.StatusCode = 404;
        }
        catch (ValidationException ex)
        {
            _logger.Error(ex, ex.Message);
            context.Response.StatusCode = 400;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}