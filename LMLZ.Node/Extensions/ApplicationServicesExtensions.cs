﻿using LMLZ.Node.Middleware;
using LMLZ.Node.P2P;
using LMLZ.Node.Services;

namespace LMLZ.Node.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPeerDiscoveryService, PeerDiscoveryService>();

        services.AddScoped<ExceptionHandlingMiddleware>();

        return services;
    }
}
