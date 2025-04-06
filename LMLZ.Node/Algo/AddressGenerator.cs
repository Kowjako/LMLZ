using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Algo;

public static class AddressGenerator
{
    public static string GenerateWalletAddress(string publicKey)
    {
        using var sha = SHA256.Create();

        var shaHash = sha.ComputeHash(Convert.FromBase64String(publicKey));
        var versionByte = 0x01;
        var addressBytes = new byte[shaHash.Length + 1];
        addressBytes[0] = (byte)versionByte; // insert version as first byte

        Buffer.BlockCopy(shaHash, 0, addressBytes, 1, shaHash.Length);

        // Generate network prefix
        var prefixBytes = Encoding.UTF8.GetBytes("0xL");
        var prefixAddress = new byte[prefixBytes.Length + addressBytes.Length];
        Buffer.BlockCopy(prefixBytes, 0, prefixAddress, 0, prefixBytes.Length);
        Buffer.BlockCopy(addressBytes, 0, prefixAddress, prefixBytes.Length, addressBytes.Length);

        // Generate checksum
        var checkSum = GetChecksum(prefixAddress);

        // Append checksum to address
        var addressWithChecksum = new byte[prefixAddress.Length + checkSum.Length];
        Buffer.BlockCopy(prefixAddress, 0, addressWithChecksum, 0, prefixAddress.Length);
        Buffer.BlockCopy(checkSum, 0, addressWithChecksum, prefixAddress.Length, checkSum.Length);

        return Convert.ToBase64String(addressWithChecksum);
    }

    private static byte[] GetChecksum(byte[] data)
    {
        using var sha256 = SHA256.Create();

        var firstHash = sha256.ComputeHash(data);
        var secondHash = sha256.ComputeHash(firstHash);
        var checksum = new byte[4];
        Buffer.BlockCopy(secondHash, 0, checksum, 0, 4);
        return checksum;
    }
}
