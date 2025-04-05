using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Algo;

public static class AesEncrypter
{
    public static string EncryptPrivateKey(string privateKey, string passphrase)
    {
        using var aesAlg = Aes.Create();
        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = new byte[16];

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        var inputBytes = Encoding.UTF8.GetBytes(privateKey);
        var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string DecryptPrivateKey(string encryptedPrivateKey, string passphrase)
    {
        using var aesAlg = Aes.Create();
        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = new byte[16];

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        var encryptedBytes = Convert.FromBase64String(encryptedPrivateKey);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
