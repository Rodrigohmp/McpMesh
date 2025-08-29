using System.Collections.Generic;

namespace McpMesh.Configuration;

public class McpServerOptions
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "stdio";
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = new();
    public Dictionary<string, string> Environment { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public int TimeoutMs { get; set; } = 30000;
    
    // Tool disambiguation options
    public string ToolPrefix { get; set; } = string.Empty;
    public string DescriptionSuffix { get; set; } = string.Empty;
    public string ServerContext { get; set; } = string.Empty;
    public Dictionary<string, string> ToolOverrides { get; set; } = new();
}