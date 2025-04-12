using System.Security.Cryptography;
using System.Text;
using LMLZ.Node.Entity;

namespace LMLZ.Node.Algo;

public static class RsaSignatureHandler
{
    public static string SignTransaction(Transaction t, byte[] privateKeyPkcs1)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyPkcs1, out _);

        var data = Encoding.UTF8.GetBytes($"{t.Timestamp:o}|{t.Sender}|{t.Receiver}|{t.Amount}|{t.PublicKey}");

        return Convert.ToHexString(rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    public static bool IsSignatureValid(Transaction t, byte[] publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);

        var data = Encoding.UTF8.GetBytes($"{t.Timestamp:o}|{t.Sender}|{t.Receiver}|{t.Amount}|{t.PublicKey}");
        var signature = Convert.FromHexString(t.Signature);

        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}