using McpMesh.Configuration;
using McpMesh.Handlers;
using McpMesh.Jobs;
using McpMesh.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Reflection;

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "custom";
});
builder.Logging.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>(options =>
{
    options.ShowLogLevel = true;
});

// Enable debug logging for everything to diagnose issues
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAntiforgery();

var mcpMeshOptionsSection = builder.Configuration.GetSection(nameof(McpMeshOptions));
builder.Services.Configure<McpMeshOptions>(mcpMeshOptionsSection);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddHostedService<ClientManagerJob>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IToolsHandler, ToolsHandler>();
builder.Services.AddSingleton<IClientManager, ClientManager>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();


builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => {
        options.Stateless = true;     
    })
    .WithListToolsHandler(async (context, token) =>
    {
        var handler = context.Services?.GetRequiredService<IToolsHandler>();
        return await McpHandler.HandleAsync(handler.HandleListToolsAsync, context, token);
    })
    .WithCallToolHandler(async (context, token) =>
    {
        var handler = context.Services?.GetRequiredService<IToolsHandler>();
        return await McpHandler.HandleAsync(handler.HandleCallToolAsync, context, token);
    });

var app = builder.Build();

// Log startup information
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Startup");
logger.LogInformation("===========================================");
logger.LogInformation("McpMesh Server v{Version} starting up", version);
logger.LogInformation("===========================================");

app.UseCors();

app.UseRouting();

app.MapControllers();
app.MapRazorPages();

app.MapMcp("{packageId}");

app.Run();