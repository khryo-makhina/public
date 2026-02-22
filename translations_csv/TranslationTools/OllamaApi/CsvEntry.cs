using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationTools.OllamaApi;

public class CsvEntry
{
    public string SourceText { get; set; } = string.Empty;

    public string TargetText { get; set; } = string.Empty;
}
