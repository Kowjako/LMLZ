using LMLZ.BootstrapNode.Repository;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LMLZ.BootstrapNode.Jobs;

public class DeadPeerCleanerJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger = Log.ForContext<DeadPeerCleanerJob>();

    public DeadPeerCleanerJob(IServiceScopeFactory serviceScopeFactory)
        => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var peerRepository = scope.ServiceProvider.GetRequiredService<IPeerRepository>();
            var thresholdTime = DateTime.UtcNow - TimeSpan.FromHours(1);

            await peerRepository.RemoveDeadPeersBasedOnThreshold(thresholdTime);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}