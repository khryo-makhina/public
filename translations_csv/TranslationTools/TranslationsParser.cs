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
        
        foreach (var line in csvLines)
        {
            var columns = line.Split(new[] { ',' }, StringSplitOptions.None);
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
        }
        return entries;
    }
}