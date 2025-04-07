using System.Security.Cryptography;
using System.Text;

namespace LMLZ.Node.Algo;

public static class AesEncrypter
{
    public static string EncryptPrivateKey(byte[] privateKeyPkcs1, string passphrase)
    {
        using var aesAlg = Aes.Create();
        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = new byte[16]; // randomize init vector and store along private key 

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        var encryptedBytes = encryptor.TransformFinalBlock(privateKeyPkcs1, 0, privateKeyPkcs1.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static byte[] DecryptPrivateKey(string encryptedPrivateKey, string passphrase)
    {
        using var aesAlg = Aes.Create();
        var key = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("LMLZ"), 10000, HashAlgorithmName.SHA256);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = new byte[16];

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        var encryptedBytes = Convert.FromBase64String(encryptedPrivateKey);
        var privateKeyPkcs1 = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return privateKeyPkcs1;
    }
}
