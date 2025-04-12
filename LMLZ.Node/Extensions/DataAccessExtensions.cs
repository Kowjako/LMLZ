using FluentMigrator.Runner;
using LMLZ.Node.DataAccess.Repository;

namespace LMLZ.Node.Extensions;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddFluentMigratorCore()
                .ConfigureRunner(r => r.AddSQLite()
                                       .WithGlobalConnectionString(config["ConnectionString"])
                                       .ScanIn(AppDomain.CurrentDomain.GetAssemblies()).For.Migrations());

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IPeerCacheRepository, PeerCacheRepository>();

        return services;
    }
}
