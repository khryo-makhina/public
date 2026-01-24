using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TranslationTools;
using TextToSpeechApp.TextToSpeechService;

using var ttsEn = new TextToSpeechService("en-GB");
using var ttsFi = new TextToSpeechService("fi-FI");

//await ttsEn.SpeakAsync("Hello, this is the English voice.");
//await ttsFi.SpeakAsync("Hei, tämä on suomenkielinen ääni.");

List<TranslationEntry> translationEntryList = new List<TranslationEntry>();

if (args.Length > 0)
{
    Console.WriteLine("Arguments passed. Processing...");
    foreach (var arg in args)
    {
        Console.WriteLine(arg);

        if (arg == "--help" || arg == "-h")
        {
            Console.WriteLine("Usage: TextToSpeechApp [path to translations CSV file]"); 
            return;   
        }

        if (arg.Contains("csv_files"))
        {
            Console.WriteLine("Loading CSV key-value pairs from: " + arg);
            translationEntryList = TranslationEntryLoader.LoadTranslationEntriesFromCsv(arg);            
        }
        else
        {
            Console.WriteLine("Unknown argument: " + arg);
            return;
        }
    }    
}
else
{
    Console.WriteLine("No arguments passed. Using default translations CSV file.");
    Console.WriteLine("Loading CSV key-value pairs from: " + "translations_csv/translations.csv");
    translationEntryList = TranslationEntryLoader.LoadTranslationEntriesFromCsv("translations_csv/translations.csv");
}

Console.WriteLine("Starting text-to-speech for translation entries. Press Ctrl+C to stop.");
// Speak random entries in either English or Finnish
var entriesCount = translationEntryList.Count;
var entriesCountChars = entriesCount.ToString().Length;
var random1 = new Random(entriesCount);
int i = 0;
for (i = 0; i < entriesCount; i++)
{
    var randomChoice = random1.Next(0, entriesCount - 1);
    var entry = translationEntryList[randomChoice];
    var padding = (i + 1).ToString().PadLeft(entriesCountChars, '0');
    Console.WriteLine($"{padding} / {entriesCount} :: {entry.EnglishText} : {entry.FinnishText}");
    //Console.WriteLine($"EN: {entry.EnglishText}");
    await ttsEn.SpeakAsync(entry.EnglishText);

    //Console.WriteLine($"FI: {entry.FinnishText}");
    await ttsFi.SpeakAsync(entry.FinnishText);
}
