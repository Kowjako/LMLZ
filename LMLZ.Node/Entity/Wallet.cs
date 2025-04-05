namespace LMLZ.Node.Entity;

public record Wallet
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string PublicKey { get; init; } = null!;
    public string PrivateKeyProtected { get; init; } = null!;
}
