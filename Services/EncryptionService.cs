using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Json;
using McpMesh.Configuration;

namespace McpMesh.Services;

public interface IEncryptionService
{
    string Encrypt<T>(T obj) where T : class;
    T Decrypt<T>(string encryptedJson) where T : class;
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IOptions<McpMeshOptions> options)
    {
        _key = Convert.FromBase64String(options.Value.Encryption.Key);
        _iv = Convert.FromBase64String(options.Value.Encryption.Iv);
    }

    public string Encrypt<T>(T obj) where T : class
    {
        var json = JsonSerializer.Serialize(obj);

        var aesProvider = new AesProvider(_key, _iv);
        var bytesToEncrypt = Encoding.UTF8.GetBytes(json);
        var encryptedBytes = aesProvider.Encrypt(bytesToEncrypt);
        return Convert.ToBase64String(encryptedBytes);
    }

    public T Decrypt<T>(string encryptedJson) where T : class
    {
        var aesProvider = new AesProvider(_key, _iv);
        var encryptedBytes = Convert.FromBase64String(encryptedJson);
        var decryptedBytes = aesProvider.Decrypt(encryptedBytes);
        var json = Encoding.UTF8.GetString(decryptedBytes);

        return JsonSerializer.Deserialize<T>(json);
    }
}