// See https://aka.ms/new-console-template for more information
// namespace and class declaration are not needed for top-level statements in C# 9.0 and later,
// which is being used here. The code will be executed in the context of an implicit Program class with a Main method.

namespace TextFileSplitterApp;

internal class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await RunTextFileSplitterAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task RunTextFileSplitterAsync(string[] args)
    {
        var applicationArguments = ApplicationArguments.Parse(args);
        if (!applicationArguments.IsValid)
        {
            Console.WriteLine(applicationArguments.ValidationError);
            ShowHelp();
            return;
        }

        var splitter = new TextFileSplitter();
        var splitRequest = await GetSplitRequestAsync(splitter, applicationArguments);
        
        if (!splitRequest.IsValid)
        {
            Console.WriteLine($"File splitting cannot proceed due to error: {splitRequest.Error}");
            return;
        }

        Console.WriteLine(splitRequest.ToString());

        if (!ConfirmUserConsent())
        {
            Console.WriteLine("File splitting cancelled by user.");
            return;
        }

        var splitResult = await ExecuteSplitAsync(splitter, splitRequest);
        
        if (applicationArguments.FormatAsCsv && splitResult.Succeeded)
        {
            await FormatSplitFilesAsCsvAsync(splitter, splitResult);
        }
    }

    private static async Task<SplitRequestInfo> GetSplitRequestAsync(TextFileSplitter splitter, ApplicationArguments arguments)
    {
        return await splitter.GetSplittingInformation(arguments.InputFilePath, arguments.MaxLinesPerFile);
    }

    private static async Task<SplitProcessInfo> ExecuteSplitAsync(TextFileSplitter splitter, SplitRequestInfo splitRequest)
    {
        Console.WriteLine("Proceeding with file splitting...");
        var splitResult = await splitter.SplitFileAsync(splitRequest);
        
        Console.WriteLine("File splitting complete.");
        Console.WriteLine($"Details: {splitResult}");
        
        return splitResult;
    }

    private static async Task FormatSplitFilesAsCsvAsync(TextFileSplitter splitter, SplitProcessInfo splitResult)
    {
        Console.WriteLine("CSV option detected. Formatting the split files as CSV...");
        await splitter.FormatAsTranslationEntries(splitResult);
        Console.WriteLine("CSV formatting complete.");
    }

    private static bool ConfirmUserConsent()
    {
        Console.WriteLine("Do you want to proceed with splitting the file? (y/n)");
        var response = Console.ReadLine();
        return response != null && response.Equals("y", StringComparison.InvariantCultureIgnoreCase);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Usage: TextFileSplitter <input_file_path> <max_lines_per_file> <option: --csv>");
        Console.WriteLine("Example: TextFileSplitter \"C:\\path\\to\\input.txt\" 1000 --csv");
    }
}

/// <summary>
/// Represents parsed and validated command-line arguments for the TextFileSplitter application.
/// </summary>
public record ApplicationArguments
{
    public string InputFilePath { get; init; } = string.Empty;
    public int MaxLinesPerFile { get; init; }
    public bool FormatAsCsv { get; init; }
    public bool IsValid { get; init; }
    public string ValidationError { get; init; } = string.Empty;

    /// <summary>
    /// Parses command-line arguments and validates them.
    /// </summary>
    public static ApplicationArguments Parse(string[] args)
    {
        if (args.Length < 2)
        {
            return new ApplicationArguments
            {
                IsValid = false,
                ValidationError = "Error: Insufficient arguments provided."
            };
        }

        var inputFilePath = args[0];
        
        if (!int.TryParse(args[1], out var maxLinesPerFile))
        {
            return new ApplicationArguments
            {
                IsValid = false,
                ValidationError = "Error: Invalid value for max_lines_per_file. Please provide an integer."
            };
        }

        if (maxLinesPerFile is < TextFileSplitter.MinLinesPerFile or > TextFileSplitter.MaxLinesPerFile1)
        {
            return new ApplicationArguments
            {
                IsValid = false,
                ValidationError = $"Error: max_lines_per_file must be between {TextFileSplitter.MinLinesPerFile} and {TextFileSplitter.MaxLinesPerFile1}."
            };
        }

        var formatAsCsv = args.Length > 2 && args[2].Equals("--csv", StringComparison.InvariantCultureIgnoreCase);

        return new ApplicationArguments
        {
            InputFilePath = inputFilePath,
            MaxLinesPerFile = maxLinesPerFile,
            FormatAsCsv = formatAsCsv,
            IsValid = true
        };
    }
}
