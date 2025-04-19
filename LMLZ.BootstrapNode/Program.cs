using FluentMigrator.Runner;
using LMLZ.BootstrapNode.Jobs;
using LMLZ.BootstrapNode.Middleware;
using LMLZ.BootstrapNode.Repository;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/lmlz-boot.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(r => r.AddSQLite()
                                       .WithGlobalConnectionString(builder.Configuration["ConnectionString"])
                                       .ScanIn(AppDomain.CurrentDomain.GetAssemblies()).For.Migrations());

builder.Services.AddScoped<ExceptionHandlingMiddleware>();
builder.Services.AddScoped<IPeerRepository, PeerRepository>();
builder.Services.AddHostedService<DeadPeerCleanerJob>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Migrate database
var scope = app.Services.CreateScope();
var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
migrator.MigrateUp();

app.MapControllers();
await app.RunAsync();