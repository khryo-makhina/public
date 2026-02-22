using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TranslationTools.OllamaApi;

public class OllamaTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string apiUrl = "http://localhost:11434/api/generate";

    public OllamaTranslationService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }    

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
            string json = string.Empty;
            // Create a StringWriter to hold the JSON output
            using (var stringWriter = new StringWriter())
            {
                // Create a JsonSerializer instance
                var serializer = new Newtonsoft.Json.JsonSerializer
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

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            response.EnsureSuccessStatusCode(); // Throw exception on error status codes (4xx, 5xx)

            string responseBody = await response.Content.ReadAsStringAsync();

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
            Console.Error.WriteLine($"HTTP request error for '{text}': {ex.Message}");
            // Log the error (e.g., using Serilog or NLog)
            return text; // Return the original text or a default value
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Console.Error.WriteLine($"JSON parsing error for '{text}': {ex.Message}");
            // Log the error
            return text; // Return the original text or a default value
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error during translation of '{text}': {ex.Message}");
            // Log the error
            return text; // Return the original text or a default value
        }
    }

    private string GetRequestPrompt(string text)
    {
        return $"Translate '{text}' to Finnish and return ONLY the translation. If multiple translatation candidates, pick two accurate and separate them in the translate content with a '/'.";
    }
    private string GetLlmModelName()
    {
        return "translategemma:12b";
    }
}