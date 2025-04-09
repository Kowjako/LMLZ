namespace LMLZ.Node.Dto.Input;

public record ChangeWalletPassphraseDto (string WalletName, string OldPassphrase, string NewPassphrase);