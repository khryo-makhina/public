using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TextToSpeechApp.TextToSpeechService;
using TranslationTools;

//await ttsEn.SpeakAsync("Hello, this is the English voice.");
//await ttsFi.SpeakAsync("Hei, tämä on suomenkielinen ääni.");

var translationEntryList = new TranslationEntryList();

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
            Console.WriteLine("Argument contains CSV file path: " + arg);

            var csvFilePath = arg.Substring(arg.IndexOf("csv_files"));
            Console.WriteLine("Loading CSV key-value pairs from refined file path: " + csvFilePath);
            translationEntryList = TranslationEntryLoader.LoadTranslationEntriesFromCsv(csvFilePath);            
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

bool isSingleLanguage = translationEntryList.IsSingleLanguage;

Console.WriteLine($"Source Language:  {translationEntryList.SourceLanguage}, Source Language Culture Name: {translationEntryList.SourceLanguageCultureName}");
Console.WriteLine($"Target Language:  {translationEntryList.TargetLanguage}, Target Language Culture Name: {translationEntryList.TargetLanguageCultureName}");

using var ttsSource = new TextToSpeechService(translationEntryList.SourceLanguageCultureName);
using var ttsTarget = isSingleLanguage ? null : new TextToSpeechService(translationEntryList.TargetLanguageCultureName);

// Speak random entries in either source language (e.g. English) and in target language (e.g. Finnish)
var entriesCount = translationEntryList.Count;
var entriesCountChars = entriesCount.ToString().Length;

var randomizer = new Random(entriesCount);
int i = 0;
for (i = 0; i < entriesCount; i++)
{
    var randomIndex = randomizer.Next(0, entriesCount - 1);
    var entry = translationEntryList[randomIndex];
    var padding = (i + 1).ToString().PadLeft(entriesCountChars, '0');

    if (!isSingleLanguage)    
    { 
        Console.WriteLine($"{padding} / {entriesCount} :: {entry.SourceText} : {entry.TargetText}");

        await ttsSource.SpeakAsync(entry.SourceText);
        await ttsTarget.SpeakAsync(entry.TargetText);
    }
    else
    {
        Console.WriteLine($"{padding} / {entriesCount} :: {entry.SourceText}");

        await ttsSource.SpeakAsync(entry.SourceText);
    }
}
