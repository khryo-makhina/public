using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationTools.OllamaApi;

internal class TranslationCsvFileHandler
{
    /// <summary>
    /// Ensure that the content is not double-quoted, to maintain CSV integrity.
    /// </summary>
    /// <remarks>
    /// This method trims any leading or trailing whitespace and double quotes from the input string.
    /// It also replaces any internal double quotes with single quotes to prevent issues when writing to CSV files.
    /// </remarks>
    /// <param name="input">String input might already containing double quotes or none.</param>
    /// <returns>Cleaned string double quoted both ends. If the input string is null or empty, it returns an empty quoted string ("").</returns>    
    public string EnsureContentNotQuoted(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return "\"\""; // Return empty quoted string for null or empty input
        }

        var output = input ?? string.Empty;
        output = output.Trim().Trim('\"');
        output = output.Replace("\"", "`"); // Replace internal quotes with single quotes to avoid CSV issues        

        return output;
    }

    /// <summary>
    /// Reads CSV records from the specified input file path and returns a list of CsvEntry objects. 
    /// The method uses UTF-8 encoding with BOM for better compatibility with Excel. It utilizes the CsvHelper 
    /// library to read and parse the CSV file, mapping each record to a CsvEntry object. 
    /// The resulting list of CsvEntry objects is returned to the caller.
    /// </summary>
    /// <param name="inputFilepath">The path to the input CSV file.</param>
    /// <returns>A list of CsvEntry objects representing the records in the CSV file.</returns>
    public List<CsvEntry> ReadCsvRecords(string inputFilepath)
    {
        // UTF-8 with BOM for better Excel compatibility
        var utf8WithBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        using var reader = new StreamReader(inputFilepath, utf8WithBom);
        using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
        List<CsvEntry> records = [.. csv.GetRecords<CsvEntry>()];//Use collection expression to create a new list from the records
        return records;
    }

    /// <summary>
    /// Writes a list of CsvEntry objects to a CSV file at the specified output file path. 
    /// The method ensures that the output directory exists before writing the file. 
    /// It uses UTF-8 encoding with BOM for better compatibility with Excel and the CsvHelper library 
    /// to write the records to the CSV file. The method returns a string indicating the result of the operation, 
    /// including the number of records saved or any errors encountered during the writing process.
    /// </summary>
    /// <param name="translatedRecords">A list of CsvEntry objects to be written to the output CSV file.</param>
    /// <param name="outputFilepath">The path to the output CSV file.</param>
    /// <returns>A string indicating the result of the operation, including the number of records saved or any errors encountered.</returns>
    public string WriteCsvRecords(List<CsvEntry> translatedRecords, string outputFilepath)
    {
        // UTF-8 with BOM for better Excel compatibility
        var utf8WithBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        // Ensure output directory exists
        var outputDir = EnsureOutputDirectoryExists(outputFilepath);

        // Write output CSV

        var utf8_Bom = new UTF8Encoding(true); // UTF with BOM encoding
        using var stream = new FileStream("file.csv", FileMode.Create);
        using var writer = new StreamWriter(outputFilepath, false, utf8WithBom);
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ",",// CSV delimiter
            Encoding = Encoding.UTF8,
            Quote = '"', // Character used for quoting
            ShouldQuote = args => true, // Always quote all fields
            NewLine = Environment.NewLine // Platform-specific newline
        };

        var result = string.Empty;
        using var csvWriter = new CsvWriter(writer, config);

        try
        {
            csvWriter.WriteRecords(translatedRecords);

            result = $"Done: {translatedRecords.Count} results saved to {outputFilepath}";
        }
        catch (IOException ex)
        {
            result = $"Error: Failed to write output file `{outputFilepath}`: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Ensures that the output directory exists before writing the CSV file. If the directory does not exist, it creates it.
    /// </summary>
    /// <param name="outputFilePath">The full path to the output CSV file.</param>
    /// <returns>The output directory path.</returns>
    private static string EnsureOutputDirectoryExists(string outputFilePath)
    {
        var outputDir = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        return outputDir ?? string.Empty;
    }
}
