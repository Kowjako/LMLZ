using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Algo;

public static class AesEncrypter
{
    public static string EncryptPrivateKey(byte[] privateKeyPkcs1, string passphrase)
    {
        using var aesAlg = Aes.Create();
        aesAlg.GenerateIV();
        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        var encryptedBytes = encryptor.TransformFinalBlock(privateKeyPkcs1, 0, privateKeyPkcs1.Length);

        var combined = new byte[aesAlg.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aesAlg.IV, 0, combined, 0, aesAlg.IV.Length); // copy init vector
        Buffer.BlockCopy(encryptedBytes, 0, combined, aesAlg.IV.Length, encryptedBytes.Length); // copy key

        return Convert.ToBase64String(encryptedBytes);
    }

    public static byte[] DecryptPrivateKey(string encryptedPrivateKey, string passphrase)
    {
        var combined = Convert.FromBase64String(encryptedPrivateKey);

        using var aesAlg = Aes.Create();
        var iv = new byte[16];

        Buffer.BlockCopy(combined, 0, iv, 0, iv.Length); // grab init vector

        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = iv;

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        var encryptedBytes = new byte[combined.Length - iv.Length];

        Buffer.BlockCopy(combined, iv.Length, encryptedBytes, 0, encryptedBytes.Length); // copy key
        return decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
    }
}
