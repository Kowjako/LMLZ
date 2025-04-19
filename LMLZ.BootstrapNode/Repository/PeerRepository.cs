using Dapper;
using LMLZ.BootstrapNode.Model;
using Microsoft.Data.Sqlite;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LMLZ.BootstrapNode.Repository;

public interface IPeerRepository
{
    Task<IEnumerable<PeerDto>> GetPeersRandomChunkAsync(int maxCount);
    Task RemovePeerAsync(Guid id);
    Task<Guid> AddPeerAsync(string ip, string port);
    Task UpdateLastSeenAsync(Guid id);
    Task RemoveDeadPeersBasedOnThreshold(DateTime thresholdTime);
}

public class PeerRepository : IPeerRepository
{
    private readonly ILogger _logger = Log.ForContext<PeerRepository>();
    private readonly IConfiguration _configuration;

    public PeerRepository(IConfiguration configuration)
     => _configuration = configuration;

    public async Task<Guid> AddPeerAsync(string ip, string port)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        using var tran = await conn.BeginTransactionAsync();

        try
        {
            var cmd = @"INSERT OR IGNORE INTO Peers (Id, IP, Port) VALUES (@id, @ip, @port);";
            await conn.ExecuteAsync(cmd, new { id = Guid.NewGuid(), ip, port });

            var existingId = await conn.ExecuteScalarAsync<string>(
                "SELECT Id FROM Peers WHERE IP = @ip AND Port = @port",
                new { ip, port });

            await tran.CommitAsync();

            return Guid.Parse(existingId!);
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<PeerDto>> GetPeersRandomChunkAsync(int maxCount)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return await conn.QueryAsync<PeerDto>("SELECT IP, Port FROM Peers ORDER BY RANDOM() LIMIT @Count", new { Count = maxCount });
    }

    public async Task RemoveDeadPeersBasedOnThreshold(DateTime thresholdTime)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        await conn.ExecuteAsync("DELETE FROM Peers WHERE LastSeen < @thresholdTime", new { thresholdTime });
    }

    public async Task RemovePeerAsync(Guid id)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        var cmd = "DELETE FROM Peers WHERE Id = @id";

        await conn.ExecuteAsync(cmd, new { id });
    }

    public async Task UpdateLastSeenAsync(Guid id)
    {
        _logger.Information($"Updating last seen for peer {id}");

        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        var cmd = @"UPDATE Peers SET LastSeen = @LastSeen WHERE Id = @Id";

        await conn.ExecuteAsync(cmd, new { Id = id, LastSeen = DateTime.UtcNow });
    }
}
