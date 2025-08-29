using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Collections.Generic;
using McpMesh.Configuration;

namespace McpMesh.Extensions;

public static class ToolExtensions
{
    public static Tool GetTool(this McpClientTool originalTool, McpServerOptions server)
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

        // Build structured description with server context
        var contextParts = new List<string>();

        if (!string.IsNullOrEmpty(server.ServerContext))
        {
            contextParts.Add($"Server: {server.ServerContext}");
        }

        if (!string.IsNullOrEmpty(server.Name))
        {
            contextParts.Add($"Source: {server.Name}");
        }

        if (!string.IsNullOrEmpty(server.ServerInstructions))
        {
            contextParts.Add($"Usage: {server.ServerInstructions}");
        }

        var contextString = contextParts.Count > 0 ? $"[{string.Join(" | ", contextParts)}] " : "";

        // Build final description
        if (!string.IsNullOrEmpty(description))
        {
            description = $"{contextString}{description}";
        }
        else
        {
            description = $"{contextString}{toolName}";
        }

        // Add description suffix if configured
        if (!string.IsNullOrEmpty(server.DescriptionSuffix))
        {
            description = $"{description} {server.DescriptionSuffix}";
        }

        return new Tool
        {
            Name = toolName,
            Description = description,
            InputSchema = originalTool.JsonSchema
        };
    }
}