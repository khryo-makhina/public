using System.Text;
using System.Globalization;
using System.Linq;

namespace TranslationTools;

public class TranslationEntryList
{
    public string SourceLanguage { get; set; } = "English";
    public string TargetLanguage { get; set; } = "Finnish";

    private CultureInfo GetCultureFromLanguage(string languageName)
    {
        // Step 1: Find the neutral culture (e.g., "English" → "en")
        var neutral = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
            .FirstOrDefault(c =>
                c.EnglishName.Equals(languageName, StringComparison.OrdinalIgnoreCase));

        if (neutral == null)
            return CultureInfo.CurrentCulture; // Fallback to current culture if not found

        // Step 2: Convert neutral → specific culture using CreateSpecificCulture
        // This uses the machine's default mapping (e.g., "en" → "en-GB" on UK systems)
        return CultureInfo.CreateSpecificCulture(neutral.Name);
    }

    public string SourceLanguageCultureName
    {
        get
        {
            var culture = GetCultureFromLanguage(SourceLanguage);
            return culture?.Name ?? "en-GB";
        }
    }
    public string TargetLanguageCultureName
    {
        get
        {
            var culture = GetCultureFromLanguage(TargetLanguage);
            return culture?.Name ?? "fi-FI";
        }
    }

    public bool IsEmpty()
    {
        return Entries.Count == 0;
    }

    public int Count => Entries.Count;

    public void Clear()
    {
        Entries.Clear();
    }

    public List<TranslationEntry> Entries { get; set; } = new List<TranslationEntry>();
    public bool IsSingleLanguage
    {
        get
        {
            var isSourceOnly = !String.IsNullOrEmpty(SourceLanguage) && String.IsNullOrEmpty(TargetLanguage);
            return isSourceOnly;
        }
    }

    public void Add(TranslationEntry entry) => Entries.Add(entry);
    public TranslationEntry this[int index] => Entries[index];
    public bool Contains(string englishText, string targetLanguage)
    {
        return Entries.Any(e => e.SourceText == englishText && e.TargetLanguage == targetLanguage);
    }

    public TranslationEntry Find(string englishText, string targetLanguage)
    {
        if (Entries == null || string.IsNullOrEmpty(englishText) || string.IsNullOrEmpty(targetLanguage))
        {
            return TranslationEntry.Empty;
        }    
        return Entries.FirstOrDefault(e => e.SourceText == englishText && e.TargetLanguage == targetLanguage) ?? TranslationEntry.Empty;
    }

    public void AddRange(IEnumerable<TranslationEntry> entries)
    {
        Entries.AddRange(entries);
    }

    public List<TranslationEntry> ToList()
    {
        return [.. Entries];//Return a shallow copy of the list using collection expression.
    }
    public string ToCsvString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Source:" + SourceLanguage + "],[Target:" + TargetLanguage + "]");

        foreach (var entry in Entries)
        {
            sb.AppendLine($"\"{entry.SourceText}\",\"{entry.TargetText}\"");
        }
        return sb.ToString();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var entry in Entries)
        {
            sb.AppendLine($"{entry.SourceText} => {entry.TargetText}");
        }
        return sb.ToString();
    }
}