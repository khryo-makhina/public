using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace TranslationTools.OllamaApi;

/// <summary>
/// The OllamaTranslator class provides functionality to translate text from English to Finnish using the Ollama API. 
/// It includes methods for translating individual texts, batch translating a list of entries, 
/// and processing CSV files containing source texts and saving the translated results. 
/// The class handles HTTP communication with the Ollama API, error handling, and ensures proper CSV formatting for the output.
/// </summary>
public class OllamaTranslator
{
    /// <summary>
    /// The HttpClient instance used for making HTTP requests to the Ollama API. 
    /// It is initialized in the constructor and can be optionally injected for testing or customization purposes.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The base URL for the Ollama API endpoint used for generating translations.
    /// </summary>
    private readonly string _ollamaUrl = "http://localhost:11434/api/generate"; // Default Ollama port

    /// <summary>
    /// The ITranslationService instance used for translating text. 
    /// It is initialized in the constructor and can be optionally injected for testing or customization purposes.
    /// </summary>
    private readonly ITranslationService _translationService;

    /// <summary>
    /// Initializes a new instance of the OllamaTranslator class with optional parameters for translation service and HTTP client.
    /// </summary>
    /// <param name="translationService"><see cref="ITranslationService"/> instance to use for translation operations.</param>
    /// <param name="httpClient">Optional <see cref="HttpClient"/> instance to use for HTTP requests.</param>
    public OllamaTranslator(ITranslationService? translationService = null, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _translationService = translationService ?? new OllamaTranslationService();
    }

    /// <summary>
    /// Translate in batches with controlled parallelism to avoid overwhelming the API and to improve performance.
    /// </summary>
    /// <param name="entries">The source texts contained in a list of <see cref="CsvEntry"/></param>
    /// <param name="maxParallelTasks">Optional max concurrent requests. Default: 4.</param>
    /// <returns>A list of <see cref="CsvEntry"/> containing translations.</returns>
    public async Task<List<CsvEntry>> BatchTranslateAsync(List<CsvEntry> entries, int maxParallelTasks = 4)
    {
        if (entries == null || !entries.Any())
            return entries ?? new List<CsvEntry>();

        var results = new List<CsvEntry>();
        var translatedEntries = new List<CsvEntry>();

        TranslationCsvFileHandler csvFileHandler = new();
        await Parallel.ForEachAsync(entries, new ParallelOptions { MaxDegreeOfParallelism = maxParallelTasks }, async (entry, token) =>
        {
            await Task.Delay(100, token); // Small delay to prevent overwhelming the API, a delay without blocking.

            try
            {
                var response = await _translationService.TranslateAsync(entry.SourceText);
                var responseObject = JsonConvert.DeserializeObject<OllamaResponseObject>(response);
                if(responseObject == null)
                {
                    Console.WriteLine($"Received NULL response for '{entry.SourceText}'.");
                    translatedEntries.Add(entry); // Add the entry even if translation fails.
                    return;
                }

                var targetText = responseObject.Response?.Trim() ?? entry.SourceText; // Fallback to source text if translation is empty


                entry.TargetText = csvFileHandler.EnsureContentNotQuoted(targetText); // Ensure proper CSV formatting
                entry.SourceText = csvFileHandler.EnsureContentNotQuoted(entry.SourceText); // Ensure proper CSV formatting

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
    /// Processes a CSV file by reading it, translating its contents, and writing the translated results to a new CSV file.
    /// </summary>
    /// <param name="inputFilepath">The path to the input CSV file.</param>
    /// <param name="outputFilepath">The path where the translated output CSV file will be saved.</param>
    public async Task ProcessCsvAsync(string inputFilepath, string outputFilepath)
    {       
        // Verify input file exists
        if (!File.Exists(inputFilepath))
        {
            throw new FileNotFoundException($"Input file `{inputFilepath}` not found in current working directory `{Directory.GetCurrentDirectory()}`.");
        }

        TranslationCsvFileHandler csvFileHandler = new();
        try
        {            
            List<CsvEntry> records = csvFileHandler.ReadCsvRecords(inputFilepath);

            if(records == null || !records.Any())
            {
                Console.WriteLine($"No records found in `{inputFilepath}`. Please check the file content.");
                return; // Exit if no records to process
            }
            
            Console.WriteLine($"Translating {records.Count} phrases...");
            List<CsvEntry> translatedRecords = await BatchTranslateAsync(records);

            var result = csvFileHandler.WriteCsvRecords(translatedRecords, outputFilepath);
            Console.WriteLine(result);
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