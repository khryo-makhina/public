using System.Text;

namespace TranslationTools;

/// <summary>
///     Parses translation CSV lines into translation entries.
/// </summary>
public class TranslationsParser
{
    /// <summary>
    ///     Parses CSV lines into a list of TranslationEntry objects.
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

        var headerLine = GetCleanHeaderLine(csvLines[0]);
        var headerColumns = GetHeaderColumns(headerLine);

        var (sourceLanguages, targetLanguages) = ExtractSourceAndTargetLanguages(headerColumns);

        AddVoiceLanguages(entryList, sourceLanguages, LanguageTypes.Source);
        AddVoiceLanguages(entryList, targetLanguages, LanguageTypes.Target);

        entryList.IsTranslationEntryTranslations = csvLines.Length > 2
                                                   && headerLine.Contains("Source Language,") &&
                                                   headerLine.Contains("Target Language,");


        foreach (var line in csvLines)
        {
            var entryRow = CreateEntryRowFromLine(line, entryList.VoiceLanguages);
            entryList.Add(entryRow);
        }

        return entryList;
    }

    /// <summary>
    ///    Retrieves the language name for a given index from the voice languages list. If the index is out of range, it returns a default system language. This method ensures that each text entry is associated with the correct language based on its position in the CSV line, while also providing a fallback option to maintain robustness in cases where the CSV structure may not perfectly align with the expected format.
    /// </summary>
    /// <param name="voiceLanguages">The list of detected voice languages</param>
    /// <param name="index">The index of the language to retrieve</param>
    /// <returns>The voice language at the specified index or a default system language if the index is out of range</returns>
    private VoiceLanguage GetLanguageName(VoiceLanguageList voiceLanguages, int index)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (index < voiceLanguages.Count)
        {
            return voiceLanguages[index];
        }
        return VoiceLanguage.System;
    }

    /// <summary>
    ///     Returns a cleaned header line with quotes and brackets removed and trimmed.
    /// </summary>
    /// <param name="rawHeader">The original header line</param>
    /// <returns>The cleaned header line</returns>
    private string GetCleanHeaderLine(string rawHeader)
    {
        return rawHeader.Replace("\"", "").Replace("[", "").Replace("]", "").Replace(" ", "").Trim();
    }

    /// <summary>
    ///     Splits a cleaned header line into individual header columns.
    /// </summary>
    /// <param name="cleanHeader">A cleaned header line</param>
    /// <returns>An array of header column strings</returns>
    private string[] GetHeaderColumns(string cleanHeader)
    {
        return cleanHeader.Split(',').Select(h => h.Trim()).ToArray();
    }

    /// <summary>
    ///     Extracts source and target language names from header columns.
    /// </summary>
    /// <param name="headerColumns">Array of header column strings</param>
    /// <returns>A tuple containing source languages and target languages arrays</returns>
    private (string[] sources, string[] targets) ExtractSourceAndTargetLanguages(string[] headerColumns)
    {
        var sources = headerColumns.Where(h => h.StartsWith("Source", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Split(':')[1]).ToArray();
        var targets = headerColumns.Where(h => h.StartsWith("Target", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Split(':')[1]).ToArray();

        return (sources, targets);
    }

    /// <summary>
    ///     Adds detected voice languages to the provided entry list and logs detection.
    /// </summary>
    /// <param name="entryList">The entry list to update</param>
    /// <param name="languages">Array of language names to add</param>
    /// <param name="type">The language type (Source/Target)</param>
    private void AddVoiceLanguages(TranslationEntryList entryList, string[] languages, LanguageTypes type)
    {
        foreach (var language in languages)
        {
            ConsoleLogger.WriteLine($"Detected {type.ToString().ToLower()} language: {language}");
            entryList.VoiceLanguages.Add(new VoiceLanguage(language, type));
        }
    }

    /// <summary>
    ///     Creates a <see cref="TextEntryRow"/> from a CSV line using the provided voice language list.
    /// </summary>
    /// <param name="line">The CSV line to parse</param>
    /// <param name="voiceLanguages">The list of detected voice languages</param>
    /// <returns>A populated <see cref="TextEntryRow"/></returns>
    private TextEntryRow CreateEntryRowFromLine(string line, VoiceLanguageList voiceLanguages)
    {
        var entryRow = new TextEntryRow();
        var columns = SplitCsvLine(line).ToArray();
        for (var i = 0; i < columns.Length; i++)
        {
            var column = columns[i];
            var entry = new TextEntry
            {
                Language = GetLanguageName(voiceLanguages, i),
                Text = column.Trim('"').Trim()
            };

            entryRow.Add(entry);
        }

        return entryRow;
    }

    /// <summary>
    ///     Splits a CSV line into individual fields, handling quoted fields and escaped quotes.
    /// </summary>
    /// <param name="line">The CSV line to parse</param>
    /// <returns>A list of strings representing the CSV fields</returns>
    /// <remarks>
    ///     This method properly handles:
    ///     - Fields enclosed in quotes
    ///     - Escaped quotes within fields (represented as "")
    ///     - Commas within quoted fields
    ///     - Empty fields
    /// </remarks>
    public static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        var position = 0;

        foreach (var c in line)
        {
            if (c == '"')
            {
                // Toggle quoted state, unless it's an escaped quote ("")
                if (inQuotes && position + 1 < line.Length && line[position + 1] == '"')
                {
                    current.Append('"'); // escaped quote
                    position++; // skip next quote
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