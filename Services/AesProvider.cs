using System.Security.Cryptography;

namespace McpMesh.Services;

public class AesProvider
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesProvider(byte[] key, byte[] iv)
    {
        _key = key;
        _iv = iv;
    }

    public byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }
}