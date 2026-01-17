using System;
using System.IO;

static string EscapeCsv(string s) => s?.Replace("\"", "\"\"") ?? string.Empty;

var csvPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "translations.csv"));
Console.WriteLine($"Appending new entries to: {csvPath}");

while (true)
{
    Console.Write("Enter English text (leave empty to quit): ");
    var english = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(english))
        break;

    var line = $"\"{EscapeCsv(english)}\",\"\"";
    try
    {
        File.AppendAllText(csvPath, line + Environment.NewLine);
        Console.WriteLine("Added (Finnish left empty). Waiting for next input...");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to append: {ex.Message}");
        break;
    }
}

Console.WriteLine("Exiting. Goodbye.");
