using System.Text;

namespace TextFileSplitterApp;

public class TextFileSplitter
{
    public const int MaxLinesPerFile1 = 10000;
    public const int MinLinesPerFile = 2;

    public async Task<SplitFileOutcomeInfo> SplitFileAsync(SplitFileInfo splitFileInfo)
    {
        var splitInfo = new SplitFileOutcomeInfo(splitFileInfo);

        var initError = splitInfo.Initialize();

        if (!string.IsNullOrEmpty(initError))
        {
            return splitInfo;
        }

        using var reader = new StreamReader(splitFileInfo.InputFilepath);

        splitInfo.FileCount += 1;
        var newFileName = splitInfo.GetNewFilename();
        FileStream outputFileStream = new FileStream(newFileName, FileMode.Create);
        StreamWriter outputFileWriter = new StreamWriter(outputFileStream);

        try
        {
            var line = await reader.ReadLineAsync() ?? string.Empty;

            if (line.Length < splitInfo.SplitLinesPerFile)
            {
                return splitInfo;
            }

            while (!string.IsNullOrEmpty(line))
            {
                await outputFileWriter!.WriteLineAsync(line);

                splitInfo.LineNumber += 1;

                if (splitInfo.LineNumber % splitFileInfo.SplitLinesPerFile == 0)
                {
                    try
                    {
                        outputFileWriter.Flush();

                        outputFileStream?.Close();
                        outputFileWriter?.Close();

                        splitInfo.FileCount += 1;
                        newFileName = splitInfo.GetNewFilename();
                        outputFileStream = new FileStream(newFileName, FileMode.Create);
                        outputFileWriter = new StreamWriter(outputFileStream);
                    }
                    catch (Exception)
                    {

                    }
                }

                line = await reader.ReadLineAsync() ?? string.Empty;
            }

            splitInfo.GetNewFilename();

        }
        finally
        {
            outputFileWriter?.Close();
            outputFileStream?.Close();

            reader?.Close();
        }

        return splitInfo;
    }

    public async Task<int> GetTotalLinesAsync(string filePath)
    {
        int lineCount = 0;
        using (var reader = new StreamReader(filePath))
        {
            string line = await reader.ReadLineAsync() ?? string.Empty;
            while (!string.IsNullOrEmpty(line))
            {
                lineCount++;
            }
        }
        return lineCount;
    }

    public async Task<SplitFileInfo> GetSplittingInformation(string filePath, int maxLinesPerFile)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new SplitFileInfo() { Error = $"Error: File not found at path: {filePath}" };
            }

            int totalLines = await GetTotalLinesAsync(filePath);
            int estimatedFileCount = (int)Math.Ceiling((double)totalLines / maxLinesPerFile);

            var splitFileInfo = new SplitFileInfo()
            {
                InputFilepath = filePath,
                TotalLinesPerFile = totalLines,
                SplitLinesPerFile = estimatedFileCount,
                SplitFilesAmount = estimatedFileCount
            };

            return splitFileInfo;
        }
        catch (Exception ex)
        {
            return new SplitFileInfo() { Error = $"Error occurred: {ex.Message}" };
        }
    }

    public async Task FormatAsTranslationEntries(SplitFileOutcomeInfo splitResult)
    {
        foreach (string filePath in splitResult.OutputFiles)
        {
            // Create a new TranslationEntryFormatter instance for each file
            var formatter = new TranslationEntryFormatter();
            await formatter.FormatFile(filePath);
        }
    }
}

public class SplitFileInfo
{
    public string InputFilepath { get; set; } = string.Empty;
    public int TotalLinesPerFile { get; set; } = 0;
    public int SplitLinesPerFile { get; set; } = 0;
    public int SplitFilesAmount { get; set; } = 0;
    public string Error { get; internal set; } = string.Empty;

    public override string ToString()
    {
        var outcomes = new StringBuilder();
        outcomes.Append($"Input file: {InputFilepath}" + Environment.NewLine);
        outcomes.Append($"Total lines in file: {TotalLinesPerFile}" + Environment.NewLine);
        outcomes.Append($"Max lines per file: {SplitLinesPerFile}" + Environment.NewLine);
        outcomes.Append($"Estimated number of files to be created: {SplitFilesAmount}" + Environment.NewLine);

        outcomes.Append($"Error: {Error}" + Environment.NewLine);

        return outcomes.ToString();
    }
}

public class SplitFileOutcomeInfo
{
    private readonly SplitFileInfo _splitFileInfo;

    public SplitFileOutcomeInfo(SplitFileInfo splitFileInfo) => _splitFileInfo = splitFileInfo;

    public string Initialize()
    {
        try
        {
            SplitFilesAmountLength = SplitFilesAmount.ToString().Length;
            BaseFileName = Path.GetFileNameWithoutExtension(InputFilepath);
            FileExtension = Path.GetExtension(InputFilepath) ?? string.Empty;
            OutputDirectory = Path.GetDirectoryName(InputFilepath);

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                Warning.Add("Warning: Could not determine output directory from input file path. Using current directory for output files.");
                OutputDirectory = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            LineNumber = 0;
            FileCount = 0;
            OutputFiles = [];

            return string.Empty;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public string InputFilepath { get; set; } = string.Empty;
    public int TotalLinesPerFile { get; set; } = 0;
    public int SplitLinesPerFile { get; set; } = 0;
    public int SplitFilesAmount { get; set; } = 0;
    public string Error { get; internal set; } = string.Empty;

    public int LineNumber { get; internal set; } = 0;
    public int FileCount { get; internal set; } = 0;
    public List<string> OutputFiles { get; internal set; } = new List<string>();
    public string? OutputDirectory { get; internal set; } = string.Empty;
    public string FileExtension { get; internal set; }
    public string BaseFileName { get; internal set; }
    public List<string> Warning { get; internal set; }
    public string OutputFileFullPath { get; internal set; }
    public int SplitFilesAmountLength { get; internal set; }

    public override string ToString()
    {
        var outcomes = new StringBuilder();
        outcomes.Append($"Input file: {InputFilepath}" + Environment.NewLine);
        outcomes.Append($"Total lines in file: {TotalLinesPerFile}" + Environment.NewLine);
        outcomes.Append($"Max lines per file: {SplitLinesPerFile}" + Environment.NewLine);
        outcomes.Append($"Estimated number of files to be created: {SplitFilesAmount}" + Environment.NewLine);

        outcomes.Append($"Error: {Error}" + Environment.NewLine);

        return outcomes.ToString();
    }

    public string GetNewFilename()
    {
        FileCount += 1;
        OutputFileFullPath = Path.Combine(OutputDirectory, $"{BaseFileName}_{FileCount}{FileExtension}");

        OutputFiles.Add(OutputFileFullPath);

        return OutputFileFullPath;
    }
}
