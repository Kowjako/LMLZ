namespace LMLZ.Node.Dto.Input;

public record ImportWalletDto (IFormFile PrivateKeyPem, string WalletName, string Password);
