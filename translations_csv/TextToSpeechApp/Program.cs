using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextToSpeechApp;
using TranslationTools;

Console.OutputEncoding = Encoding.UTF8;
int startingLineNumber;
var csvFilePath = ParseArguments(args, out startingLineNumber);

Console.WriteLine("Loading CSV entries from '" + csvFilePath + "' ...");
TranslationEntryList entryList = TranslationEntryLoader.LoadTranslationEntriesFromCsv(csvFilePath, startingLineNumber);
ConsoleLogger.Logs.ForEach(log => Console.WriteLine(log.LogText));

var entriesLoadedAmount = entryList.Count;
Console.WriteLine("Loaded entries: " + entriesLoadedAmount);

if (entriesLoadedAmount == 0)
{
    Console.WriteLine("No translation entries found. Exiting.");
    return;
}

await RunRecitingLoop(entryList);

/// <summary>
/// Parses command-line arguments to determine the translations CSV file path
/// and an optional starting line number.
/// </summary>
/// <param name="args">The command-line arguments passed to the application.</param>
/// <param name="startingLineNumber">Outputs the parsed starting line number or -1 if none provided.</param>
/// <returns>The resolved CSV file path to load.</returns>
static string ParseArguments(string[] args, out int startingLineNumber)
{
    startingLineNumber = -1;
    if (args.Contains("--help") || args.Contains("-h"))
    {
        Console.WriteLine("Usage: TextToSpeechApp [path to translations CSV file]");
        Environment.Exit(0);
    }

    var csvFilePath = "translations_csv/translations.csv";

    if (args.Length == 0)
    {
        Console.WriteLine("No arguments passed. Using default relative translations CSV file: " + csvFilePath);
        return csvFilePath;
    }

    Console.WriteLine("Arguments passed. Processing...");
    foreach (var arg in args)
    {
        Console.WriteLine(arg);

        if (arg.Contains(".csv", StringComparison.InvariantCultureIgnoreCase))
        {
            if (arg.Contains("csv_files"))
            {
                Console.WriteLine("Argument contains CSV file path: " + arg);

                var resolvedFilePath = arg[arg.IndexOf("csv_files", StringComparison.Ordinal)..];
                if (!File.Exists(resolvedFilePath))
                {
                    Console.WriteLine("Error! CSV file not found from resolved file path '" + resolvedFilePath +
                                      "' of `" + arg + "`.");
                    Environment.Exit(1);
                }

                Console.WriteLine("Loading CSV key-value pairs will be loaded from resolved file path '" +
                                  resolvedFilePath + "' of `" + arg + "`.");
                csvFilePath = resolvedFilePath;
            }
            else if (arg.EndsWith(".csv"))
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
#pragma warning disable IDE0045 // Convert to conditional expression
                if (arg.Contains("csv_files"))
                {
                    csvFilePath = arg;
                }
                else
                {
                    csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "csv_files", arg);
                }
#pragma warning restore IDE0045 // Convert to conditional expression

                if (!File.Exists(csvFilePath))
                {
                    Console.WriteLine("Error! CSV file not found: " + csvFilePath);
                    Environment.Exit(1);
                }

                Console.WriteLine("CSV key-value pairs will be loaded from file path '" + csvFilePath + "'.");
            }

            continue;
        }

        if (int.TryParse(arg, out startingLineNumber))
        {
            Console.WriteLine("Argument indicates line number to read: " + startingLineNumber);
            continue;
        }

        Console.WriteLine("Warning! Unknown argument: " + arg);
    }

    return csvFilePath;
}

/// <summary>
/// Checks for a pressed console key and handles control commands.
/// Returns <c>true</c> when the user requested to stop execution (Escape),
/// otherwise <c>false</c>. Pressing Enter will pause execution for 10 seconds.
/// </summary>
/// <returns><c>true</c> if execution should stop; otherwise <c>false</c>.</returns>
static bool HandleConsoleKey()
{
    if (!Console.KeyAvailable)
    {
        return false;
    }

    ConsoleKeyInfo key = Console.ReadKey(true);

    if (key.Key == ConsoleKey.Escape)
    {
        Console.WriteLine("Esc pressed — stopping speech. Exiting.");
        return true;
    }

    if (key.Key == ConsoleKey.Enter)
    {
        Console.WriteLine("Enter pressed — pausing for 10 seconds.");
        Thread.Sleep(10000);
    }

    return false;
}

/// <summary>
/// Runs the main reciting loop that speaks translation entries using
/// the text-to-speech service. The loop selects entries randomly and
/// responds to console key input to pause or stop.
/// </summary>
/// <param name="entryList">The list of translation entries to recite.</param>
/// <returns>A task that represents the asynchronous reciting operation.</returns>
static async Task RunRecitingLoop(TranslationEntryList entryList)
{
    Console.WriteLine("Starting text-to-speech for translation entries. Press Ctrl+C to stop.");

    var textExitPrompt = "Press Escape to stop or Enter to pause for 10 seconds.";
    Console.WriteLine(textExitPrompt);

    using var textToSpeechService = new TextToSpeechService(entryList.VoiceLanguages, entryList.Entries);

    var entriesLoadedAmount = entryList.Count;
    var textStartingRecitingLoop = "Reciting " + entriesLoadedAmount + " entries, randomly.";
    Console.WriteLine(textStartingRecitingLoop);
    await textToSpeechService.SpeakTextAsync(textStartingRecitingLoop);

    var entriesCountChars = entriesLoadedAmount.ToString().Length;

    // Speak random entries in either source language (e.g. English) and in target language (e.g. Finnish)
    var randomizer = new Random(entriesLoadedAmount);
    for (var i = 0; i < entriesLoadedAmount; i++)
    {
        // Check if a key has been pressed
        if (HandleConsoleKey())
        {
            break;
        }

        var randomIndex = randomizer.Next(0, entriesLoadedAmount - 1);
        TextEntryRow entryRow = entryList[randomIndex];
        var padding = (i + 1).ToString().PadLeft(entriesCountChars, '0');

        Console.WriteLine($"{padding} / {entriesLoadedAmount} :: {entryRow.GetAllAsString()}");

        foreach (TextEntry entry in entryRow)
        {
            await textToSpeechService.SpeakEntryAsync(entry);
        }

        // avoid burning CPU
        Thread.Sleep(50);
    }
}