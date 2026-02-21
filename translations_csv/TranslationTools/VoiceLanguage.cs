using System.Globalization;

namespace TranslationTools;

public class VoiceLanguage
{
    public string LanguageName { get; }

    public LanguageTypes LanguageType { get; }

    public CultureInfo LanguageCulture { get; }

    private static VoiceLanguage? _emptyVoiceLanguage = null;
    public static VoiceLanguage Empty
    {
        get
        {
            if (_emptyVoiceLanguage != null)
            {
                return _emptyVoiceLanguage;
            }
            _emptyVoiceLanguage = new VoiceLanguage(string.Empty, LanguageTypes.None);
            return _emptyVoiceLanguage;
        }
    }

    private static VoiceLanguage? _systemVoiceLanguage = null;
    public static VoiceLanguage System
    {
        get
        {
            if (_systemVoiceLanguage != null)
            {
                return _systemVoiceLanguage;
            }
            var englishName = CultureInfo.CurrentCulture.EnglishName.Split(' ')[0];
            _systemVoiceLanguage = new VoiceLanguage(englishName, LanguageTypes.System);
            return _systemVoiceLanguage;
        }
    }

    public VoiceLanguage(string englishLanguageName, LanguageTypes languageType)
    {
        LanguageName = englishLanguageName;
        LanguageType = languageType;
        LanguageCulture = GetNeutralCulture(englishLanguageName);
    }

    private static CultureInfo GetNeutralCulture(string languageName)
    {
        var neutral = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
            .FirstOrDefault(c =>
                c.EnglishName.Equals(languageName, StringComparison.OrdinalIgnoreCase));

        if (neutral == null)
        {
            return CultureInfo.CurrentCulture; // Fallback to current culture if not found
        }

        var lang = CultureInfo.CreateSpecificCulture(neutral.Name);

        return lang;
    }   

    override public string ToString()
    {
        return LanguageName;
    }
}