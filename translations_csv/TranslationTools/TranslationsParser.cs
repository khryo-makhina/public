using System.Text;
using System.Collections.Generic;

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
    public TranslationEntryList ParseTranslationsCsvLines(string[] csvLines)
    {
        var entryList = new TranslationEntryList();

        if(csvLines.Length == 0)
        {
            return entryList;
        }   

        bool isTranslationEntryTranslations = csvLines.Length > 2 
          && csvLines[0].Contains("Source Language,") && csvLines[0].Contains("Target Language,");

        int SourceTextColumnIndex = 0;
        int TargetTextColumnIndex = 1;
        if (isTranslationEntryTranslations)
        {
            Console.WriteLine("Parsing " + nameof(TranslationEntry) + " CSV lines ...");
            entryList.SourceLanguage = csvLines[0].Split(',')[0].Replace("Source Language,", "").Trim();
            entryList.TargetLanguage = csvLines[0].Split(',')[2].Replace("Target Language,", "").Trim();    
            Console.WriteLine($"Source Language: {entryList.SourceLanguage}, Target Language: {entryList.TargetLanguage}");
        }
        else
        {
            Console.WriteLine("Parsing value-pair CSV lines ...");
            bool isTwoColumnsTargetFirst = csvLines[0].StartsWith("[Target:") && csvLines[0].Contains("[Source:");
            if (isTwoColumnsTargetFirst)
            {
                SourceTextColumnIndex = 1;
                TargetTextColumnIndex = 0;
                var sourceLanguage = csvLines[0].Split(',')[1].Replace("[Source:", "").Replace("]", "").Trim();
                entryList.SourceLanguage = sourceLanguage;

                var targetLanguage = csvLines[0].Split(',')[0].Replace("[Target:", "").Replace("]", "").Trim();
                entryList.TargetLanguage = targetLanguage;    
                Console.WriteLine($"Source Language: {entryList.SourceLanguage}, Target Language: {entryList.TargetLanguage}");
            }
            else
            {
                var isTwoColumnsSourceFirst = csvLines[0].StartsWith("[Source:") && csvLines[0].Contains("[Target:");
                if (isTwoColumnsSourceFirst)
                {
                    SourceTextColumnIndex = 0;
                    TargetTextColumnIndex = 1;
                    var sourceLanguage = csvLines[0].Split(',')[0].Replace("[Source:", "").Replace("]", "").Trim();
                    entryList.SourceLanguage = sourceLanguage;
                    var targetLanguage = csvLines[0].Split(',')[1].Replace("[Target:", "").Replace("]", "").Trim();
                    entryList.TargetLanguage = targetLanguage;
                    Console.WriteLine($"Source Language: {entryList.SourceLanguage}, Target Language: {entryList.TargetLanguage}");
                }
                else
                {
                    var isSingleColumn = csvLines[0].StartsWith("[Source:") && !csvLines[0].Contains("[Target:");
                    if (isSingleColumn)
                    {
                        SourceTextColumnIndex = 0;
                        TargetTextColumnIndex = 1;
                        var sourceLanguage = csvLines[0].Split(',')[0].Replace("[Source:", "").Replace("]", "").Trim();
                        entryList.SourceLanguage = sourceLanguage;
                        entryList.TargetLanguage = String.Empty;
                        Console.WriteLine($"Source Language: {entryList.SourceLanguage}, Target Language: {entryList.TargetLanguage}");
                    }
                }
            }
        }

        foreach (var line in csvLines)
        {
            if(line.StartsWith("[Source Language") || line.StartsWith("[Target Language"))
            {
                // Skip header line
                continue;
            }

            var columns = SplitCsvLine(line).ToArray();
            //Example: English,"among the monolith, sunk into the ground",Finnish,"maahan vajonneiden, megaliittien keskellä"
            if (columns.Length >= 4)
            {
                var sourceText = columns[1].Trim('"');
                var targetText = columns[3].Trim('"');
                entryList.Add(new TranslationEntry
                {
                    SourceText = sourceText,
                    TargetText = targetText
                });
            }
            else
            {
                if (columns.Length >= 2)
                {                    
                    //Example: "Hello, this is a sample.","Hei, tämä on esimerkki."
                    string sourceText = columns[SourceTextColumnIndex].Trim('"');
                    string targetText = columns[TargetTextColumnIndex].Trim('"');

                    entryList.Add(new TranslationEntry
                    {
                        SourceText = sourceText,
                        TargetText = targetText
                    });
                }
            }
        }
        return entryList;
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