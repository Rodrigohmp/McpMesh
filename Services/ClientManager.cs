using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McpMesh.Configuration;
using McpMesh.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpMesh.Services;

public interface IClientManager : IAsyncDisposable
{
    Task InitializeAsync();
    Task<List<McpClient>> GetClientsAsync(string packageId);
    Task<McpClient> GetClientAsync(string packageId, string toolName);
}

public class ClientManager : IClientManager
{
    private readonly IOptions<McpMeshOptions> _options;
    private readonly ILogger<ClientManager> _logger;
    private readonly Dictionary<string, McpClient> _clients = new();
    private readonly Dictionary<string, List<string>> _packages;

    public ClientManager(IOptions<McpMeshOptions> options, ILogger<ClientManager> logger)
    {
        _options = options;
        _logger = logger;

        var packages = _options.Value.Packages;
        _packages = packages.ToDictionary(
            p => p.Id,
            p => p.Servers
        );
    }

    public async Task InitializeAsync()
    {
        var options = _options.Value;
        
        if (options.StartupDelay > 0)
        {
            _logger.LogInformation("Waiting {StartupDelay} seconds before initializing MCP clients", options.StartupDelay);
            await Task.Delay(TimeSpan.FromSeconds(options.StartupDelay));
        }

        var servers = options.Servers;
        foreach (var server in servers.Where(s => s.Enabled))
        {
            await InitializeClientWithRetryAsync(server, options.RetryAttempts, options.RetryDelay);
        }
    }

    private async Task InitializeClientAsync(McpServerOptions server)
    {
        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = server.Name,
            Command = server.Command,
            Arguments = server.Args,
            EnvironmentVariables = server.Environment
        });

        var client = await McpClientFactory.CreateAsync(clientTransport);
        var tools = await client.ListToolsAsync();

        var mcpClient = new McpClient
        {
            Client = client,
            Tools = tools.Select(t => EnhanceToolWithServerContext(t, server)).ToList()
        };
        
        _clients[server.Id] = mcpClient;

        _logger.LogInformation("Initialized server '{ServerId}' ({ServerName}) with {ToolCount} tools", 
            server.Id, server.Name, tools.Count);
        
        if (tools.Count > 0)
        {
            var toolList = string.Join(", ", tools.Select(t => t.Name));
            _logger.LogInformation("  Tools: {ToolList}", toolList);
        }
    }

    private async Task InitializeClientWithRetryAsync(McpServerOptions server, int maxRetries, int retryDelaySeconds)
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Initializing server '{ServerId}' (attempt {Attempt}/{MaxRetries})", 
                    server.Id, attempt, maxRetries);
                
                await InitializeClientAsync(server);
                return; // Success
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "Failed to initialize server '{ServerId}' after {MaxRetries} attempts", 
                        server.Id, maxRetries);
                }
                else
                {
                    _logger.LogWarning("Failed to initialize server '{ServerId}' (attempt {Attempt}/{MaxRetries}): {Error}. Retrying in {Delay} seconds...", 
                        server.Id, attempt, maxRetries, ex.Message, retryDelaySeconds);
                    
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                }
            }
        }
    }

    private async Task<bool> IsClientHealthyAsync(string serverId)
    {
        if (!_clients.TryGetValue(serverId, out var mcpClient))
        {
            return false;
        }

        try
        {
            // Try to list tools as a health check
            await mcpClient.Client.ListToolsAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ReconnectClientAsync(string serverId)
    {
        _logger.LogInformation("Attempting to reconnect client for server {ServerId}", serverId);

        // Dispose old client if exists
        if (_clients.TryGetValue(serverId, out var oldClient))
        {
            try
            {
                await oldClient.Client.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing old client for server {ServerId}", serverId);
            }
            _clients.Remove(serverId);
        }

        // Find server config and reinitialize
        var server = _options.Value.Servers.FirstOrDefault(s => s.Id == serverId && s.Enabled);
        if (server != null)
        {
            try
            {
                await InitializeClientAsync(server);
                _logger.LogInformation("Successfully reconnected client for server {ServerId}", serverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect client for server {ServerId}", serverId);
                throw;
            }
        }
    }

    public async Task<List<McpClient>> GetClientsAsync(string packageId)
    {
        if (_packages.TryGetValue(packageId, out var serverIds))
        {
            var result = new List<McpClient>();
            
            foreach (var serverId in serverIds)
            {
                if (_clients.TryGetValue(serverId, out var client))
                {
                    // Check if client is healthy, reconnect if not
                    var isHealthy = await IsClientHealthyAsync(serverId);
                    if (!isHealthy)
                    {
                        try
                        {
                            await ReconnectClientAsync(serverId);
                            if (_clients.TryGetValue(serverId, out client))
                            {
                                result.Add(client);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to reconnect client for server {ServerId}", serverId);
                        }
                    }
                    else
                    {
                        result.Add(client);
                    }
                }
            }
            
            return result;
        }

        return [];
    }

    public async Task<McpClient> GetClientAsync(string packageId, string toolName)
    {
        var clients = await GetClientsAsync(packageId);
        return clients.FirstOrDefault(c => c.Tools.Any(t => t.Name == toolName));
    }

    private Tool EnhanceToolWithServerContext(McpClientTool originalTool, McpServerOptions server)
    {
        var toolName = originalTool.Name;
        var description = originalTool.Description ?? string.Empty;

        // Apply tool-specific overrides first
        if (server.ToolOverrides.TryGetValue(toolName, out var overrideName))
        {
            toolName = overrideName;
        }
        else if (!string.IsNullOrEmpty(server.ToolPrefix))
        {
            // Add prefix if no specific override
            toolName = $"{server.ToolPrefix}_{toolName}";
        }

        // Enhance description with server context
        if (!string.IsNullOrEmpty(server.ServerContext))
        {
            if (string.IsNullOrEmpty(description))
            {
                description = $"[{server.ServerContext}] {toolName}";
            }
            else
            {
                description = $"{description} [{server.ServerContext}]";
            }
        }

        // Add description suffix if configured
        if (!string.IsNullOrEmpty(server.DescriptionSuffix))
        {
            description = string.IsNullOrEmpty(description) 
                ? server.DescriptionSuffix 
                : $"{description} {server.DescriptionSuffix}";
        }

        _logger.LogDebug("Enhanced tool '{OriginalName}' -> '{NewName}' with description: {Description}", 
            originalTool.Name, toolName, description);

        return new Tool
        {
            Name = toolName,
            Description = description,
            InputSchema = originalTool.JsonSchema
        };
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients.Values.Where(client => client?.Client != null))
        {
            try
            {
                await client.Client.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing client");
            }
        }
    }
}