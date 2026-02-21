using System.Globalization;

namespace TranslationTools;

public class VoiceLanguageList: List<VoiceLanguage>
{

    public VoiceLanguageList()
    {
    }

    private VoiceLanguage? _voiceLanguage = null;
    public VoiceLanguage DefaultVoice
    {
        get
        {
            if(_voiceLanguage != null)
            {
                return _voiceLanguage;
            }
            
            _voiceLanguage = VoiceLanguage.System;
            return _voiceLanguage;
        }
    }

    public override string ToString()
    {
        var listString = string.Join(", ", this.Select(v => v.LanguageName.ToString()));
        return listString;
    }
}
