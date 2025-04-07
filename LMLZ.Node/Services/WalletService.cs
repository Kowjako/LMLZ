using System.Security.Cryptography;
using System.Text;
using LMLZ.Node.Algo;
using LMLZ.Node.DataAccess.Repository;
using LMLZ.Node.Dto.Input;
using LMLZ.Node.Dto.Output;
using LMLZ.Node.Entity;
using LMLZ.Node.Exceptions;

namespace LMLZ.Node.Services;

public interface IWalletService
{
    Task CreateNewWalletAsync(string pass);
    Task<string> ExportWalletPemAsync(string wallet, string passphrase);
    Task<IEnumerable<WalletDto>> GetWalletsAsync();
    Task ImportWalletUsingPemAsync(ImportWalletDto walletImportDto);
}

public class WalletService : IWalletService
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<WalletService>();
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task CreateNewWalletAsync(string pass)
    {
        using var rsa = new RSACryptoServiceProvider(2048);

        var privateKey = rsa.ExportRSAPrivateKey(); // PKCS#1
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()); // PKCS#1

        var encryptedPrivateKey = AesEncrypter.EncryptPrivateKey(privateKey, pass);

        var wallet = new Wallet
        {
            Address = AddressGenerator.GenerateWalletAddress(publicKey),
            PublicKey = publicKey,
            PrivateKeyProtected = encryptedPrivateKey
        };

        await _walletRepository.AddWalletAsync(wallet);
    }

    public async Task<string> ExportWalletPemAsync(string walletName, string passphrase)
    {
        var wallet = await _walletRepository.GetWalletByNameAsync(walletName);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet {walletName} not found");
        }
        
        var decryptedPrivateKey = AesEncrypter.DecryptPrivateKey(wallet.PrivateKeyProtected, passphrase);
        
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(decryptedPrivateKey, out _);

        return rsa.ExportRSAPrivateKeyPem();
    }

    public async Task<IEnumerable<WalletDto>> GetWalletsAsync()
    {
        var wallets = await _walletRepository.GetWalletsAsync();

        // calculate balances - consider appropriate index on table

        return Enumerable.Empty<WalletDto>();
    }

    public async Task ImportWalletUsingPemAsync(ImportWalletDto walletDto)
    {
        var existingWallet = await _walletRepository.GetWalletByNameAsync(walletDto.WalletName);
        if (existingWallet is not null)
        {
            throw new ValidationException($"Wallet with name {walletDto.WalletName} already exists");
        }

        using var reader = new StreamReader(walletDto.PrivateKeyPem.OpenReadStream());
        var pemContent = await reader.ReadToEndAsync();

        var base64PrivateKey = ExtractBase64FromPem(pemContent);
        var privateKeyBytes = Convert.FromBase64String(base64PrivateKey);

        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

        var encryptedPrivateKey = AesEncrypter.EncryptPrivateKey(privateKeyBytes, walletDto.Password);

        var wallet = new Wallet
        {
            Name = walletDto.WalletName,
            Address = AddressGenerator.GenerateWalletAddress(publicKey),
            PublicKey = publicKey,
            PrivateKeyProtected = encryptedPrivateKey
        };

        await _walletRepository.AddWalletAsync(wallet);
        _logger.Information($"Wallet {walletDto.WalletName} imported with public key {publicKey}");
    }

    private string ExtractBase64FromPem(string pem)
    {
        var header = "-----BEGIN RSA PRIVATE KEY-----";
        var footer = "-----END RSA PRIVATE KEY-----";

        var start = pem.IndexOf(header, StringComparison.Ordinal);
        var end = pem.IndexOf(footer, StringComparison.Ordinal);

        if (start < 0 || end < 0)
            throw new ValidationException("Invalid PEM format");

        var base64 = pem.Substring(start + header.Length, end - start - header.Length);
        return base64.Replace("\r", "").Replace("\n", "").Trim();
    }
}
