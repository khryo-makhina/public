namespace TranslationTools;

/// <summary>
///     Represents a text entry with the language code.
/// </summary>
public class TextEntry
{
    /// <summary>
    ///     Language code (e.g., "en-GB")
    /// </summary>
    public VoiceLanguage Language { get; set; } = VoiceLanguage.Empty;

    /// <summary>
    ///     Text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     Presents an empty Entry.
    /// </summary>
    public static TextEntry Empty { get; } = new() { Language = VoiceLanguage.Empty, Text = string.Empty };

    /// <summary>
    ///    Returns a string representation of the TextEntry, including the language code and text content. This method provides a convenient way to visualize the contents of a TextEntry instance, making it easier to debug or display the language and text information in a readable format. The output format is "Language: {Language}, Text: {Text}", where {Language} is the language code and {Text} is the associated text content.
    /// </summary>
    /// <returns>A string representation of the TextEntry.</returns>
    public override string ToString()
    {
        return $"Language: {Language}, Text: {Text}";
    }
}