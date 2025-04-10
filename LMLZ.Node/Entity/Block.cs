using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Entity;

public record Block
{
    public long Index { get; set; } // for ordering
    public string Hash => ComputeHash();
    public string Transactions { get; init; } = null!;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public int Nonce { get; init; }
    public string MerkleRoot { get; init; } = null!;
    public string PreviousHash { get; init; } = null!;
    public int BlockSizeInMB { get; init; }
    public string Version { get; init; } = "1.0";

    private string ComputeHash()
    {
        using var sha256 = SHA256.Create();
        var blockData = $"{Index}{Timestamp}{PreviousHash}{Transactions}{MerkleRoot}{Nonce}{Version}";
        var byteData = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
        return Convert.ToHexString(byteData).ToLowerInvariant();
    }
}
