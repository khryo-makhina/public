namespace TranslationTools;

/// <summary>
/// Represents a translation entry with English and Finnish text.
/// </summary>
public class TranslationEntry
{
    /// <summary>
    /// Source language code (e.g., "en-GB")
    /// </summary>
    public string SourceLanguage { get; set; } = string.Empty;

    /// <summary>
    /// English text to be translated.
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// Target language code (e.g., "fi-FI")
    /// </summary>
    public string TargetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Finnish translated text.
    /// </summary>
    public string TargetText { get; set; } = string.Empty;

    /// <summary>
    /// Presents an empty TranslationEntry.
    /// </summary>
    public static TranslationEntry Empty { get; } = new TranslationEntry
    {
        SourceLanguage = string.Empty,
        SourceText = string.Empty,
        TargetLanguage = string.Empty,
        TargetText = string.Empty
    };    
}
