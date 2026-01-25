using System;
using System.Linq;
using TextToSpeechApp;
using TranslationTools;

//await ttsEn.SpeakAsync("Hello, this is the English voice.");
//await ttsFi.SpeakAsync("Hei, tämä on suomenkielinen ääni.");

var entriesLoadedAmount = 0;

var translationEntryList = new TranslationEntryList();
int startinglineNumber = -1;

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("Usage: TextToSpeechApp [path to translations CSV file]");
    return;
}

string csvFilePath = "translations_csv/translations.csv";

if (args.Length == 0)
{
    Console.WriteLine("No arguments passed. Using default relative translations CSV file: " + csvFilePath);   
}
else 
{ 
    Console.WriteLine("Arguments passed. Processing...");
    foreach (var arg in args)
    {
        Console.WriteLine(arg);

        if (arg.Contains(".csv", StringComparison.InvariantCultureIgnoreCase))
        {
            if (arg.Contains("csv_files"))
            {
                Console.WriteLine("Argument contains CSV file path: " + arg);

                var resolvedFilePath = arg.Substring(arg.IndexOf("csv_files"));
                if (!System.IO.File.Exists(resolvedFilePath))
                {
                    Console.WriteLine("Error! CSV file not found from resolved file path '" + resolvedFilePath + "' of `" + arg + "`.");
                    return;
                }

                Console.WriteLine("Loading CSV key-value pairs will be loaded from resolved file path '" + resolvedFilePath + "' of `" + arg + "`.");
                csvFilePath = resolvedFilePath;
            }
            else if (arg.EndsWith(".csv"))
            {
                if (!System.IO.File.Exists(arg))
                {
                    Console.WriteLine("Error! CSV file not found: " + arg);
                    return;
                }

                Console.WriteLine("CSV key-value pairs will be loaded from file path '" + csvFilePath + "'.");
                csvFilePath = arg;
            }
            continue;
        }

        if (int.TryParse(arg, out startinglineNumber))
        {
            Console.WriteLine("Argument indicates line number to read: " + startinglineNumber);
            continue;
        }

        Console.WriteLine("Warning! Unknown argument: " + arg);
    }    
}

Console.WriteLine("Loading CSV entries from '" + csvFilePath + "' ...");
translationEntryList = TranslationEntryLoader.LoadTranslationEntriesFromCsv(csvFilePath, startinglineNumber);
entriesLoadedAmount = translationEntryList.Count;
Console.WriteLine("Loaded entries: " + entriesLoadedAmount);

if (entriesLoadedAmount == 0)
{
    Console.WriteLine("No translation entries found. Exiting.");
    return;
}

Console.WriteLine("Starting text-to-speech for translation entries. Press Ctrl+C to stop.");

bool isSingleLanguage = translationEntryList.IsSingleLanguage;

Console.WriteLine($"Source Language:  {translationEntryList.SourceLanguage}, Source Language Culture Name: {translationEntryList.SourceLanguageCultureName}");
Console.WriteLine($"Target Language:  {translationEntryList.TargetLanguage}, Target Language Culture Name: {translationEntryList.TargetLanguageCultureName}");

using var ttsSource = new TextToSpeechService(translationEntryList.SourceLanguageCultureName);
using var ttsTarget = isSingleLanguage ? null : new TextToSpeechService(translationEntryList.TargetLanguageCultureName);

string textExitPrompt = "Press Escape to stop or Enter to pause for 10 seconds.";
Console.WriteLine(textExitPrompt);
await ttsSource.SpeakAsync(textExitPrompt);

string textStartingRecitingLoop = "Reciting " + entriesLoadedAmount + " entries, randomly.";
Console.WriteLine(textStartingRecitingLoop);
await ttsSource.SpeakAsync(textStartingRecitingLoop);

var entriesCountChars = entriesLoadedAmount.ToString().Length;

// Speak random entries in either source language (e.g. English) and in target language (e.g. Finnish)
var randomizer = new Random(entriesLoadedAmount);
int i = 0;
for (i = 0; i < entriesLoadedAmount; i++)
{
    // Check if a key has been pressed
    if (Console.KeyAvailable)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Escape)
        {
            Console.WriteLine("Esc pressed — stopping speech. Exiting.");
            break;
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine("Enter pressed — pausing for 10 seconds.");
            System.Threading.Thread.Sleep(10000);
        }
    }    

    var randomIndex = randomizer.Next(0, entriesLoadedAmount - 1);
    var entry = translationEntryList[randomIndex];
    var padding = (i + 1).ToString().PadLeft(entriesCountChars, '0');

    if (!isSingleLanguage)    
    { 
        Console.WriteLine($"{padding} / {entriesLoadedAmount} :: {entry.SourceText} : {entry.TargetText}");

        await ttsSource.SpeakAsync(entry.SourceText);
        await ttsTarget.SpeakAsync(entry.TargetText);
    }
    else
    {
        Console.WriteLine($"{padding} / {entriesLoadedAmount} :: {entry.SourceText}");

        await ttsSource.SpeakAsync(entry.SourceText);
    }

    // avoid burning CPU
    System.Threading.Thread.Sleep(50);
}
