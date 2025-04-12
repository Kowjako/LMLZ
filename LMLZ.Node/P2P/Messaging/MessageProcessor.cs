using LMLZ.Node.Entity;
using LMLZ.Node.P2P.Messaging.Handlers;
using Serilog;

namespace LMLZ.Node.P2P.Messaging;

public class MessageProcessor
{
    // For processing single peer messages sequentially, not static because we need to lock per peer, not globally
    private readonly SemaphoreSlim _locker = new(1, 1);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly Serilog.ILogger _logger = Log.ForContext<MessageProcessor>();

    public MessageProcessor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ProcessMessage(Peer p, string msgType, string payload)
    {
        try
        {
            _logger.Information($"Handling message with type = {msgType} from {p}");

            await _locker.WaitAsync();
            var scope = _serviceScopeFactory.CreateScope();
            var invoker = scope.ServiceProvider.GetKeyedService<AbstractHandler>(msgType.ToLower());
            await invoker?.HandleMessageAsync(p, msgType, payload)!;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error acquiring lock or during message processing");
        }
        finally
        {
            _locker.Release();
        }
    }
}
