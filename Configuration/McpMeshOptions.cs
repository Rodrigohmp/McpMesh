using System.Collections.Generic;
using System.Net.Http;

namespace McpMesh.Configuration;

public class McpMeshOptions
{
    public List<McpMeshPackage> Packages { get; set; } = new();
    public AuthenticationOptions Authentication { get; set; } = new();
    public List<McpServerOptions> Servers { get; set; } = new();
    public EncryptionOptions Encryption { get; set; } = new();
    public string BaseUrl { get; set; }
    public int StartupDelay { get; set; } = 0;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelay { get; set; } = 5;
}

public class EncryptionOptions
{
    public string Key { get; set; }
    public string Iv { get; set; }
}

public class AuthenticationOptions
{
    public bool Enabled { get; set; } = false;
    public RequestMessage LoginRequest { get; set; }
}

public class RequestMessage
{
    public string Method { get; set; }
    public string Url { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string BodyTemplate { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
}
