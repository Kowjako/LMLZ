using Dapper;
using LMLZ.Node.Entity;
using Microsoft.Data.Sqlite;
using Serilog;

namespace LMLZ.Node.DataAccess.Repository;

public interface IPeerCacheRepository
{
    Task<IEnumerable<Peer>> GetAllLiveAsync(int maxCount);
    Task RemoveUnhealthyPeers(List<int> ids);
    Task AddPeerAsync(Peer peer);
}

public class PeerCacheRepository : IPeerCacheRepository
{
    private readonly Serilog.ILogger _logger = Log.ForContext<PeerCacheRepository>();
    private readonly IConfiguration _configuration;

    public PeerCacheRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task AddPeerAsync(Peer peer)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);

        // Unique constrain on (Ip, Port) will omit duplicate
        var cmd = @"INSERT OR IGNORE INTO Peers (IP, Port) VALUES (@IP, @Port)";
        return conn.ExecuteAsync(cmd, peer);
    }

    public Task<IEnumerable<Peer>> GetAllLiveAsync(int maxCount)
    {
        _logger.Information($"Fetching live peers, max count = {maxCount}");

        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.QueryAsync<Peer>("SELECT FROM Peers LIMIT @MaxCount", new { MaxCount = maxCount });
    }
}
