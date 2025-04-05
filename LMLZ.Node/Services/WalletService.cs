using System.Security.Cryptography;
using LMLZ.Node.Algo;
using LMLZ.Node.DataAccess.Repository;
using LMLZ.Node.Dto;
using LMLZ.Node.Entity;

namespace LMLZ.Node.Services;

public interface IWalletService
{
    Task CreateNewWalletAsync(string pass);
    Task<IEnumerable<WalletDto>> GetWalletsAsync();
}

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task CreateNewWalletAsync(string pass)
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

        var encryptedPrivateKey = AesEncrypter.EncryptPrivateKey(privateKey, pass);

        var wallet = new Wallet
        {
            Address = "0x" + publicKey.ToUpper(),
            PublicKey = publicKey,
            PrivateKeyProtected = encryptedPrivateKey
        };
    }

    public async Task<IEnumerable<WalletDto>> GetWalletsAsync()
    {
        var wallets = await _walletRepository.GetWalletsAsync();

        // calculate balances

        return Enumerable.Empty<WalletDto>();
    }
}
