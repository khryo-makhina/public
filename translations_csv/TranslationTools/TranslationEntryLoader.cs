using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TranslationTools;

namespace TranslationTools;

public class TranslationEntryLoader 
{
    public static List<TranslationEntry> LoadTranslationEntriesFromCsv(string csvFilePath) 
    {
        var translationFinder = new TranslationTools.TranslationsFinder();
        string translationFilepath = translationFinder.FindTranslationsCsvFilepath(csvFilePath);
                
        string[] translationLines = translationFinder.GetTranslationsLines(translationFilepath);

        if (translationLines.Length == 0)
        {
            Console.WriteLine("No " + nameof(TranslationTools.TranslationEntry) + " lines found in the CSV file.");
            
            return new List<TranslationEntry>();
        }
        Console.WriteLine($"Loaded {translationLines.Length} lines from translations CSV file.");

        var translationsParser = new TranslationTools.TranslationsParser();
        var translationEntryList = translationsParser.ParseTranslationsCsvLines(translationLines);

        Console.WriteLine($"Loaded {translationEntryList.Count} translation entries.");
        return translationEntryList;
    }
}