using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TranslationTools.OllamaApi;

public class OllamaTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string apiUrl = "http://localhost:11434/api/generate";

    public OllamaTranslationService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    private string GetRequestPrompt(string text)
    {
        return "instruction: Translate the following English text to Finnish. Provide ONLY the translation." + Environment.NewLine +
            "format: plain text" + Environment.NewLine +
            "input_text:" + text;        
    }

    public async Task<string> TranslateAsync(string text)
    {
        var textTrimmed = text.Trim().Trim('"');

        var requestPrompt = GetRequestPrompt(textTrimmed);

        var request = new
        {
            model = "translategemma:12b",
            prompt = $"Translate to Finnish '{text}' and return ONLY the translation",
            stream = false
        };

        try
        {
            using (var client = _httpClient)
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Attempt a simple parsing.  This might need adjustment based on the actual response format.
                    // For example, if the response is a JSON object with a "result" field, you'd access it here.

                    if (string.IsNullOrWhiteSpace(responseBody))
                    {
                        Console.WriteLine("Error: Empty response body.");
                        return text; // Return original text on error
                    }

                    Console.WriteLine($"Response: {responseBody}"); // Log the raw response
                    return responseBody.Trim(); // Return the trimmed response
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.WriteLine($"Error Body: {await response.Content.ReadAsStringAsync()}");
                    return text; // Return original text on error
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
            return text; // Return original text on error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return text; // Return original text on error
        }
    }
}