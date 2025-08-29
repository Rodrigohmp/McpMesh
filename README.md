# McpMesh

A .NET 8 service that aggregates and exposes tools from multiple Model Context Protocol (MCP) servers through HTTP REST API, making them accessible to Claude Desktop and other MCP clients.

## Features

- **Multi-MCP Aggregation**: Connect to multiple MCP servers simultaneously
- **HTTP REST API**: Expose MCP tools through standard REST endpoints
- **Claude Desktop Integration**: Compatible with Claude Desktop as an MCP connector
- **Flexible Configuration**: Configure MCP servers via appsettings or Kubernetes ConfigMaps
- **Health Monitoring**: Built-in health checks and status monitoring for each MCP server
- **Authentication**: Optional authentication support for secure access
- **Containerized**: Docker support with Kubernetes deployment manifests

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js (for MCP servers that require it)
- Docker (optional, for containerized deployment)

### Local Development

1. Clone the repository:
```bash
git clone https://github.com/dmatvienco/McpMesh.git
cd McpMesh
```

2. Configure MCP servers in `appsettings.Development.json`:
```json
{
  "McpMeshOptions": {
    "Servers": [
      {
        "Id": "everything-server",
        "Name": "MCP Everything Server",
        "Type": "stdio",
        "Command": "npx",
        "Args": ["@modelcontextprotocol/server-everything"],
        "Enabled": true,
        "TimeoutMs": 30000
      },
      {
        "Id": "time-server",
        "Name": "Time MCP Server",
        "Type": "stdio",
        "Command": "uvx",
        "Args": ["mcp-server-time"],
        "Enabled": true,
        "TimeoutMs": 30000
      }
    ]
  }
}
```

3. Run the application:
```bash
dotnet run
```

4. Access the API at `http://localhost:5293`

## Configuration

### MCP Server Configuration

Configure MCP servers in your `appsettings.json`:

```json
{
  "McpMeshOptions": {
    "Packages": [
      {
        "Id": "example-package",
        "Servers": ["everything-server", "time-server"],
        "Enabled": true
      }
    ],
    "Servers": [
      {
        "Id": "everything-server",
        "Name": "MCP Everything Server",
        "Type": "stdio",
        "Command": "npx",
        "Args": ["@modelcontextprotocol/server-everything"],
        "Environment": {},
        "Enabled": true,
        "TimeoutMs": 30000
      },
      {
        "Id": "time-server",
        "Name": "Time MCP Server",
        "Type": "stdio",
        "Command": "uvx",
        "Args": ["mcp-server-time"],
        "Environment": {},
        "Enabled": true,
        "TimeoutMs": 30000
      }
    ]
  }
}
```

### Authentication (Optional)

Enable authentication by configuring the authentication section:

```json
{
  "McpMeshOptions": {
    "Authentication": {
      "Enabled": true,
      "LoginRequest": {
        "Url": "https://your-auth-server.com/api/auth/login",
        "Method": "POST",
        "BodyTemplate": "{\"username\": \"{username}\",\"password\": \"{password}\"}",
        "ContentType": "application/json"
      }
    }
  }
}
```

## API Endpoints

### Tool Access (Internal API)
- `GET /api/tools` - List all aggregated tools
- `GET /api/{mcpId}/tools` - List tools from specific MCP server
- `POST /api/tools/{toolId}/invoke` - Invoke a specific tool

### MCP Protocol Endpoints (Claude Desktop Integration)
- `POST /mcp/tools/list` - List available tools (MCP protocol)
- `POST /mcp/resources/list` - List available resources (MCP protocol)  
- `POST /mcp/tools/call` - Call a tool (MCP protocol)
- `POST /mcp/resources/read` - Read a resource (MCP protocol)

### Status and Monitoring
- `GET /api/status` - Get status of all MCP servers
- `GET /api/status/{mcpId}` - Get detailed status of specific MCP server
- `GET /health` - Health check endpoint

### Authentication
- `POST /api/auth/token` - Generate authentication token

## Docker Deployment

1. Build the Docker image:
```bash
docker build -t mcpmesh .
```

2. Run with Docker:
```bash
docker run -p 5293:80 mcpmesh
```

## Kubernetes Deployment

Deploy to Kubernetes using the provided manifests:

```bash
kubectl apply -f deployment/namespace.yaml
kubectl apply -f deployment/configmap.yaml
kubectl apply -f deployment/deployment.yaml
kubectl apply -f deployment/service.yaml
```

## Claude Desktop Integration

To use McpMesh with Claude Desktop, add it as an MCP server in your Claude Desktop configuration:

```json
{
  "mcpServers": {
    "mcpmesh": {
      "command": "curl",
      "args": ["-X", "POST", "http://localhost:5293/mcp/tools/list"]
    }
  }
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Support

- GitHub Issues: https://github.com/dmatvienco/McpMesh/issues
- Documentation: https://github.com/dmatvienco/McpMesh/wiki