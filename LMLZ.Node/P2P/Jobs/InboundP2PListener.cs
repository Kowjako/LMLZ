namespace LMLZ.Node.P2P.Jobs;

public class InboundP2PListener : BackgroundService
{
    private readonly ITcpListenerService _tcpListenerService;

    public InboundP2PListener(ITcpListenerService tcpListenerService)
        => _tcpListenerService = tcpListenerService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _tcpListenerService.StartListeningAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _tcpListenerService.StopListening();
        return base.StopAsync(cancellationToken);
    }
}
