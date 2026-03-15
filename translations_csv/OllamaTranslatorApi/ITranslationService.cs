namespace OllamaTranslatorApi;

/*Usage example:
class Program
{
    static async Task Main(string[] args)
    {
        var translator = new OllamaTranslator();

        // Example 1: Translate a single phrase
        var singleTranslation = await translator.TranslateToFinnishAsync("Hello, how are you?");
        Console.WriteLine($"Single translation: {singleTranslation}");

        // Example 2: Process a CSV file
        await translator.ProcessCsvAsync(
            inputFilePath: $"G:\code\GitHub\khryo-makhina\public\translations_csv\TranslationTools\OllamaApi\test\input_phrases.csv",  // CSV with a "text" column
            outputFilePath: $"G:\code\GitHub\khryo-makhina\public\translations_csv\TranslationTools\OllamaApi\test\translated_fi.csv"
        );
    }
}*/

/// <summary>
///   Defines an interface for a translation service that provides asynchronous translation capabilities. Implementations of this interface are expected to provide a method for translating text, allowing for flexibility in how translations are performed (e.g., using different APIs or algorithms). The interface abstracts the translation functionality, enabling the application to use various translation services interchangeably without being tightly coupled to a specific implementation.
/// </summary>
public interface ITranslationService
{

    /// <summary>
    /// Translates the given text asynchronously using the Ollama API and returns the translated result as 
    /// a string. The method constructs a translation request based on the input text, sends it to the Ollama API, 
    /// and processes the response to extract the translated text. It includes error handling to manage potential 
    /// issues during the HTTP request or response processing, ensuring that the application can gracefully handle 
    /// errors and return appropriate fallback values when necessary. The asynchronous nature of the method allows 
    /// for non-blocking operations, making it suitable for use in applications that require responsive user 
    /// interfaces or need to perform multiple translations concurrently.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the translated text.</returns>    
    Task<string> TranslateAsync(string text);
}