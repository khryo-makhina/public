namespace OllamaTranslatorApi;

/// <summary>
///   Represents a single entry in the translations CSV file, containing the source text and its corresponding target text. This class serves as a data model for storing and manipulating translation pairs during the processing of the CSV file, allowing for easy access to both the original text and its translation throughout the application.
/// </summary>
public class CsvEntry
{
    /// <summary>
    ///  Gets or sets the source text from the CSV entry. This property holds the original text that is to be translated, allowing for easy reference and manipulation during the translation process. It is initialized to an empty string to ensure that it always has a valid value, even if the CSV entry is incomplete or missing data.
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the target text from the CSV entry. This property holds the translated text corresponding to the source text, allowing for easy reference and manipulation during the translation process. It is initialized to an empty string to ensure that it always has a valid value, even if the CSV entry is incomplete or missing data.
    /// </summary>
    public string TargetText { get; set; } = string.Empty;
}