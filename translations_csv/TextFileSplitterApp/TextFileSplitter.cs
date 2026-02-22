using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TextFileSplitterApp;

public class TextFileSplitter
{
    public async Task<List<string>> SplitFileAsync(string inputFilePath, int maxLinesPerFile)
    {
        var outcomes = new StringBuilder();
        string baseFileName = Path.GetFileNameWithoutExtension(inputFilePath);
        string fileExtension = Path.GetExtension(inputFilePath);
        string? outputDirectory = Path.GetDirectoryName(inputFilePath);

        if (string.IsNullOrEmpty(outputDirectory))
        {
            outcomes.Append("Warning: Could not determine output directory from input file path. Using current directory for output files.");
            outputDirectory = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        int lineNumber = 0;
        int fileCount = 1;
        List<string> outputFiles = new List<string>();
        string outputFileFullPath = Path.Combine(outputDirectory, $"{baseFileName}_{fileCount}{fileExtension}");
        FileStream outputFileStream = null;
        StreamWriter outputFileWriter = null;

        try
        {
            using (var reader = new StreamReader(inputFilePath))
            {
                outputFileStream = new FileStream(outputFileFullPath, FileMode.Create);
                outputFileWriter = new StreamWriter(outputFileStream);

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    outputFileWriter.WriteLine(line);
                    lineNumber++;

                    if (lineNumber % maxLinesPerFile == 0)
                    {
                        outputFileWriter.Flush();
                        outputFileStream.Flush();

                        fileCount++;
                        outputFileFullPath = Path.Combine(outputDirectory, $"{baseFileName}_{fileCount}{fileExtension}");
                        outputFileStream = new FileStream(outputFileFullPath, FileMode.Create);
                        outputFileWriter = new StreamWriter(outputFileStream);
                        outputFiles.Add(outputFileFullPath);
                    }
                }

                outputFileWriter.Flush();
                outputFileStream.Flush();
                outputFiles.Add(outputFileFullPath);
            }
        }
        finally
        {
            outputFileWriter?.Close();
            outputFileStream?.Close();
        }

        return outputFiles;
    }

    public async Task<int> GetTotalLinesAsync(string filePath)
    {
        int lineCount = 0;
        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineCount++;
            }
        }
        return lineCount;
    }

    public async Task<string> GetSplittingInformation(string filePath, int maxLinesPerFile)
    {
        var outcomes = new StringBuilder();
        try
        {
            if (!File.Exists(filePath))
            {
                outcomes.Append($"Error: File not found at path: {filePath}");
                return outcomes.ToString();
            }

            int totalLines = await GetTotalLinesAsync(filePath);
            int estimatedFileCount = (int)Math.Ceiling((double)totalLines / maxLinesPerFile);

            outcomes.Append($"Input file: {filePath}");
            outcomes.Append($"Max lines per file: {maxLinesPerFile}");
            outcomes.Append($"Total lines in file: {totalLines}");
            outcomes.Append($"Estimated number of files to be created: {estimatedFileCount}");
            return outcomes.ToString();
        }
        catch (Exception ex)
        {
            outcomes.Append($"An error occurred: {ex.Message}");
            return outcomes.ToString();
        }
    }

    public async Task FormatAsTranslationEntries(List<string> filePaths)
    {
        foreach (string filePath in filePaths)
        {
            // Create a new TranslationEntryFormatter instance for each file
            var formatter = new TranslationEntryFormatter();
            await formatter.FormatFile(filePath);
        }
    }
}