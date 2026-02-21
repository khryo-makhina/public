namespace TranslationTools;

public class ConsoleLogger
{
    public static List<ConsoleLogEntry> Logs { get; } = new List<ConsoleLogEntry>();
    internal static void WriteLine(string log)
    {
        var logEntry = new ConsoleLogEntry(log);
        Logs.Add(logEntry);
    }
}

public class ConsoleLogEntry
{
    public string LogText;

    public ConsoleLogEntry(string log)
    {
        this.LogText = log;
    }
}
