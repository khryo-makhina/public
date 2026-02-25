using System.Text;
using TranslationTools;

Console.OutputEncoding = Encoding.UTF8;

var translationFinder = new TranslationsFinder();
var translationFilepath = translationFinder.FindTranslationsCsvFilepath();

Console.WriteLine($"Appending new entries to: {translationFilepath}");

var isSearching = false;
var allSearchLines = Array.Empty<string>();
while (true)
{
    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
    if (isSearching)
    {
        PrintModePrompt(isSearching);
    }
    else
    {
        PrintModePrompt(isSearching);
    }

    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        if (!isSearching)
        {
            break;
        }

        isSearching = false;
        allSearchLines = Array.Empty<string>();
        Console.WriteLine("Exited the Search mode. Entries unloaded.");
        Console.WriteLine("Returning to Add mode.");
        continue;
    }

    if (input.StartsWith("--search"))
    {
        allSearchLines = EnterSearchMode(translationFinder, translationFilepath);
        isSearching = allSearchLines.Length > 0;
        continue;
    }

    if (isSearching)
    {
        PerformSearch(input, allSearchLines);
        continue;
    }

    var line = $"\"English\",\"{EscapeCsv(input)}\",\"Finnish\",\"\",\"<Context Categorization Hashtags>\"";
    try
    {
        if (AppendNewEntry(translationFilepath, line))
        {
            Console.WriteLine("Added (Finnish left empty). Waiting for next input...");
        }
        else
        {
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to append: {ex.Message}");
        break;
    }
}

Console.WriteLine("Exiting. Goodbye.");
return;

/// <summary>
///    Escapes a string for safe inclusion in a CSV file by doubling any existing double quotes
///    and ensuring that the string is properly enclosed in double quotes if it contains commas or newlines.
/// </summary> <param name="s">The input string to escape</param>
/// <returns>The escaped string, safe for inclusion in a CSV file</returns>
static string EscapeCsv(string s)
{
    return s.Replace("\"", "\"\"");
}

/// <summary>
/// Prints the input prompt corresponding to the current mode (search or add).
/// </summary>
/// <param name="isSearching">If <c>true</c> prints the Search mode prompt; otherwise prints the Add mode prompt.</param>
static void PrintModePrompt(bool isSearching)
{
    if (isSearching)
    {
        Console.WriteLine("Search Mode:: Enter English text (leave empty to quit): ");
    }
    else
    {
        Console.WriteLine("Add Mode:: Enter English text (leave empty to quit): ");
    }
}

/// <summary>
/// Enters search mode by loading existing translation lines from the provided filepath.
/// Writes status messages to the console and returns the loaded lines.
/// </summary>
/// <param name="translationFinder">A <see cref="TranslationsFinder"/> used to retrieve translation lines.</param>
/// <param name="translationFilepath">Path to the translations CSV file.</param>
/// <returns>An array of translation file lines to be used for searching; empty if none found.</returns>
static string[] EnterSearchMode(TranslationsFinder translationFinder, string translationFilepath)
{
    Console.WriteLine("Entered Search mode. Loading existing entries...");
    try
    {
        var allSearchLines = translationFinder.GetTranslationsLines(translationFilepath);
        if (allSearchLines.Length == 0)
        {
            Console.WriteLine("Exiting Search mode due to no entries found.");
            return Array.Empty<string>();
        }

        Console.WriteLine($"Loaded {allSearchLines.Length} entries for searching.");
        return allSearchLines;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load entries for searching: {ex.Message}");
        return Array.Empty<string>();
    }
}

/// <summary>
/// Performs a search over the provided translation lines and writes matching results
/// to the console. Matches are performed against the English column (index 1).
/// </summary>
/// <param name="input">The search query to match against English text.</param>
/// <param name="allSearchLines">All translation CSV lines previously loaded.</param>
static void PerformSearch(string input, string[] allSearchLines)
{
    try
    {
        Console.WriteLine("Search results:");
        foreach (var searchLine in allSearchLines)
        {
            var columns = searchLine.Split([','], StringSplitOptions.None);
            var englishText = columns.Length >= 2 ? columns[1].Trim('"') : string.Empty;
            if (columns.Length >= 2 && englishText.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var finnishText = columns.Length >= 4 ? columns[3].Trim('"') : string.Empty;
                Console.WriteLine("  " + englishText + " = " + finnishText);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to read file for searching: {ex.Message}");
    }
}

/// <summary>
/// Appends the specified CSV line to the translations file.
/// </summary>
/// <param name="translationFilepath">The translations CSV file path.</param>
/// <param name="line">The CSV-formatted line to append.</param>
/// <returns><c>true</c> if the append succeeded; otherwise <c>false</c>.</returns>
static bool AppendNewEntry(string translationFilepath, string line)
{
    try
    {
        File.AppendAllText(translationFilepath, line + Environment.NewLine);
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to append: {ex.Message}");
        return false;
    }
}