using System.Globalization;

namespace TranslationTools;

public class TranslationEntryList
{  
    public VoiceLanguageList VoiceLanguages { get; set; } = [];

    public int Count => Entries.Count;

    public List<TextEntryRow> Entries { get; set; } = new List<TextEntryRow>();
    public bool IsSingleLanguage
    {
        get
        {
            var isSourceOnly = VoiceLanguages.Count == 1;
            return isSourceOnly;
        }
    }

    public bool IsTranslationEntryTranslations { get; internal set; }

    public void Add(TextEntryRow entryRow) => Entries.Add(entryRow);
    public TextEntryRow this[int index] => Entries[index];   
}
