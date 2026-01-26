namespace TranslationTools;

/// <summary>
/// Provides methods to find and read translation CSV files.
/// </summary>
public class TranslationsFinder
{
    private const string SettingsJsonFilename = "settings.json";
    private const string TranslationCsvFilename = "translations.csv";

    /// <summary>
    /// Finds the .csv file path by searching parent directories or using settings.
    /// </summary>
    /// <returns>Full path to the .csv file</returns>
    public string FindTranslationsCsvFilepath(string csvFilePath = "")
    {
        bool isTranslationEntryTranslations = String.IsNullOrEmpty(csvFilePath) || csvFilePath.Contains(TranslationCsvFilename);
        if (!isTranslationEntryTranslations)
        {
            Console.WriteLine("Using " + TranslationCsvFilename + " file at: " + csvFilePath);
            return csvFilePath;
        }

        Console.WriteLine($"Resolving {TranslationCsvFilename} file path from settings ...");
        var translationFilepath = GetTranslationFilePathFromSettings();
        if (!String.IsNullOrEmpty(translationFilepath))
        {
            if(!File.Exists(translationFilepath))
            {
                Console.WriteLine($"Warning: The translation file path from settings does not exist: {translationFilepath}");
            }
            else
            {
                Console.WriteLine($"Using {TranslationCsvFilename} file at: {translationFilepath}");
                return translationFilepath;
            }            
        }

        Console.WriteLine($"Could not determine {TranslationCsvFilename} file path from settings.");
        Console.WriteLine($"Resolving {TranslationCsvFilename} file path from parent directories...");
        
        // Check current working directory first
        if(File.Exists(TranslationCsvFilename))
        {
            translationFilepath = Path.GetFullPath(TranslationCsvFilename);
            Console.WriteLine($"Found {TranslationCsvFilename} in current working directory at: {translationFilepath}");
            return translationFilepath;
        }

        // Try to find the repository folder named "translations_csv" by climbing parents
        Console.WriteLine($"Searching for of folder of file {TranslationCsvFilename} ...");
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (string.Equals(dir.Name, TranslationCsvFilename, StringComparison.OrdinalIgnoreCase))
            {
                translationFilepath = Path.Combine(dir.FullName, TranslationCsvFilename);
                break;
            }
            dir = dir.Parent;
        }

        if (!String.IsNullOrEmpty(translationFilepath))
        {
            if(!File.Exists(translationFilepath))
            {
                Console.WriteLine($"Warning: The found translation file path does not exist: {translationFilepath}");
            }   
            else
            {   
                Console.WriteLine($"Found {TranslationCsvFilename} at: {translationFilepath}");
                return translationFilepath; 
            }
        }

        // Fallback to previous behavior (parent of current working dir)
        var fallbackPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", TranslationCsvFilename));
        if(!File.Exists(fallbackPath))
        {
            Console.WriteLine($"Warning: The fallback translation file path does not exist: {fallbackPath}");
            return String.Empty;
        }   
        else
        {   
            Console.WriteLine($"Using fallback {TranslationCsvFilename} file at: {fallbackPath}");
            return fallbackPath; 
        }        
    }

    /// <summary>
    /// Reads the translation file path from settings.json if available.
    /// </summary>
    /// <returns>Translation file path or empty string if not found</returns>
    public string GetTranslationFilePathFromSettings()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsJsonFilename);
        bool settingsJsonExists = File.Exists(settingsPath);
        if (!settingsJsonExists)
        {
            Console.WriteLine($"Settings file not found at: {settingsPath}");
            return String.Empty;
        }

        var json = File.ReadAllText(settingsPath);
        var startIndex = json.IndexOf("\"translation_filepath\"", StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            Console.WriteLine($"Translation file path entry not found in settings.");
            return String.Empty;
        }   
        
        startIndex = json.IndexOf(":", startIndex) + 1;
        var endIndex = json.IndexOfAny(new[] { ',', '}', '\n', '\r' }, startIndex);
        var pathValue = json.Substring(startIndex, endIndex - startIndex).Trim().Trim('"');

        if(string.IsNullOrWhiteSpace(pathValue))
        {
            Console.WriteLine($"Translation file path entry is empty in settings.");
            return String.Empty;
        }
        
        string tidyPath = Path.GetFullPath(pathValue);
        if (!string.IsNullOrWhiteSpace(tidyPath))
        {
            Console.WriteLine($"Translation file path found in settings: {tidyPath}");
            return tidyPath;
        }        

        return String.Empty;
    }

    /// <summary>
    /// Reads all lines from the .csv file.
    /// </summary>
    /// <param name="translationFilepath"></param>
    /// <returns>A list of strings representing the lines in the CSV file</returns>
    public string[] GetTranslationsLines(string translationFilepath, int startinglineNumber = -1)
    {
        if (!File.Exists(translationFilepath))
        {
            return Array.Empty<string>();
        }

        IEnumerable<string> array;
        if (startinglineNumber > 0)
        {
            array = File.ReadAllLines(translationFilepath).Skip(startinglineNumber);
        }
        else
        {
            array = File.ReadAllLines(translationFilepath);
        }
        return [.. array];//to array
    }
}

