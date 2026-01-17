using System;
using System.IO;

static string EscapeCsv(string s) => s?.Replace("\"", "\"\"") ?? string.Empty;

string FindTranslationsCsv()
{
    // Try to find the repository folder named "translations_csv" by climbing parents
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (string.Equals(dir.Name, "translations_csv", StringComparison.OrdinalIgnoreCase))
            return Path.Combine(dir.FullName, "translations.csv");
        dir = dir.Parent;
    }

    // Fallback to previous behavior (parent of current working dir)
    return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "translations.csv"));
}

string GetTranslationFilePathFromSettings()
{
    var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    if (File.Exists(settingsPath))
    {
        var json = File.ReadAllText(settingsPath);
        var startIndex = json.IndexOf("\"translation_filepath\"", StringComparison.OrdinalIgnoreCase);
        if (startIndex >= 0)
        {
            startIndex = json.IndexOf(":", startIndex) + 1;
            var endIndex = json.IndexOfAny(new[] { ',', '}', '\n', '\r' }, startIndex);
            var pathValue = json.Substring(startIndex, endIndex - startIndex).Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(pathValue))
                return pathValue;
        }
    }
    return String.Empty;
}

var translationFilepath = FindTranslationsCsv();
if(String.IsNullOrEmpty(translationFilepath))
{
    translationFilepath = GetTranslationFilePathFromSettings();
    if(String.IsNullOrEmpty(translationFilepath))
    {
        Console.WriteLine("Could not determine translations.csv file path.");
        return;
    }        
}
Console.WriteLine($"Appending new entries to: {translationFilepath}");

while (true)
{
    Console.Write("Enter English text (leave empty to quit): ");
    var english = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(english))
        break;

    var line = $"\"English\",\"{EscapeCsv(english)}\",\"Finnish\",\"\"";
    try
    {
        File.AppendAllText(translationFilepath, line + Environment.NewLine);
        Console.WriteLine("Added (Finnish left empty). Waiting for next input...");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to append: {ex.Message}");
        break;
    }
}

Console.WriteLine("Exiting. Goodbye.");
