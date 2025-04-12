using LMLZ.Node.P2P;
using LMLZ.Node.P2P.Messaging;
using LMLZ.Node.P2P.Messaging.Handlers;

namespace LMLZ.Node.Extensions;

public static class P2PServicesExtension
{
    public static IServiceCollection AddP2PDependencies(this IServiceCollection services)
    {
        services.AddScoped<MessageProcessor>();
        services.AddSingleton<ITcpListenerService, TcpListenerService>();
        services.AddSingleton<OutboundP2PManager>();

        services.AddKeyedScoped<AbstractHandler, HandshakeHandler>("handshake");
        services.AddKeyedScoped<AbstractHandler, NewBlockHandler>("new_block");
        services.AddKeyedScoped<AbstractHandler, NewTransactionHandler>("new_tx");
        services.AddKeyedScoped<AbstractHandler, RequestBlockHandler>("request_block");

        return services;
    }
}
