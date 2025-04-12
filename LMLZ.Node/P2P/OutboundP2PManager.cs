using System.Net.Sockets;
using System.Text;
using LMLZ.Node.DataAccess.Repository;
using LMLZ.Node.Entity;
using Serilog;

namespace LMLZ.Node.P2P;

public class OutboundP2PManager
{
    // long-live connections for outbound peers
    public static List<TcpClient> OutboundPeers = new List<TcpClient>();

    private readonly Serilog.ILogger _logger = Log.ForContext<OutboundP2PManager>();
    private readonly IPeerCacheRepository _peerCacheRepository;

    private int _maxOutboundConnections;

    public OutboundP2PManager(IConfiguration configuration, IPeerCacheRepository peerCacheRepository)
    {
        _maxOutboundConnections = configuration.GetValue<int>("MaxOutboundConnections");
        _peerCacheRepository = peerCacheRepository;
    }

    public async Task ConnectToPeersAsync()
    {
        var peers = await _peerCacheRepository.GetAllLiveAsync(_maxOutboundConnections);

        foreach (var peer in peers)
        {
            try
            {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(peer.IP, peer.Port);

                OutboundPeers.Add(tcpClient);

                _logger.Information($"Connected to peer {peer.IP}:{peer.Port}");
            }
            catch (SocketException ex)
            {
                _logger.Error(ex, "SocketException: Failed to connect to peer {IP}:{Port}", peer.IP, peer.Port);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(ex, "TimeoutException: Failed to connect to peer {IP}:{Port}", peer.IP, peer.Port);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to connect to peer {IP}:{Port}", peer.IP, peer.Port);
            }
        }
    }

    public async Task BroadcastMessageAsync()
    {
        // Compose message

        // Broadcast
        var tasks = OutboundPeers.Select(client =>
        {
           var stream = client.GetStream();
           var message = Encoding.UTF8.GetBytes("Hello from LMLZ Node");
           return stream.WriteAsync(message, 0, message.Length);
        });

        await Task.WhenAll(tasks);
    }

    public Task SendMessageAsync(Peer peer, string msgType, string payload)
    {
        throw new NotImplementedException();
    }
}
