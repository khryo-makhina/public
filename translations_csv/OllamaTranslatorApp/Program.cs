// See https://aka.ms/new-console-template for more information

using System.Text;
using TranslationTools.OllamaApi;

Console.OutputEncoding = Encoding.UTF8;

var aiTranslator = new OllamaTranslator();

await aiTranslator.ProcessCsvAsync(
    Path.Combine(Directory.GetCurrentDirectory(), @"csv_files\input.csv"),
    Path.Combine(Directory.GetCurrentDirectory(), @"csv_files\output.csv")
);
