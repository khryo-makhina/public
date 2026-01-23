using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TranslationTools;
using TextToSpeechApp.TextToSpeechService;

using var ttsEn = new TextToSpeechService("en-GB");
using var ttsFi = new TextToSpeechService("fi-FI");

await ttsEn.SpeakAsync("Hello, this is the English voice.");
await ttsFi.SpeakAsync("Hei, tämä on suomenkielinen ääni.");

var translationFinder = new TranslationTools.TranslationsFinder();
var translationFilepath = translationFinder.FindTranslationsCsvFilepath();
string[] translationLines = translationFinder.GetTranslationsLines(translationFilepath);

if (translationLines.Length == 0)
{
    Console.WriteLine("No translation lines found in the CSV file.");
    return;
}
Console.WriteLine($"Loaded {translationLines.Length} lines from translations CSV file.");

var translationsParser = new TranslationTools.TranslationsParser();
var translationEntryList = translationsParser.ParseTranslationsCsvLines(translationLines);

Console.WriteLine($"Loaded {translationEntryList.Count} translation entries.");

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
