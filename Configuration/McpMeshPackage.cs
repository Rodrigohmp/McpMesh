using System.Collections.Generic;

namespace McpMesh.Configuration;

public class McpMeshPackage
{
    public string Id { get; set; }
    public bool Enabled { get; set; } = true;
    public List<string> Servers { get; set; } = new();
}