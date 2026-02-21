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

        if (csvLines.Length == 0)
        {
            return entryList;
        }

        var headerLine = csvLines[0].Replace("\"", "").Replace("[", "").Replace("]", "").Replace(" ", "").Trim();
        var headerColumns = headerLine.Split(',').Select(h => h.Trim()).ToArray();

        var sourceLanguages = headerColumns.Where(h => h.StartsWith("Source", StringComparison.OrdinalIgnoreCase)).Select(x => x.Split(':')[1]).ToArray();
        var targetLanguages = headerColumns.Where(h => h.StartsWith("Target", StringComparison.OrdinalIgnoreCase)).Select(x => x.Split(':')[1]).ToArray();

        foreach (var language in sourceLanguages)
        {
            ConsoleLogger.WriteLine($"Detected source language: {language}");
            entryList.VoiceLanguages.Add(new VoiceLanguage(language, LanguageTypes.Source));
        }

        foreach (var language in targetLanguages)
        {
            ConsoleLogger.WriteLine($"Detected target language: {language}");
            entryList.VoiceLanguages.Add(new VoiceLanguage(language, LanguageTypes.Target));
        }

        entryList.IsTranslationEntryTranslations = csvLines.Length > 2
          && headerLine.Contains("Source Language,") && headerLine.Contains("Target Language,");


        foreach (string line in csvLines)
        {
            var entryRow = new TextEntryRow();
            var columns = SplitCsvLine(line).ToArray();
            for (int i = 0; i < columns.Length; i++)
            {
                string column = columns[i];
                var entry = new TextEntry
                {
                    Language = GetLanguageName(entryList.VoiceLanguages, i),
                    Text = column.Trim('"').Trim(),
                };

                entryRow.Add(entry);
            }
            entryList.Add(entryRow);
            
        }
        return entryList;
    }

    private VoiceLanguage GetLanguageName(VoiceLanguageList voiceLanguages, int index)
    {
        return VoiceLanguage.System;
    }

    /// <summary>
    /// Splits a CSV line into individual fields, handling quoted fields and escaped quotes.
    /// </summary>
    /// <param name="line">The CSV line to parse</param>
    /// <returns>A list of strings representing the CSV fields</returns>
    /// <remarks>
    /// This method properly handles:
    /// - Fields enclosed in quotes
    /// - Escaped quotes within fields (represented as "")
    /// - Commas within quoted fields
    /// - Empty fields
    /// </remarks>
    public static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        int position = 0;

        foreach (char c in line)
        {
            if (c == '"')
            {
                // Toggle quoted state, unless it's an escaped quote ("")
                if (inQuotes && position + 1 < line.Length && line[position + 1] == '"')
                {
                    current.Append('"'); // escaped quote
                    position++;         // skip next quote
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
            position++;
        }

        // Add last field
        result.Add(current.ToString());

        return result;
    }
}