using System.Threading.Tasks;
using McpMesh.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace McpMesh.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly IClientManager _clientManager;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IClientManager clientManager, ILogger<HealthController> logger)
    {
        _clientManager = clientManager;
        _logger = logger;
    }

    [HttpGet("live")]
    public IActionResult Liveness()
    {
        return Ok(new { status = "alive", timestamp = System.DateTime.UtcNow });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Readiness()
    {
        try
        {
            // Check if we can get clients for default package
            var clients = await _clientManager.GetClientsAsync("default");
            
            return Ok(new { 
                status = "ready", 
                timestamp = System.DateTime.UtcNow,
                clientCount = clients.Count
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "Readiness check failed");
            return StatusCode(503, new { 
                status = "not ready", 
                error = ex.Message,
                timestamp = System.DateTime.UtcNow
            });
        }
    }

    [HttpGet("startup")]
    public async Task<IActionResult> Startup()
    {
        try
        {
            // More lenient check for startup - just verify service is responding
            var clients = await _clientManager.GetClientsAsync("default");
            
            return Ok(new { 
                status = "started", 
                timestamp = System.DateTime.UtcNow,
                clientCount = clients.Count
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogDebug(ex, "Startup check not ready yet");
            return StatusCode(503, new { 
                status = "starting", 
                error = ex.Message,
                timestamp = System.DateTime.UtcNow
            });
        }
    }
}