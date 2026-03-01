namespace TextFileSplitterApp;

/// <summary>
/// Provides methods to split large text files into smaller files and
/// to format split outputs as translation entries.
/// </summary>
public class TextFileSplitter
{
    /// <summary>
    /// Default maximum number of lines per output file (legacy constant).
    /// </summary>
    public const int MaxLinesPerFile1 = 10000;

    /// <summary>
    /// Minimum number of lines required to perform a split.
    /// </summary>
    public const int MinLinesPerFile = 2;

    /// <inheritdoc />
    public async Task<SplitProcessInfo> SplitFileAsync(SplitRequestInfo splitRequestInfo)
    {
        var splitProcessInfo = new SplitProcessInfo(splitRequestInfo);

        var initError = splitProcessInfo.Initialize();

        if (!string.IsNullOrEmpty(initError))
        {
            return splitProcessInfo;
        }

        using var reader = new StreamReader(splitProcessInfo.InputFilepath);

        FileStream? outputFileStream = null;
        StreamWriter? outputFileWriter = null;
        var currentOutputLineCount = 0;

        try
        {
            // Read lines until EOF (ReadLineAsync returns null at EOF).
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line is null)
                {
                    // EOF reached
                    break;
                }

                // Lazily create writer for the first output file or when starting a new file
                if (outputFileWriter is null)
                {
                    var newFileName = splitProcessInfo.GetNewFilename();
                    outputFileStream = new FileStream(newFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    outputFileWriter = new StreamWriter(outputFileStream);
                }

                await outputFileWriter.WriteLineAsync(line);
                splitProcessInfo.LineNumber += 1;
                currentOutputLineCount += 1;

                // When we hit the configured lines per file, rotate to the next file
                if (currentOutputLineCount >= Math.Max(splitProcessInfo.SplitLinesPerFile, MinLinesPerFile))
                {
                    try
                    {
                        await outputFileWriter.FlushAsync();
                        outputFileWriter.Dispose();
                        outputFileWriter = null;
                        outputFileStream?.Dispose();
                        outputFileStream = null;
                    }
                    catch (Exception e)
                    {
                        splitProcessInfo.ErrorList.Add(e.Message);
                    }

                    currentOutputLineCount = 0;
                }
            }

            // Ensure the last writer is flushed and disposed
            if (outputFileWriter is not null)
            {
                await outputFileWriter.FlushAsync();
                outputFileWriter.Dispose();
                outputFileStream?.Dispose();
            }
        }
        finally
        {
            outputFileWriter?.Dispose();
            outputFileStream?.Dispose();

            reader.Close();
        }

        return splitProcessInfo;
    }

    /// <summary>
    /// Counts the total number of non-empty lines in the specified file.
    /// This is an internal helper and does not perform public validation.
    /// </summary>
    /// <param name="filePath">Path to the file to count lines for.</param>
    /// <returns>Total number of non-empty lines in the file.</returns>
    private async Task<int> GetTotalLinesAsync(string filePath)
    {
        var lineCount = 0;
        using var reader = new StreamReader(filePath);
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            // Count every line read, including empty lines, to avoid losing content
            lineCount++;
        }

        return lineCount;
    }

    /// <inheritdoc />
    public async Task<SplitRequestInfo> GetSplittingInformation(string filePath, int maxLinesPerFile)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var errorRequestInfo = new SplitRequestInfo();
                errorRequestInfo.Error.Add($"ErrorList: File not found at path: {filePath}");
                return errorRequestInfo;
            }

            var totalLines = await GetTotalLinesAsync(filePath);
            var estimatedFileCount = Math.Max(1, (int)Math.Ceiling((double)totalLines / Math.Max(1, maxLinesPerFile)));

            var splitFileInfo = new SplitRequestInfo
            {
                InputFilepath = filePath,
                TotalLinesPerFile = totalLines,
                // The requested number of lines per output file
                SplitLinesPerFile = maxLinesPerFile,
                // Estimated number of output files required
                SplitFilesAmount = estimatedFileCount
            };

            return splitFileInfo;
        }
        catch (Exception ex)
        {
            var errorRequestInfo = new SplitRequestInfo();
            errorRequestInfo.Error.Add($"ErrorList occurred: {ex.Message}");
            return errorRequestInfo;
        }
    }

    /// <summary>
    /// Formats the files produced by a split operation as translation entries.
    /// </summary>
    /// <param name="splitResult">The split operation result containing output file paths.</param>
    /// <returns>A task representing the asynchronous formatting operation.</returns>
    public async Task FormatAsTranslationEntries(SplitProcessInfo splitResult)
    {
        foreach (var filePath in splitResult.OutputFiles)
        {
            // Create a new TranslationEntryFormatter instance for each file
            var formatter = new TranslationEntryFormatter();
            await formatter.FormatFile(filePath);
        }
    }
}