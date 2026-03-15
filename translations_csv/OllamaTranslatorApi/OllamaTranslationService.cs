using System.Text;
using Newtonsoft.Json;

namespace OllamaTranslatorApi;

public class OllamaTranslationService(HttpClient? httpClient = null) : ITranslationService
{
    /// <summary>
    /// Initializes a new instance of the OllamaTranslationService class, which provides functionality to translate text using the Ollama API. The constructor accepts an optional HttpClient parameter, allowing for dependency injection of a custom HttpClient instance. If no HttpClient is provided, a new instance will be created internally. This design promotes flexibility and testability of the translation service, enabling it to be easily integrated into various applications and testing scenarios without being tightly coupled to a specific HttpClient implementation.
    /// </summary>
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    /// <summary>
    /// Defines the URL endpoint for the Ollama API translation service. This constant string specifies the base URL to which translation requests will be sent. The URL is set to "http://localhost:11434/api/generate", indicating that the Ollama API is expected to be running locally on port 11434 and that the translation requests should be directed to the "/api/generate" endpoint. This constant can be modified if the API endpoint changes or if the service is hosted on a different server or port, allowing for easy configuration of the translation service without requiring changes to the core logic of the application.
    /// </summary>
    private const string ApiUrl = "http://localhost:11434/api/generate";

    /// <inheritdoc />
    public async Task<string> TranslateAsync(string text)
    {
        var textTrimmed = text.Trim().Trim('"').Replace("\"", "`");

        var request = new OllamaTranslationRequest
        {
            Model = GetLlmModelName(),
            Prompt = GetRequestPrompt(textTrimmed),
            Stream = false
        };

        try
        {
            string json;
            // Create a StringWriter to hold the JSON output
            await using (var stringWriter = new StringWriter())
            {
                // Create a JsonSerializer instance
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented // Makes JSON pretty-printed
                };

                // Serialize the object to the StringWriter
                serializer.Serialize(stringWriter, request);

                // Get the JSON string
                json = stringWriter.ToString();

                // Output the JSON
                Console.WriteLine("Serialized JSON:");
                Console.WriteLine(json);
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(ApiUrl, content);

            response.EnsureSuccessStatusCode(); // Throw exception on error status codes (4xx, 5xx)

            var responseBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                Console.WriteLine($"Warning: Empty response body for '{text}'. Returning original text.");
                return text; // Return original text on error
            }

            Console.WriteLine($"Response: {responseBody}"); // Log the raw response
            return responseBody.Trim(); // Return the trimmed response
        }
        catch (HttpRequestException ex)
        {
            await Console.Error.WriteLineAsync($"HTTP request error for '{text}': {ex.Message}");
            // Log the error (e.g., using Serilog or NLog)
            return text; // Return the original text or a default value
        }
        catch (JsonException ex)
        {
            await Console.Error.WriteLineAsync($"JSON parsing error for '{text}': {ex.Message}");
            // Log the error
            return text; // Return the original text or a default value
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error during translation of '{text}': {ex.Message}");
            // Log the error
            return text; // Return the original text or a default value
        }
    }

    /// <summary>
    /// Generates the prompt text for the translation request based on the input text. This method constructs a specific instruction for the language model, asking it to translate the given text into Finnish and to return only the translation. If there are multiple accurate translation candidates, it instructs the model to separate them with a '/' character. This structured prompt helps guide the language model to produce the desired output format and ensures that the translation results are clear and concise for further processing within the application.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The generated prompt text for the translation request.</returns>
    private string GetRequestPrompt(string text)
    {
        return
            $"Translate '{text}' to Finnish and return ONLY the translation. If multiple translation candidates, pick two accurate and separate them in the translate content with a '/'.";
    }

    /// <summary>
    /// Gets the name of the language model to be used for translation. This method currently returns a hardcoded model name "translategemma:12b", which is specified in the OllamaTranslationRequest class as the default value for the Model property. The method can be modified in the future to allow for dynamic selection of different models based on specific translation requirements or user preferences, enabling greater flexibility and customization of the translation process.
    /// </summary>
    /// <returns>The name of the language model to be used for translation.</returns>
    private string GetLlmModelName()
    {
        return "translategemma:12b";
    }
}