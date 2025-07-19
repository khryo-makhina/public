namespace FilenameSanitizer;

public class FilenameSanitationOperationLog
{
    public List<string> Errors { get; private set; } = new();
    public List<string> Warnings { get; private set; } = new();

    public List<string> Info { get; private set; } = new();

    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;

    public bool HasInfo => Info.Count > 0;

    public bool HasErrorsOrWarnings => HasErrors || HasWarnings;

    /// <summary>
    /// Writes the log entries to a file in the specified folder.
    /// </summary>
    /// <param name="folder">Folder where the log file will be created</param>
    public void FlushToFile(string folder)
    {
        var timestamp = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
        var logFileName = Path.Combine(folder, $"FileNameSanitizer.log.{timestamp}.txt");

        var logEntries = FormatLogEntries();

        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllLines(logFileName, logEntries);
        }
        catch (Exception ex)
        {
            Errors.Add($"Failed to write log file: {ex.Message}");
        }
    }

    private IEnumerable<string> FormatLogEntries()
    {
        var entries = new List<string>
        {
            "=== FileNameSanitizer Operation Log ===",
            $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            string.Empty
        };

        if (HasErrors)
        {
            entries.Add("ERRR:");
            entries.AddRange(Errors.Select(e => $"- {e}"));
            entries.Add(string.Empty);
        }

        if (HasWarnings)
        {
            entries.Add("WARN:");
            entries.AddRange(Warnings.Select(w => $"- {w}"));
            entries.Add(string.Empty);
        }

        if (HasInfo)
        {
            entries.Add("INFO:");
            entries.AddRange(Info.Select(i => $"- {i}"));
        }

        return entries;
    }
}
