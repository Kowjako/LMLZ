using System.Xml.Linq;
using Dapper;
using LMLZ.Node.Entity;
using Microsoft.Data.Sqlite;
using Serilog;

namespace LMLZ.Node.DataAccess.Repository;

public interface IWalletRepository
{
    Task SetNameAsync(int walletId, string name);
    Task SetPrivateProtectedKeyAsync(int walletId, string privateKeyProtected);
    Task AddWalletAsync(Wallet wallet);
    Task<IEnumerable<Wallet>> GetWalletsAsync();
    Task<Wallet?> GetWalletByNameAsync(string name);
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

    public Task<Wallet?> GetWalletByNameAsync(string name)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.QueryFirstOrDefaultAsync<Wallet>("SELECT * FROM Wallets WHERE Name = @Name", new 
        {
            Name = name 
        });
    }

    public Task<IEnumerable<Wallet>> GetWalletsAsync()
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.QueryAsync<Wallet>("SELECT * FROM Wallets");
    }

    public Task SetNameAsync(int walletId, string name)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.ExecuteAsync("UPDATE Wallets SET Name = @Name WHERE Id = @Id", new
        {
            Name = name,
            Id = walletId
        });
    }

    public Task SetPrivateProtectedKeyAsync(int walletId, string privateKeyProtected)
    {
        using var conn = new SqliteConnection(_configuration["ConnectionString"]);
        return conn.ExecuteAsync("UPDATE Wallets SET PrivateKeyProtected = @PrivateKeyProtected WHERE Id = @Id", new
        {
            PrivateKeyProtected = privateKeyProtected,
            Id = walletId
        });
    }
}
