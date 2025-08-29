using McpMesh.Configuration;
using McpMesh.Models;
using McpMesh.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace McpMesh.Handlers;

public class McpHandler
{
    public delegate Task<TOut> RequestHandler<TIn, TOut>(RequestContext<TIn> context, string packageId, CancellationToken cancellationToken);

    public static async Task<TOut> HandleAsync<TIn, TOut>(
        RequestHandler<TIn, TOut> handler, RequestContext<TIn> context, CancellationToken cancellationToken = default)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler), "Handler cannot be null.");
        }

        var httpContextAccessor = context.Services!.GetRequiredService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext");

        var packageId = httpContext.Request.RouteValues.TryGetValue("packageId", out var v) ? v?.ToString() : null;
        var token = httpContext.Request.Query["token"].ToString();

        var options = context.Services?.GetRequiredService<IOptions<McpMeshOptions>>().Value;
        if (options?.Authentication?.Enabled == true)
        {
            var encryptionService = context.Services?.GetRequiredService<IEncryptionService>();
            var authService = context.Services?.GetRequiredService<IAuthService>();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException("Token is missing.");
            }

            var credentials = encryptionService.Decrypt<Credentials>(token);
            if (credentials == null || string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
            {
                throw new UnauthorizedAccessException("Invalid or missing credentials.");
            }

            var isAuthenticated = await authService.ValidateAsync(credentials.Username, credentials.Password);
            if (!isAuthenticated)
            {
                throw new UnauthorizedAccessException("Authentication failed.");
            }
        }

        return await handler(context, packageId, cancellationToken);
    }
}