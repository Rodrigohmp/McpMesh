# McpMesh

A .NET 9 service for aggregating multiple Model Context Protocol (MCP) servers.

## Features

- **Multi-MCP Aggregation**: Connect to multiple MCP servers simultaneously
- **MCP Protocol Support**: Exposes MCP protocol endpoints using ModelContextProtocol.AspNetCore
- **Flexible Configuration**: Configure MCP servers via appsettings or Kubernetes ConfigMaps  
- **Health Monitoring**: Built-in health endpoints for container orchestration
- **Containerized**: Docker support with Kubernetes deployment manifests

## Quick Start

### Prerequisites

- .NET 9 SDK
- Node.js and Python (for MCP servers that require them)

### Local Development

1. Clone the repository:
```bash
git clone https://github.com/dmatvienco/McpMesh.git
cd McpMesh
```

2. Run the application:
```bash
dotnet run
```

3. The service will start on `http://localhost:5293`

## Configuration

Configure MCP servers in your `appsettings.Development.json`:

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

## Endpoints

### Health Endpoints
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe  
- `GET /health/startup` - Startup probe

### MCP Protocol Endpoints
- MCP protocol endpoints are available at `/{packageId}` 
- Supports `tools/list` and `tools/call` MCP operations

## Docker Deployment

```bash
docker build -t mcpmesh .
docker run -p 5293:80 mcpmesh
```

## Kubernetes Deployment

```bash
kubectl apply -f deployment/namespace.yaml
kubectl apply -f deployment/configmap.yaml
kubectl apply -f deployment/deployment.yaml
kubectl apply -f deployment/service.yaml
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - see LICENSE file for details.