using System.Net;
using Serilog;

namespace LMLZ.BootstrapNode.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly Serilog.ILogger _logger = Log.ForContext<ExceptionHandlingMiddleware>();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
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