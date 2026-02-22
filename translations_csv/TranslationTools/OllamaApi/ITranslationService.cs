namespace TranslationTools.OllamaApi;

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

public interface ITranslationService
{
    Task<string> TranslateAsync(string text);
}
