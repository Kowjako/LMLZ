using System.Net.Sockets;
using LMLZ.Node.DataAccess.Repository;
using LMLZ.Node.Entity;
using Serilog;

namespace LMLZ.Node.P2P.Jobs;

public class PeerHealthChecker : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _serviceScopeFactory; // BackgroundService is singletone, and repo is scoped
    private readonly Serilog.ILogger _logger = Log.ForContext<PeerHealthChecker>();

    public PeerHealthChecker(IServiceScopeFactory serviceScopeFacotry)
    {
        _serviceScopeFactory = serviceScopeFacotry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var peersToRemove = new List<int>();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var peerCacheRepository = scope.ServiceProvider.GetRequiredService<IPeerCacheRepository>();

            var peers = await peerCacheRepository.GetAllLiveAsync(100);
            foreach (var peer in peers)
            {
                var isHealthy = await CheckPeerHealthAsync(peer);

                if (!isHealthy)
                {
                    peersToRemove.Add(peer.Id);
                    _logger.Information($"Peer {peer.IP}:{peer.Port} is unhealthy, removing from cache");
                }
            }

            await peerCacheRepository.RemoveUnhealthyPeers(peersToRemove);
            peersToRemove.Clear();

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task<bool> CheckPeerHealthAsync(Peer peer)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5 seconds timeout
            await tcpClient.ConnectAsync(peer.IP, peer.Port, cts.Token);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
