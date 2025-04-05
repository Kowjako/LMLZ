using Serilog;

namespace LMLZ.Node.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder appBuilder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/lmlz.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        if (appBuilder.Environment.IsDevelopment())
        {
            // ASP.NET Pipeline Core Logs
            appBuilder.Logging.AddSerilog();
        }
    }
}
