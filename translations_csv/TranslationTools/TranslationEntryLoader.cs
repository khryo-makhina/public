using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TranslationTools;

namespace TranslationTools;

public class TranslationEntryLoader 
{
    public static TranslationEntryList LoadTranslationEntriesFromCsv(string csvFilePath, int startinglineNumber = -1) 
    {
        var translationFinder = new TranslationsFinder();
        string translationFilepath = translationFinder.FindTranslationsCsvFilepath(csvFilePath);
                
        string[] translationLines = translationFinder.GetTranslationsLines(translationFilepath, startinglineNumber);

        if (translationLines.Length == 0)
        {
            ConsoleLogger.WriteLine("No " + nameof(TextEntry) + " lines found in the CSV file.");
            
            return new TranslationEntryList();
        }
        ConsoleLogger.WriteLine($"Loaded {translationLines.Length} lines from translations CSV file - one is expected to be a header line.");

        var translationsParser = new TranslationsParser();
        var translationEntryList = translationsParser.ParseTranslationsCsvLines(translationLines);

        ConsoleLogger.WriteLine($"Loaded {translationEntryList.Count} translation entries.");
        return translationEntryList;
    }
}