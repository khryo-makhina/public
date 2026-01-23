
using System.IO;

static string EscapeCsv(string s) => s?.Replace("\"", "\"\"") ?? string.Empty;
var translationFinder = new TranslationTools.TranslationsFinder();
var translationFilepath = translationFinder.FindTranslationsCsvFilepath();

Console.WriteLine($"Appending new entries to: {translationFilepath}");

var isSearching = false;
string[] allSearchLines = Array.Empty<string>();
while (true)
{
    if (isSearching)
    {
        Console.WriteLine("Search Mode:: Enter English text (leave empty to quit): ");
    }
    else
    {
        Console.WriteLine("Add Mode:: Enter English text (leave empty to quit): ");
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
        isSearching = true;
        Console.WriteLine("Entered Search mode. Loading existing entries...");
        allSearchLines = translationFinder.GetTranslationsLines(translationFilepath);        
        if (allSearchLines.Length == 0)
        {
            Console.WriteLine("Exiting Search mode due to no entries found.");
            isSearching = false;
            continue;
        }  
        else
        {
            Console.WriteLine($"Loaded {allSearchLines.Length} entries for searching.");
        }   
        continue;
    }

    if (isSearching)
    {
        try
        {            
            Console.WriteLine("Search results:");
            foreach (var searchLine in allSearchLines)
            {
                var columns = searchLine.Split(new[] { ',' }, StringSplitOptions.None);
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
        continue;
    }

    var line = $"\"English\",\"{EscapeCsv(input)}\",\"Finnish\",\"\"";
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
