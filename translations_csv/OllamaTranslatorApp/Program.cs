// See https://aka.ms/new-console-template for more information

using System.Text;
using TranslationTools.OllamaApi;

Console.OutputEncoding = Encoding.UTF8;

try
{
    if (args.Length == 0)
    {
        ShowHelp();
        return;
    }

    if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
    {
        ShowHelp();
        return;
    }

    var sourceTargetList = new Dictionary<string, string>();

    if (args[0] == "--folder")
    {
        var fileList = Directory.GetFiles(args[1]);
        foreach (var file in fileList)
        {
            var source = file;
            var target = string.Empty;

            var dotIndex = file.IndexOf('.');
            var extension = string.Empty;
            if (dotIndex >= 1)
            {
                extension = file[dotIndex..];
                target = file[..dotIndex];
            }
            else
            {
                extension = ".output.csv";
                target = file + extension;
            }

            sourceTargetList.Add(source, target);
        }

        Console.WriteLine($"Source files found: " + sourceTargetList.Count);
    }
    else
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Error: Invalid arguments. Use --help for syntax.");
            ShowHelp();
            return;
        }
        string sourcePath = args[0];
        string targetPath = args[1];

        Console.WriteLine($"Source file: '{sourcePath}', target file: {targetPath}");
        sourceTargetList.Add(sourcePath, targetPath);
    }

    var aiTranslator = new OllamaTranslator();

    foreach (var sourceTarget in sourceTargetList)
    {
        var source = sourceTarget.Key;
        var target = sourceTarget.Value;

        if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target))
        {
            Console.WriteLine($"Error: required source '{source}' or target '{target}' missing.");
            continue;
        }

        if (File.Exists(source))
        {
            Console.WriteLine($"Error: required source '{source}' does not exist.");
            continue;
        }

        Console.WriteLine($"translating '{source}' ... ");
        await aiTranslator.ProcessCsvAsync(source, source);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

static void ShowHelp()
{
    Console.WriteLine("Ollama Translator Tool");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  OllamaTranslatorApp <source.csv> <target.csv>");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <source.csv>   File path to the input CSV file");
    Console.WriteLine("  <target.csv>   File path where the translated CSV will be saved");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --help, -h     Show this help message");
    Console.WriteLine("  --folder       The folder containing source .csv files");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  OllamaTranslatorApp --help");
    Console.WriteLine("  OllamaTranslatorApp --folder my_folder");
    Console.WriteLine("  OllamaTranslatorApp input.csv output.csv");
}
