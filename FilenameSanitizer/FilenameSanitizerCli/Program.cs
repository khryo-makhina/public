using FilenameSanitizer;

var (folder, patterns) = ParseArguments(args);

if (folder == null)
{
    ShowHelp();
    return;
}

try
{
    var sanitizer = new FilenameSanitizer(folder);
    FilenameSanitationOperation operation;

    if (patterns != null)
    {
        Console.WriteLine($"Removing patterns from filenames in: {folder}");
        Console.WriteLine("Patterns to remove:");
        Console.WriteLine(patterns);
        operation = sanitizer.RenameFilesRemovingPatterns(patterns);
    }
    else
    {
        Console.WriteLine($"Sanitizing filenames in: {folder}");
        operation = sanitizer.RenameFilesToMeetOsRequirements();
    }

    // Output results
    if (operation.Log.HasErrors)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var error in operation.Log.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
        Console.ResetColor();
    }

    if (operation.Log.HasWarnings)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var warning in operation.Log.Warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }
        Console.ResetColor();
    }

    if (operation.Log.HasInfo)
    {
        foreach (var info in operation.Log.Info)
        {
            Console.WriteLine($"Info: {info}");
        }
    }

    // Write log file to the target folder
    operation.Log.FlushToFile(folder);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    return;
}

static (string? folder, string? patterns) ParseArguments(string[] args)
{
    if (args.Length == 0) return (null, null);

    var folder = args[0];
    var patterns = args.Length > 1 ? string.Join(Environment.NewLine, args[1..]) : null;

    return (folder, patterns);
}

static void ShowHelp()
{
    Console.WriteLine("FilenameSanitizerCli - Sanitize filenames for OS compatibility");
    Console.WriteLine("\nUsage:");
    Console.WriteLine("  FilenameSanitizerCli <folder>                    - Sanitize all filenames in folder");
    Console.WriteLine("  FilenameSanitizerCli <folder> <pattern1> ...     - Remove patterns from filenames");
    Console.WriteLine("\nExamples:");
    Console.WriteLine(@"  FilenameSanitizerCli C:\MyFiles");
    Console.WriteLine(@"  FilenameSanitizerCli C:\MyFiles prefix- _old .bak");
}
