using CsvHelper;
using Newtonsoft.Json;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace TranslationTools.OllamaApi;

public class OllamaTranslator
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl = "http://localhost:11434/api/generate"; // Default Ollama port
    private readonly ITranslationService _translationService;

    public OllamaTranslator(ITranslationService? translationService = null, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _translationService = translationService ?? new OllamaTranslationService();
    }

    /// <summary>
    /// Translate a single text to Finnish using Ollama
    /// </summary>
    public async Task<string> TranslateToFinnishAsync(string text)
    {
        var request = new
        {
            model = "tinyllama", // or "llama3" for better quality
            prompt = $"Translate the following English text to Finnish:\n{text}\nFinnish translation:",
            stream = false
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_ollamaUrl, request);
            response.EnsureSuccessStatusCode();

            var ollamaResponse = JsonConvert.DeserializeObject<OllamaResponse>(
        await response.Content.ReadAsStringAsync()
    );


            // Extract the translation (Ollama returns JSON with "response" field)
            return ollamaResponse?.Response?.Trim() ?? text; // Fallback to original if empty
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translation failed for '{text}': {ex.Message}");
            return text; // Fallback to original text
        }
    }

    /// <summary>
    /// Add a retry mechanism for transient errors:
    /// </summary>
    /// <param name="text"></param>
    /// <param name="retries"></param>
    /// <returns>string</returns>   
    private async Task<string> TranslateWithRetryAsync(string text, int retries = 3)
    {
        var request = new
        {
            model = "tinyllama",
            prompt = $"Translate the following English text to Finnish:\n{text}\nFinnish translation:",
            stream = false
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_ollamaUrl, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonConvert.DeserializeObject<OllamaResponse>(result);

            Console.WriteLine($"Request: {JsonConvert.SerializeObject(request)}");
            Console.WriteLine($"Response: {result}");

            // Safely extract the translation (null-check + fallback)
            return ollamaResponse?.Response?.Trim() ?? text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translation failed for '{text}': {ex.Message}");
            return text; // Fallback to original text
        }
    }

    ///// <summary>
    ///// Batch translate a list of texts (with parallel processing).
    ///// Adjust `maxParallelTasks` (e.g., `8` for SSDs, `4` for HDDs).
    ///// </summary>
    //public async Task<List<CsvEntry>> BatchTranslateAsync(List<CsvEntry> entries, int maxParallelTasks = 4)
    //{
    //    if (entries == null || !entries.Any())
    //        return entries ?? new List<CsvEntry>();

    //    var results = new List<CsvEntry>();

    //    // Process entries in parallel
    //    var tasks = entries.Select(async entry =>
    //    {
    //        try
    //        {
    //            // Translate each entry
    //            entry.TargetText = await _translationService.TranslateAsync(entry.SourceText);
    //            return entry;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error translating '{entry.SourceText}': {ex.Message}");
    //            return entry;
    //        }
    //    });

    //    await Task.WhenAll(tasks);
    //    return entries;
    //}   

    public async Task<List<CsvEntry>> BatchTranslateAsync(List<CsvEntry> entries, int maxParallelTasks = 4)
    {
        if (entries == null || !entries.Any())
            return entries ?? new List<CsvEntry>();

        var results = new List<CsvEntry>();
        var translatedEntries = new List<CsvEntry>();

        await Parallel.ForEachAsync(entries, new ParallelOptions { MaxDegreeOfParallelism = maxParallelTasks }, async (entry, token) =>
        {
            try
            {
                entry.TargetText = await _translationService.TranslateAsync(entry.SourceText);
                translatedEntries.Add(entry);  // Add to the translated list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error translating '{entry.SourceText}': {ex.Message}");
                // Optionally:  You might want to add the entry with an error flag or message.
                translatedEntries.Add(entry); // Add the entry even if there's an error.
            }
        });

        return translatedEntries; // Return the translated list.
    }

    /// <summary>
    /// Read CSV, translate, and save results
    /// </summary>
    public async Task ProcessCsvAsync(string inputFilePath, string outputFilePath)
    {
        // UTF-8 with BOM for better Excel compatibility
        var utf8WithBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        // Verify input file exists
        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException($"Input file `{inputFilePath}` not found in Current working directory `{Directory.GetCurrentDirectory()}`.");
        }

        try
        {
            // Read input CSV
            using var reader = new StreamReader(inputFilePath, utf8WithBom);
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
            List<CsvEntry> records = csv.GetRecords<CsvEntry>().ToList();

            // Translate in batches
            Console.WriteLine($"Translating {records.Count} phrases...");
            List<CsvEntry> translatedRecords = await BatchTranslateAsync(records);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Write output CSV
            using var writer = new StreamWriter(outputFilePath, false, utf8WithBom);
            using var csvWriter = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(translatedRecords);

            Console.WriteLine($"Done! Results saved to {outputFilePath}");
        }
        catch (IOException ex)
        {
            // Re-throw IO exceptions to maintain expected behavior
            throw new IOException($"File operation failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            // Re-throw all other exceptions
            throw new Exception($"Unexpected error during processing: {ex.Message}", ex);
        }
    }

}