using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace McpMesh.Services;

public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool ShowLogLevel { get; set; } = true;
}

public class CustomConsoleFormatter : ConsoleFormatter
{
    private readonly IOptions<CustomConsoleFormatterOptions> _options;

    public CustomConsoleFormatter(IOptions<CustomConsoleFormatterOptions> options) : base("custom")
    {
        _options = options;
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        var logLevel = logEntry.LogLevel;
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        
        // Color based on log level
        var originalColor = Console.ForegroundColor;
        
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case LogLevel.Information:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogLevel.Critical:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;
        }

        // Format based on log level
        if (logLevel >= LogLevel.Debug)
        {
            // For Debug and above, show category in verbose mode
            if (_options.Value.ShowLogLevel && logLevel == LogLevel.Debug || logLevel == LogLevel.Trace)
            {
                textWriter.Write($"[{timestamp}] [{logLevel,-5}] [{logEntry.Category}] ");
            }
            else
            {
                textWriter.Write($"[{timestamp}] ");
                if (logLevel >= LogLevel.Warning)
                {
                    textWriter.Write($"[{logLevel,-5}] ");
                }
            }
        }
        
        textWriter.WriteLine(message);
        
        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception);
        }
        
        Console.ForegroundColor = originalColor;
    }
}