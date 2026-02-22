// See https://aka.ms/new-console-template for more information
// namespace and class declaration are not needed for top-level statements in C# 9.0 and later,
// which is being used here. The code will be executed in the context of an implicit Program class with a Main method.

using System;
using TextFileSplitterApp;

if (args.Length < 2)
{
    Console.WriteLine("Usage: TextFileSplitter <input_file_path> <max_lines_per_file>");
    return;
}

string inputFilePath = args[0];
int maxLinesPerFile;

if (!int.TryParse(args[1], out maxLinesPerFile))
{
    Console.WriteLine("Error: Invalid value for max_lines_per_file.  Please provide an integer.");
    return;
}

try
{
    TextFileSplitter splitter = new TextFileSplitter();
    var splittingInformation = await splitter.GetSplittingInformation(inputFilePath, maxLinesPerFile);
    Console.WriteLine(splittingInformation);
    Console.WriteLine("Do you want to proceed with splitting the file? (y/n)");
    var kbRes = Console.ReadLine();
    if (kbRes == null || !kbRes.Equals("y", StringComparison.InvariantCultureIgnoreCase))
    {
        Console.WriteLine("File splitting cancelled by user.");
        return;
    }

    Console.WriteLine("Proceeding with file splitting...");
    var splitResult = await splitter.SplitFileAsync(inputFilePath, maxLinesPerFile);

    Console.WriteLine($"File splitting complete.");
    Console.WriteLine($"Details: {splitResult}");

    // After splitting, format the files.  Get the list of files that were created.
    // This part needs to be updated based on how the SplitFileAsync method returns the file paths.
    // For now, I'm assuming it returns a list of file paths directly.
    Console.WriteLine("Formatting the split files...");
    await splitter.FormatAsTranslationEntries(splitResult);
    Console.WriteLine("File formatting complete.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}