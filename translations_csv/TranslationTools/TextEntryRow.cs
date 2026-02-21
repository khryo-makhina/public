namespace TranslationTools;

public class TextEntryRow: List<TextEntry>
{
    public string GetAllAsString()
    {
        return Environment.NewLine + string.Join($"{Environment.NewLine}", this.Select(e => $"  {e.Language.LanguageCulture.TwoLetterISOLanguageName}: {e.Text}"));
    }
}