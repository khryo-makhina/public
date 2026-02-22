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

namespace TranslationTools.OllamaApi;

public class OllamaTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string apiUrl = "http://localhost:11434/api/generate";

    public OllamaTranslationService(HttpClient httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> TranslateAsync(string text)
    {
        var request = new
        {
            model = "translategemma:12b",
            prompt = $"Translate the following English text to Finnish:\n{text}\nFinnish translation:",
            stream = false
        };

        try
        {
            using (var client = _httpClient)
            {
                // Create HttpContent from the request object
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var ollamaResponse = JsonConvert.DeserializeObject<OllamaResponse>(await
response.Content.ReadAsStringAsync());

                        if (ollamaResponse != null)
                        {
                            // Check if Context is null before accessing it
                            if (ollamaResponse.Context != null)
                            {
                                Console.WriteLine("Context values:");
                                foreach (var value in ollamaResponse.Context)
                                {
                                    Console.WriteLine(value);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Context is null in the response.");
                            }

                            Console.WriteLine($"Response: {ollamaResponse.Response}");
                            Console.WriteLine($"Done: {ollamaResponse.Done}");

                            return ollamaResponse.Response; // Return the translation!
                        }
                        else
                        {
                            Console.WriteLine("Error: OllamaResponse is null.");
                            return text; // Return original text on error.
                        }
                    }
                    catch (JsonSerializationException jsonEx)
                    {
                        Console.WriteLine($"JSON Deserialization error: {jsonEx.Message}");
                        Console.WriteLine($"Response body: {await response.Content.ReadAsStringAsync()}");
                        return text; // Return original text on error.
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.WriteLine($"Error Body: {await response.Content.ReadAsStringAsync()}");
                    return text; // Return original text on error.
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translation failed for '{text}': {ex.Message}");
            return text; // Fallback to original text
        }
    }
}