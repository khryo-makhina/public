namespace FilenameSanitizer;

public interface ILogger
{
    void LogError(string message);
    void LogError(string message, Exception ex);
    void LogDebug(string message);
}
