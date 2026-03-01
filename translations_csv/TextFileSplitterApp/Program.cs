// See https://aka.ms/new-console-template for more information
// namespace and class declaration are not needed for top-level statements in C# 9.0 and later,
// which is being used here. The code will be executed in the context of an implicit Program class with a Main method.

namespace TextFileSplitterApp;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var (success, inputFilepath, lines) = ParseAndValidateArguments(args);
        if (!success)
        {
            return;
        }

        try
        {
            var splitter = new TextFileSplitter();
            var splitFileInfo = await splitter.GetSplittingInformation(inputFilepath, lines);

            if (splitFileInfo.Error.Count > 0)
            {
                Console.WriteLine($"File splitting cannot proceed due to error: {splitFileInfo.Error}");
                return;
            }

            Console.WriteLine(splitFileInfo.ToString());

            if (!ConfirmUserConsent())
            {
                Console.WriteLine("File splitting cancelled by user.");
                return;
            }

            SplitProcessInfo splitResult = await RunSplitAsync(new TextFileSplitter(), splitFileInfo);

            if (args.Length > 2 && args[2].Equals("--csv", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!splitResult.ErrorList.Any() && splitResult.OutputFiles.Count > 0)
                {

                    Console.WriteLine("CSV option detected. Formatting the split files as CSV...");
                    await splitter.FormatAsTranslationEntries(splitResult);
                    Console.WriteLine("CSV formatting complete.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses and validates command-line arguments for the application.
    /// Returns a tuple indicating success, the resolved input file path and the number of lines.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>
    /// A tuple: (Success, InputFilepath, Lines). If Success is false the program should exit.
    /// </returns>
    private static (bool Success, string InputFilepath, int Lines) ParseAndValidateArguments(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: TextFileSplitter <input_file_path> <max_lines_per_file> <option: --csv>");
            Console.WriteLine("Example: TextFileSplitter \"C:\\path\\to\\input.txt\" 1000 --csv");
            return (false, string.Empty, 0);
        }

        var inputFilepath = args[0];
        if (!int.TryParse(args[1], out var lines))
        {
            Console.WriteLine("ErrorList: Invalid value for max_lines_per_file. Please provide an integer.");
            return (false, string.Empty, 0);
        }

        if (lines is < TextFileSplitter.MinLinesPerFile or > TextFileSplitter.MaxLinesPerFile1)
        {
            Console.WriteLine($"ErrorList: max_lines_per_file must be between {TextFileSplitter.MinLinesPerFile} and {TextFileSplitter.MaxLinesPerFile1}.");
            return (false, string.Empty, 0);
        }

        return (true, inputFilepath, lines);
    }

    /// <summary>
    /// Prompts the user for confirmation to proceed with splitting.
    /// </summary>
    /// <returns><c>true</c> if the user confirms; otherwise <c>false</c>.</returns>
    private static bool ConfirmUserConsent()
    {
        Console.WriteLine("Do you want to proceed with splitting the file? (y/n)");
        var kbRes = Console.ReadLine();
        return kbRes != null && kbRes.Equals("y", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Executes the split operation
    ///  and optionally formats the output as CSV when requested.
    /// </summary>
    /// <param name="splitter">The <see cref="TextFileSplitter"/> used to perform the split.</param>
    /// <param name="splitFileInfo">The split request information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task<SplitProcessInfo> RunSplitAsync(TextFileSplitter splitter, SplitRequestInfo splitFileInfo)
    {
        Console.WriteLine("Proceeding with file splitting...");
        SplitProcessInfo splitResult = await splitter.SplitFileAsync(splitFileInfo);

        Console.WriteLine("File splitting complete.");
        Console.WriteLine($"Details: {splitResult}");

        return splitResult;
    }
}