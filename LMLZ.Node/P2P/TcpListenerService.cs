using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LMLZ.Node.Entity;
using LMLZ.Node.P2P.Messaging;
using Serilog;

namespace LMLZ.Node.P2P;

public interface ITcpListenerService
{
    Task StartListeningAsync(CancellationToken token);
    void StopListening();
}

public class TcpListenerService : ITcpListenerService
{
    private readonly Serilog.ILogger _logger = Log.ForContext<TcpListenerService>();
    private readonly TcpListener _tcpListener;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private bool _isRunning;
    private int _activeConnections;
    private int _maxConnections;

    public TcpListenerService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _maxConnections = configuration.GetValue<int>("MaxInboundConnections");
        _tcpListener = new TcpListener(IPAddress.Any, configuration.GetValue<int>("P2PListenerPort"));

        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartListeningAsync(CancellationToken token)
    {
        _tcpListener.Start();
        _isRunning = true;

        _logger.Information("P2P Listener started");

        while (_isRunning && !token.IsCancellationRequested)
        {
            if (_activeConnections < _maxConnections)
            {
                var client = await _tcpListener.AcceptTcpClientAsync(token);
                Interlocked.Increment(ref _activeConnections); // thread-safe inc
                _ = Task.Run(() => HandleClientAsync(client)); // fire-forget each client in separate thread
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            while (client is not null && client.Connected && client.GetStream().CanRead)
            {
                var stream = client.GetStream();

                var p2pMessageLength = await ReadMessageLength(stream);
                var message = await ReadMessageBody(stream, p2pMessageLength);

                if (message is null) continue;

                if (!message.RootElement.TryGetProperty("type", out var type)) continue;

                _ = message.RootElement.TryGetProperty("replyPort", out var port);
                _ = message.RootElement.TryGetProperty("payload", out var payload);
                
                var callerIp = client?.Client?.RemoteEndPoint as IPEndPoint;
                var callerPeer = new Peer(0, callerIp!.Address.ToString(), port.GetInt32());

                using var scope = _serviceScopeFactory.CreateScope();
                var _messageProcessor = scope.ServiceProvider.GetRequiredService<MessageProcessor>();

                await _messageProcessor.ProcessMessage(callerPeer, type.GetString()!, payload.GetString()!);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error handling tcp client or peer disconnected: {ex.Message}");
        }
        finally
        {
            client.Close();
            Interlocked.Decrement(ref _activeConnections); // thread-safe dec
        }
    }

    private async Task<JsonDocument?> ReadMessageBody(NetworkStream stream, int p2pMessageLength)
    {
        if (p2pMessageLength is 0) return null;

        var p2pMessageBuffer = new byte[p2pMessageLength];

        var totalReadBytes = 0;
        while (totalReadBytes < p2pMessageLength)
        {
            var readBytes = await stream.ReadAsync(p2pMessageBuffer, totalReadBytes, p2pMessageLength - totalReadBytes);
            if (readBytes == 0)
            {
                _logger.Information("Nothing to read.. peer disconnected");
                throw new Exception("Peer is closed");
            }
            totalReadBytes += readBytes;
        }

        var message = Encoding.UTF8.GetString(p2pMessageBuffer);
        return JsonDocument.Parse(message);
    }

    private async Task<int> ReadMessageLength(NetworkStream stream)
    {
        var p2pMessageLengthHeader = new byte[4];
        var readBytes = 0;

        // Read first 4 bytes to get msg length
        // rest of bytes will stay in network stream buffer
        while (readBytes < p2pMessageLengthHeader.Length)
        {
            var bytes= await stream.ReadAsync(p2pMessageLengthHeader,
                readBytes, p2pMessageLengthHeader.Length - readBytes);
            
            if (bytes is 0)
            {
                _logger.Information("Nothing to read.. peer disconnected");
                throw new Exception("Peer is closed");
            }

            readBytes += bytes;
        }

        return BitConverter.ToInt32(p2pMessageLengthHeader, 0);
    }

    public void StopListening()
    {
        _isRunning = false;
        _tcpListener.Stop();
    }
}