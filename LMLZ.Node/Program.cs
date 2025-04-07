using FluentMigrator.Runner;
using LMLZ.Node.Extensions;
using LMLZ.Node.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(1338);
});

builder.ConfigureSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddDataAccessServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Migrate database
var scope = app.Services.CreateScope();
var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
migrator.MigrateUp();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
