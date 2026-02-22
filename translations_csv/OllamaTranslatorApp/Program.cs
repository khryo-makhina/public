// See https://aka.ms/new-console-template for more information

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using TranslationTools;
using TranslationTools.OllamaApi;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var aiTranslator = new OllamaTranslator();

var timeStampString = DateTime.Now.ToString("yyyy-MM-dddd_HH-mm-ss");
await aiTranslator.ProcessCsvAsync(
    inputFilepath: Path.Combine(Directory.GetCurrentDirectory(), $@"csv_files\input.csv"),
    outputFilepath: Path.Combine(Directory.GetCurrentDirectory(), $@"csv_files\output.csv")
);
