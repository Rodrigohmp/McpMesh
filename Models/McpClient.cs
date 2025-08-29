using System.Collections.Generic;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpMesh.Models;

public class McpClient
{
    public IMcpClient Client { get; set; }
    public List<Tool> Tools { get; set; } = new();
}