namespace TranslationTools;

/// <summary>
/// Represents a translation entry with English and Finnish text.
/// </summary>
public class TranslationEntry
{
    /// <summary>
    /// Source language code (e.g., "en-GB")
    /// </summary>
    public string SourceLanguage { get; set; }

    /// <summary>
    /// English text to be translated.
    /// </summary>
    public string EnglishText { get; set; }

    /// <summary>
    /// Target language code (e.g., "fi-FI")
    /// </summary>
    public string TargetLanguage { get; set; }  

    /// <summary>
    /// Finnish translated text.
    /// </summary>
    public string FinnishText { get; set; }
}
