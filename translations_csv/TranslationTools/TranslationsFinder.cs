namespace TranslationTools;

/// <summary>
/// Provides methods to find and read translation CSV files.
/// </summary>
public class TranslationsFinder
{
    /// <summary>
    /// Finds the translations.csv file path by searching parent directories or using settings.
    /// </summary>
    /// <returns>Full path to the translations.csv file</returns>
    public string FindTranslationsCsvFilepath(string csvFilePath = "")
    {
        bool isTranslationEntryTranslations = String.IsNullOrEmpty(csvFilePath) || csvFilePath.Contains("translations_csv");
        if(!isTranslationEntryTranslations)
        {                                 
            Console.WriteLine("Using translations CSV file at: " + csvFilePath);
            return csvFilePath;
        }

        Console.WriteLine("Resolving translations CSV file path from settings ...");
        var translationFilepath = GetTranslationFilePathFromSettings();
        if (!String.IsNullOrEmpty(translationFilepath))
        {
            return translationFilepath;
        }
        
        Console.WriteLine("Could not determine translations.csv file path from settings.");
        Console.WriteLine("Resolving translations CSV file path from parent directories...");

        // Try to find the repository folder named "translations_csv" by climbing parents
        Console.WriteLine("Searching for translations_csv folder...");
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (string.Equals(dir.Name, "translations_csv", StringComparison.OrdinalIgnoreCase))
            {
                translationFilepath = Path.Combine(dir.FullName, "translations.csv");
                break;
            }
            dir = dir.Parent;
        }

        if (!String.IsNullOrEmpty(translationFilepath))
        {
            Console.WriteLine($"Found translations.csv at: {translationFilepath}");
            return translationFilepath; ;
        }        

        // Fallback to previous behavior (parent of current working dir)
        var fallbackPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "translations.csv"));
        Console.WriteLine($"Falling back to: {fallbackPath}");
        return fallbackPath;
    }

    /// <summary>
    /// Reads the translation file path from settings.json if available.
    /// </summary>
    /// <returns>Translation file path or empty string if not found</returns>
    public string GetTranslationFilePathFromSettings()
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

    /// <summary>
    /// Reads all lines from the translations CSV file.
    /// </summary>
    /// <param name="translationFilepath"></param>
    /// <returns>A list of strings representing the lines in the CSV file</returns>
    public string[] GetTranslationsLines(string translationFilepath)
    {
        if (!File.Exists(translationFilepath))
        {
            return Array.Empty<string>();
        }

        return File.ReadAllLines(translationFilepath);
    }
}

