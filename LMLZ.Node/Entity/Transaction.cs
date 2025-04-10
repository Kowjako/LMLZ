using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Entity;

public record Transaction
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Sender { get; init; } = null!;
    public string Receiver { get; init; } = null!;
    public decimal Amount { get; init; }
    public string PublicKey { get; init; } = null!;
    public string Signature { get; init; } = null!; // digital signature
    public string? BlockHash { get; init; } // block reference

    public string Hash => ComputeHash(); // Hash as ID

    private string ComputeHash()
    {
        var rawData = $"{Timestamp:o}|{Sender}|{Receiver}|{Amount}|{PublicKey}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawData);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
