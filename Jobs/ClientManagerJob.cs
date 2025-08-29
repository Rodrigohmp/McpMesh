using System.Threading;
using System.Threading.Tasks;
using McpMesh.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace McpMesh.Jobs;

public class ClientManagerJob : IHostedService
{
    private readonly ILogger<ClientManagerJob> _logger;
    private readonly IClientManager _clientManager;

    public ClientManagerJob(ILogger<ClientManagerJob> logger, IClientManager clientManager)
    {
        _logger = logger;
        _clientManager = clientManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing ClientManager...");
        await _clientManager.InitializeAsync();
        _logger.LogInformation("ClientManager is initialized");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ClientManager...");
        await _clientManager.DisposeAsync();
    }
}