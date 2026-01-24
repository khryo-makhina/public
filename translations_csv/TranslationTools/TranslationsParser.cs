using System.Text;
namespace TranslationTools;

/// <summary>
/// Parses translation CSV lines into translation entries.
/// </summary>
public class TranslationsParser
{
    /// <summary>
    /// Parses CSV lines into a list of TranslationEntry objects.
    /// </summary>
    /// <param name="csvLines">Raw CSV lines</param>
    /// <returns>A list of TranslationEntry objects</returns>
    public List<TranslationEntry> ParseTranslationsCsvLines(string[] csvLines)
    {
        var entries = new List<TranslationEntry>();

        if(csvLines.Length == 0)
        {
            return entries;
        }   

        bool isTranslationEntryTranslations
          = csvLines[0].Contains("Source Language") && csvLines[0].Contains("Target Language");

        int FinnishTextColumnIndex = 0;
        int EnglishTextColumnIndex = 1;
        if (isTranslationEntryTranslations)
        {
            Console.WriteLine("Parsing translation entry CSV lines ...");
        }
        else
        {
            Console.WriteLine("Parsing value-pair CSV lines ...");
            if(csvLines[0].StartsWith("[Finnish]") && csvLines[0].Contains("[English]"))
            {
                FinnishTextColumnIndex = 0;
                EnglishTextColumnIndex = 1;
            }
            else if(csvLines[0].StartsWith("[English]") && csvLines[0].Contains("[Finnish]"))
            {
                EnglishTextColumnIndex = 0;
                FinnishTextColumnIndex = 1;
            }                  
        }

        foreach (var line in csvLines)
        {
            if(line.StartsWith("[Source Language]") || line.StartsWith("[Finnish]") || line.StartsWith("[English]")|| line.StartsWith("[Vietnamese]"))
            {
                // Skip header line
                continue;
            }

            var columns = SplitCsvLine(line).ToArray();
            //Example: English,"among the monolith, sunk into the ground",Finnish,"maahan vajonneiden, megaliittien keskellä"
            if (columns.Length >= 4)
            {
                var englishText = columns[1].Trim('"');
                var finnishText = columns[3].Trim('"');
                entries.Add(new TranslationEntry
                {
                    EnglishText = englishText,
                    FinnishText = finnishText
                });
            }
            else
            {
                if (columns.Length >= 2)
                {                    
                    //Example: "Hello, this is a sample.","Hei, tämä on esimerkki."
                    string englishText = columns[EnglishTextColumnIndex].Trim('"');
                    string finnishText = columns[FinnishTextColumnIndex].Trim('"');

                    entries.Add(new TranslationEntry
                    {
                        EnglishText = englishText,
                        FinnishText = finnishText
                    });
                }
            }
        }
        return entries;
    }

    public static List<string> SplitCsvLine(string line)
{
    var result = new List<string>();
    var current = new StringBuilder();
    bool inQuotes = false;

    for (int i = 0; i < line.Length; i++)
    {
        char c = line[i];

        if (c == '"')
        {
            // Toggle quoted state, unless it's an escaped quote ("")
            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                current.Append('"'); // escaped quote
                i++;                 // skip next quote
            }
            else
            {
                inQuotes = !inQuotes;
            }
        }
        else if (c == ',' && !inQuotes)
        {
            // End of field
            result.Add(current.ToString());
            current.Clear();
        }
        else
        {
            current.Append(c);
        }
    }

    // Add last field
    result.Add(current.ToString());

    return result;
}

}