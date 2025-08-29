# McpMesh

A .NET 9 service for aggregating multiple Model Context Protocol (MCP) servers.

## Features

- **Multi-MCP Aggregation**: Connect to multiple MCP servers simultaneously
- **MCP Protocol Support**: Exposes MCP protocol endpoints using ModelContextProtocol.AspNetCore
- **Flexible Configuration**: Configure MCP servers via appsettings or Kubernetes ConfigMaps  
- **Health Monitoring**: Built-in health endpoints for container orchestration
- **Containerized**: Docker support with Kubernetes deployment manifests

## Quick Start with Docker

The easiest way to run McpMesh is with Docker, which includes all necessary dependencies:

1. Clone the repository:
```bash
git clone https://github.com/dmatvienco/McpMesh.git
cd McpMesh
```

2. Build and run with Docker:
```bash
docker build -t mcpmesh .
docker run -p 5293:80 mcpmesh
```

3. The service will be available at `http://localhost:5293`

### Local Development

For development without Docker, you'll need:
- .NET 9 SDK
- Node.js and Python (for MCP servers)

```bash
dotnet run
```

## Configuration

Configure MCP servers in your `appsettings.json`. The Docker image includes all necessary dependencies to run the example MCP servers.

```json
{
  "McpMeshOptions": {
    "Packages": [
      {
        "Enabled": true,
        "Id": "example-package",
        "Servers": ["everything-server", "time-server"]
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

The default configuration includes two example MCP servers:
- **everything-server**: 11 demo tools (echo, add, longRunningOperation, etc.)  
- **time-server**: 2 time-related tools (get_current_time, convert_time)

## Endpoints

### Health Endpoints
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe  
- `GET /health/startup` - Startup probe

### MCP Protocol Endpoints
- MCP protocol endpoints are available at `/{packageId}` 
- Supports `tools/list` and `tools/call` MCP operations

## Testing

Test the MCP protocol with curl:

```bash
# Test health
curl http://localhost:5293/health/live

# Test MCP tools/list (should return 13 tools from both servers)
curl -X POST http://localhost:5293/example-package \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":"1","method":"tools/list"}'

# Test calling a tool (echo example)
curl -X POST http://localhost:5293/example-package \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":"2","method":"tools/call","params":{"name":"echo","arguments":{"message":"Hello McpMesh!"}}}'
```

For a more interactive experience, you can use the [MCP Inspector](https://modelcontextprotocol.io/docs/tools/inspector) tool, which provides a web interface to explore and test MCP servers. The Inspector can connect to McpMesh and allows you to browse available tools, examine their schemas, and execute them with a user-friendly interface.

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

## Future Plans

- **OAuth Integration**: Add support for OAuth providers (GitHub, Google, etc.) for secure authentication
- **Claude Desktop Integration**: Native integration with Claude Desktop as an MCP connector
- **Web UI**: Management interface for configuring MCP servers and monitoring status

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - see LICENSE file for details.
