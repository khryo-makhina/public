using System;

namespace FilenameSanitizer;

public class ConsoleLogger : ILogger //TODO: Fix this. Use the build in, or introduce the log level
{
    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void LogError(string message, Exception ex)
    {
        Console.WriteLine($"[ERROR] {message}: {ex.Message}");
    }
    
    public void LogDebug(string message)
    {        
        Console.WriteLine($"[DEBUG] {message}");
    }
}
