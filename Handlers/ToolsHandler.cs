using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using McpMesh.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpMesh.Handlers;

public interface IToolsHandler
{
    Task<ListToolsResult> HandleListToolsAsync(RequestContext<ListToolsRequestParams> context, string packageId, CancellationToken cancellationToken);
    Task<CallToolResult> HandleCallToolAsync(RequestContext<CallToolRequestParams> context, string packageId, CancellationToken cancellationToken);
}

public class ToolsHandler : IToolsHandler
{
    private readonly IClientManager _clientManager;
    private readonly ILogger<ToolsHandler> _logger;

    public ToolsHandler(IClientManager clientManager, ILogger<ToolsHandler> logger)
    {
        _clientManager = clientManager;
        _logger = logger;
    }

    public async Task<ListToolsResult> HandleListToolsAsync(RequestContext<ListToolsRequestParams> context, string packageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ListTools request for package: {PackageId}", packageId);

        var tools = new List<Tool>();

        var clients = await _clientManager.GetClientsAsync(packageId);
        foreach (var client in clients)
        {
            tools.AddRange(client.Tools);
        }

        _logger.LogInformation("ListTools response: {ToolCount} tools found", tools.Count);
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var toolNames = string.Join(", ", tools.Select(t => t.Name));
            _logger.LogDebug("Tools: {ToolNames}", toolNames);
        }

        return new ListToolsResult
        {
            Tools = tools
        };
    }

    public async Task<CallToolResult> HandleCallToolAsync(RequestContext<CallToolRequestParams> context, string packageId, CancellationToken cancellationToken)
    {
        var toolName = context.Params?.Name;
        if (string.IsNullOrWhiteSpace(toolName))
        {
            _logger.LogError("CallTool request missing tool name");
            throw new ArgumentException("Tool name is required.", nameof(context.Params.Name));
        }
        _logger.LogInformation("CallTool request - Package: {PackageId}, Tool: {ToolName}", packageId, toolName);
        
        if (context.Params?.Arguments != null)
        {
            var argsJson = JsonSerializer.Serialize(context.Params.Arguments);
            var truncatedArgs = argsJson.Length > 200 ? argsJson.Substring(0, 200) + "..." : argsJson;
            _logger.LogDebug("Arguments: {Arguments}", truncatedArgs);
        }
        
        var client = await _clientManager.GetClientAsync(packageId, toolName);
        if (client == null)
        {
            _logger.LogError("Tool '{ToolName}' not found in package '{PackageId}'", toolName, packageId);
            throw new InvalidOperationException($"Tool '{toolName}' not found in package '{packageId}'.");
        }

        var originalToolName = toolName;
        if (!string.IsNullOrEmpty(client.ToolPrefix) && toolName.StartsWith($"{client.ToolPrefix}_"))
        {
            originalToolName = toolName[(client.ToolPrefix.Length + 1)..];
        }

        var arguments = context.Params?.Arguments?.ToDictionary(a => a.Key, a => (object)a.Value);
        try
        {
            var response = await client.Client.CallToolAsync(originalToolName, arguments, cancellationToken: cancellationToken);
            
            _logger.LogInformation("CallTool response - Tool: {ToolName}, Success: {IsSuccess}", 
                toolName, response.IsError != true);
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var responseJson = JsonSerializer.Serialize(response);
                var truncatedResponse = responseJson.Length > 200 ? responseJson.Substring(0, 200) + "..." : responseJson;
                _logger.LogDebug("Response: {Response}", truncatedResponse);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CallTool failed - Tool: {ToolName}", toolName);
            throw;
        }
    }
}