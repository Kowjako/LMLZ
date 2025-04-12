using Dapper;
using LMLZ.BootstrapNode.Model;
using Microsoft.Data.Sqlite;

namespace LMLZ.BootstrapNode.Repository;

public interface IPeerRepository
{
	Task<IEnumerable<PeerDto>> GetPeersRandomChunkAsync(int maxCount);
	Task RemovePeerAsync(Guid id);
	Task<Guid> AddPeerAsync(string ip, string port);
}

public class PeerRepository : IPeerRepository
{
	private readonly ILogger<PeerRepository> _logger;
    private readonly IConfiguration _configuration;

	public PeerRepository(ILogger<PeerRepository> logger, IConfiguration configuration)
	{
		_logger = logger;
        _configuration = configuration;
	}

    public Task<Guid> AddPeerAsync(string ip, string port)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        var cmd = "INSERT INTO Peers (IP, Port) VALUES (@ip, @port) RETURNING Id";

        return conn.ExecuteScalarAsync<Guid>(cmd, new { ip, port });
    }

    public Task<IEnumerable<PeerDto>> GetPeersRandomChunkAsync(int maxCount)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.QueryAsync<PeerDto>("SELECT * FROM Peers ORDER BY RANDOM() LIMIT @Count", new { Count = maxCount });
    }

    public Task RemovePeerAsync(Guid id)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        var cmd = "DELETE FROM Peers WHERE Id = @id";

        return conn.ExecuteAsync(cmd, new { id });
    }
}
