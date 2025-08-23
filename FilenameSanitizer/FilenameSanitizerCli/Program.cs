using FilenameSanitizer;

var (folder, semicommaSeparatedPatterns) = ParseArguments(args);

if (folder == null)
{
    ShowHelp();
    return;
}

IFilenameSanitizer filenameSanitizer = new FilenameSanitizer.FilenameSanitizer(folder);

try
{
    if (folder == ".")
    {
        folder = Environment.CurrentDirectory; // Default to current directory if no folder is specified
    }

    if (!Directory.Exists(folder))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: The specified folder '{folder}' does not exist or cannot access it.");
        filenameSanitizer.Logger.Errors.Add($"The specified folder '{folder}' does not exist or cannot access it.");
        Console.ResetColor();
        return;
    }
    
    if (!string.IsNullOrEmpty(semicommaSeparatedPatterns))
    {        
        Console.WriteLine("Using the patterns provided in the command line parameter to remove:");
        Console.WriteLine(semicommaSeparatedPatterns);
        filenameSanitizer.Logger.Info.Add("Using the patterns provided in the command line parameter to remove:");
        filenameSanitizer.Logger.Info.Add(semicommaSeparatedPatterns);
        
        filenameSanitizer.RenameFilesRemovingPatterns(semicommaSeparatedPatterns);
    }
    else
    {        
        Console.WriteLine($"Sanitizing filenames in: {folder}");
        filenameSanitizer.Logger.Info.Add($"Sanitizing filenames in: {folder}");
        filenameSanitizer.RenameFilesToMeetOsRequirements();
    }

    // Output results
    ConsoleOutputLogs(filenameSanitizer.Logger);

    // Write log file to the target folder
    filenameSanitizer.Logger?.FlushToFile();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    filenameSanitizer.Logger.Errors.Add(ex.Message);
    Console.ResetColor();
    return;
}
finally
{
    Console.WriteLine("Operation completed.");
    Console.WriteLine($"Log file written to: {filenameSanitizer.Logger.LogFilePath}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

static (string folder, string semicommaSeparatedPatterns) ParseArguments(string[] args)
{
    if (args.Length == 0)
    {
        return (string.Empty, string.Empty);
    }
    var separator = args.Length > 1 ? ";" : " ";
    var folder = args[0];
    var semicommaSeparatedPatterns = args.Length > 1 ? string.Join(separator, args[1..]) : string.Empty;

    return (folder, semicommaSeparatedPatterns);
}

static void ShowHelp()
{
    Console.WriteLine("FilenameSanitizerCli - Sanitize filenames for OS compatibility");
    Console.WriteLine("Default settings file: " + SanitizerConstants.SanitizerSettingsFile);
    Console.WriteLine("Default replace patterns file: " + SanitizerConstants.SanitizerReplacePatternsFile);
    Console.WriteLine("File path over " + SanitizerConstants.MaxPosixNameLength + " characters, will be cut off.");
    
    Console.WriteLine("\nUsage:");
    Console.WriteLine("  FilenameSanitizerCli                     - Sanitize all filenames in folder - in current directory.");
    Console.WriteLine("  FilenameSanitizerCli <folder>            - Sanitize all filenames in folder, using settings in setting files.");
    Console.WriteLine("  FilenameSanitizerCli <folder> <patterns> - Remove patterns from filenames, separated with semicolons");
    Console.WriteLine("\nExamples:");
    Console.WriteLine(@"  FilenameSanitizerCli .");
    Console.WriteLine(@"  FilenameSanitizerCli C:\MyFiles");
    Console.WriteLine(@"  FilenameSanitizerCli C:\MyFiles prefix-;_old;.bak");
}

static void ConsoleOutputLogs(OperationLogger logger)
{    
    if (logger.HasErrors)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var error in logger.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
        Console.ResetColor();
    }

    if (logger.HasWarnings)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var warning in logger.Warnings)
        {
            Console.WriteLine($"Warning: {warning}");
        }
        Console.ResetColor();
    }

    if (logger.HasInfo)
    {
        foreach (var info in logger.Info)
        {
            Console.WriteLine($"Info: {info}");
        }
    }
}