using TranslationTools.OllamaApi;

namespace TranslationTools.Tests;

public class TestTranslationService : ITranslationService
{
    public async Task<string> TranslateAsync(string text)
    {
        return text switch
        {
            "Unknown" => "MockTranslation:Unknown",
            "Hello" => "Hola",
            "World" => "Mundo",
            "Test" => "Prueba",
            _ => throw new NotSupportedException($"No translation for '{text}'")
        };
    }
}


