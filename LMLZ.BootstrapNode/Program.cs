using FluentMigrator.Runner;
using LMLZ.BootstrapNode.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(r => r.AddSQLite()
                                       .WithGlobalConnectionString(builder.Configuration["ConnectionString"])
                                       .ScanIn(AppDomain.CurrentDomain.GetAssemblies()).For.Migrations());

builder.Services.AddScoped<IPeerRepository, PeerRepository>();

var app = builder.Build();

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
app.Run();
