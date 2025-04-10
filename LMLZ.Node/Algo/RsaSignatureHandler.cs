using System.Security.Cryptography;

namespace LMLZ.Node.Algo;

public static class RsaSignatureHandler
{
    public static byte[] SignData(byte[] data, byte[] privateKeyPkcs1)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyPkcs1, out _);

        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static bool IsSignatureValid(byte[] signature, byte[] publicKey, byte[] data)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKey, out _);

        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
