// See https://aka.ms/new-console-template for more information

using System.Text;
using TranslationTools.OllamaApi;

namespace OllamaTranslatorApp;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            await RunTranslationAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }

        
    }
    private static async Task RunTranslationAsync(string[] args)
    {
        if (ShouldShowHelp(args))
        {
            ShowHelp();
            return;
        }

        var translationRequests = ParseCommandLineArguments(args);
        if (translationRequests.Count == 0)
        {
            Console.WriteLine("Error: No valid translation requests found.");
            ShowHelp();
            return;
        }

        var aiTranslator = new OllamaTranslator();
        await ProcessTranslationRequestsAsync(aiTranslator, translationRequests);
    }

    private static bool ShouldShowHelp(string[] args)
    {
        return args.Length == 0 ||
               (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"));
    }

    private static List<TranslationRequest> ParseCommandLineArguments(string[] args)
    {
        var requests = new List<TranslationRequest>();

        if (args[0] == "--folder")
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Folder path required after --folder option.");
                return requests;
            }

            var folderPath = args[1];
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Error: Folder '{folderPath}' does not exist.");
                return requests;
            }

            var csvFiles = Directory.GetFiles(folderPath, "*.csv");
            Console.WriteLine($"Found {csvFiles.Length} CSV files in folder '{folderPath}'.");

            foreach (var file in csvFiles)
            {
                var targetPath = GenerateTargetFilePath(file);
                requests.Add(new TranslationRequest(file, targetPath));
            }
        }
        else
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Error: Invalid arguments. Use --help for syntax.");
                return requests;
            }

            var sourcePath = args[0];
            var targetPath = args[1];
            requests.Add(new TranslationRequest(sourcePath, targetPath));
        }

        return requests;
    }

    private static string GenerateTargetFilePath(string sourceFilePath)
    {
        var directory = Path.GetDirectoryName(sourceFilePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
        var targetFileName = $"{fileNameWithoutExtension}.translated.csv";

        return Path.Combine(directory ?? string.Empty, targetFileName);
    }

    private static async Task ProcessTranslationRequestsAsync(OllamaTranslator translator, List<TranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            if (!ValidateTranslationRequest(request))
            {
                continue;
            }

            Console.WriteLine($"Translating '{request.SourcePath}' to '{request.TargetPath}'...");

            try
            {
                await translator.ProcessCsvAsync(request.SourcePath, request.TargetPath);
                Console.WriteLine($"Successfully translated '{request.SourcePath}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing '{request.SourcePath}': {ex.Message}");
            }
        }
    }

    private static bool ValidateTranslationRequest(TranslationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath) || string.IsNullOrWhiteSpace(request.TargetPath))
        {
            Console.WriteLine("Error: Source or target path is empty.");
            return false;
        }

        if (!File.Exists(request.SourcePath))
        {
            Console.WriteLine($"Error: Source file '{request.SourcePath}' does not exist.");
            return false;
        }

        var sourceExtension = Path.GetExtension(request.SourcePath);
        if (!string.Equals(sourceExtension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Warning: Source file '{request.SourcePath}' may not be a CSV file.");
        }

        return true;
    }

    private static void ShowHelp()
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

    internal record TranslationRequest(string SourcePath, string TargetPath);
}