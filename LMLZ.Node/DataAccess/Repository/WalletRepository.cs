using Dapper;
using LMLZ.Node.Entity;
using Microsoft.Data.Sqlite;
using Serilog;

namespace LMLZ.Node.DataAccess.Repository;

public interface IWalletRepository
{
    Task AddWalletAsync(Wallet wallet);
    Task<IEnumerable<Wallet>> GetWalletsAsync();
}

public class WalletRepository : IWalletRepository
{
    private readonly Serilog.ILogger _logger = Log.ForContext<WalletRepository>();
    private readonly IConfiguration _configuration;

    public WalletRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task AddWalletAsync(Wallet wallet)
    {
        _logger.Information($"Creating new wallet with public key {wallet.PublicKey}");

        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        var cmd = @"INSERT INTO Wallets (Address, Name, PublicKey, PrivateKeyProtected)" +
                  @"VALUES (@Address, @Name, @PublicKey, @PrivateKeyProtected)";

        await conn.ExecuteAsync(cmd, wallet);

        _logger.Information($"Wallet with public key {wallet.PublicKey} created");
    }

    public Task<IEnumerable<Wallet>> GetWalletsAsync()
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.QueryAsync<Wallet>("SELECT * FROM Wallets");
    }
}
