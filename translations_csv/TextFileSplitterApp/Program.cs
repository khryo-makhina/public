// See https://aka.ms/new-console-template for more information
// namespace and class declaration are not needed for top-level statements in C# 9.0 and later,
// which is being used here. The code will be executed in the context of an implicit Program class with a Main method.

using TextFileSplitterApp;

if (args.Length < 2)
{
    Console.WriteLine("Usage: TextFileSplitter <input_file_path> <max_lines_per_file> <option: --csv>");
    Console.WriteLine("Example: TextFileSplitter \"C:\\path\\to\\input.txt\" 1000 --csv");
    return;
}

string inputFilepath = args[0];
int lines;
if (!int.TryParse(args[1], out lines))
{
    Console.WriteLine("Error: Invalid value for max_lines_per_file. Please provide an integer.");
    return;
}

if (lines is < TextFileSplitter.MinLinesPerFile or > TextFileSplitter.MaxLinesPerFile1)
{
    Console.WriteLine("Error: max_lines_per_file must be between 2 and 10000.");
    return;
}

try
{
    TextFileSplitter splitter = new TextFileSplitter();
    var splitFileInfo = await splitter.GetSplittingInformation(inputFilepath, lines);

    if (!String.IsNullOrEmpty(splitFileInfo.Error))
    {
        Console.WriteLine($"File splitting cannot proceed due to error: {splitFileInfo.Error}");
        return;
    }

    Console.WriteLine("Do you want to proceed with splitting the file? (y/n)");
    var kbRes = Console.ReadLine();
    if (kbRes == null || !kbRes.Equals("y", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine("File splitting cancelled by user.");
        return;
    }

    Console.WriteLine("Proceeding with file splitting...");
    SplitFileOutcomeInfo splitResult = await splitter.SplitFileAsync(splitFileInfo);

    Console.WriteLine($"File splitting complete.");
    Console.WriteLine($"Details: {splitResult}");

    if (args.Length > 2 && args[2].Equals("--csv", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine("CSV option detected. Formatting the split files as CSV...");
        await splitter.FormatAsTranslationEntries(splitResult);
        Console.WriteLine("CSV formatting complete.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}