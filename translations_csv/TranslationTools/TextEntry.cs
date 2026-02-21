namespace TranslationTools;

/// <summary>
/// Represents a text entry with the language code.
/// </summary>
public class TextEntry
{
    /// <summary>
    /// Language code (e.g., "en-GB")
    /// </summary>
    public VoiceLanguage Language { get; set; } = VoiceLanguage.Empty;

    /// <summary>
    /// Text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;   

    /// <summary>
    /// Presents an empty Entry.
    /// </summary>
    public static TextEntry Empty { get; } = new TextEntry
    {
        Language = VoiceLanguage.Empty,
        Text = string.Empty
    };    

    override public string ToString()
    {
        return $"Language: {Language}, Text: {Text}";
    }
}
