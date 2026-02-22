using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextFileSplitterApp;

public class TranslationEntryFormatter
{
    public async Task FormatFile(string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at path: {filePath}");
            return;
        }

        // Define the output file path (same directory, different name)
        string outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) +
"_formatted.txt");

        try
        {
            // Read the file line by line
            string[] lines = await File.ReadAllLinesAsync(filePath);

            // Process each line and format it
            List<string> formattedLines = new List<string>();
            foreach (string line in lines)
            {
                // Example formatting:  Let's assume each line is a translation entry
                // and we want to wrap it in <entry> tags.  Adjust this regex as needed.
                string formattedLine = $"<entry>{line}</entry>";
                formattedLines.Add(formattedLine);
            }

            // Write the formatted lines to the output file
            await File.WriteAllLinesAsync(outputFilePath, formattedLines);

            Console.WriteLine($"File formatted and saved to: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while formatting: {ex.Message}");
        }
    }
}