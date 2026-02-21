using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TranslationTools;
using Windows.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace TextToSpeechApp;

/// <summary>
/// Provides text-to-speech functionality using specified language voices.
/// </summary>
public class TextToSpeechService : IDisposable
{
    private readonly List<TextEntryRow> _rowEntries;

    public VoiceLanguageList VoiceLanguages { get; }
    public Dictionary<string, SpeechSynthesizer> Synths { get; private set; } = new Dictionary<string, SpeechSynthesizer>();
    public Dictionary<string, MediaPlayer> Players { get; private set; }  = new Dictionary<string, MediaPlayer>();

   
    public void Initialize(VoiceLanguageList languagelist)
    {
        foreach (var language in languagelist)
        {
            InitiliazieVoiceLanguage(language);
        }

        if(Synths.Count == 0 || Players.Count == 0)
        {
            InitiliazieVoiceLanguage(VoiceLanguage.System);
        }
    }

    /// <summary>
    /// Initializes a new instance of the TextToSpeechService class with the specified language.
    /// </summary>
    /// <param name="language">The language to use for speech synthesis (e.g., "en-US", "fi-FI")</param>
    private void InitiliazieVoiceLanguage(VoiceLanguage language)
    {
        var synth = new SpeechSynthesizer();
        Synths[language.LanguageName] = synth;
        var player = new MediaPlayer();

        Players[language.LanguageName] = player;

        var defaultVoice = SpeechSynthesizer.DefaultVoice;

        // Select voice by language tag (e.g., "en-US", "fi-FI")
        var twoLetterISOLanguageName = language.LanguageCulture.TwoLetterISOLanguageName;
        VoiceInformation? voice = SpeechSynthesizer.AllVoices
            .FirstOrDefault(v => v.Language.Substring(0, 2).Equals(twoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));

        if (voice != null)
        {
            if (voice.Language.Substring(0, 2) == defaultVoice.Language.Substring(0, 2) && voice != defaultVoice)
            {
                voice = defaultVoice;
            }

            Console.WriteLine($"Using voice: {voice.DisplayName} for language: {language.LanguageName.Substring(0, 2)}");
            synth.Voice = voice;
        }
        else
        {
            Console.WriteLine($"Available voices:");
            foreach (var v in SpeechSynthesizer.AllVoices)
            {
                Console.WriteLine($"- {v.DisplayName} ({v.Language.Substring(0, 2)})");
            }
            Console.WriteLine($"No voice found for language: {language.LanguageName.Substring(0, 2)}. Using default voice.");
        }
    }

    public TextToSpeechService(VoiceLanguageList voiceLanguages)
    {
        VoiceLanguages = voiceLanguages;

        Initialize(voiceLanguages);        

        _rowEntries ??= [];
    }

    public TextToSpeechService(VoiceLanguageList voiceLanguages, List<TextEntryRow> rowEntries) : this(voiceLanguages)
    {
        _rowEntries = rowEntries;
    }

    /// <summary>
    /// Speaks the given text asynchronously.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>A Task representing the asynchronous operation</returns>
    public async Task SpeakTextAsync(string text, VoiceLanguage? voiceLanguagex = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var useVoice = voiceLanguagex ?? VoiceLanguages.DefaultVoice;

        var stream = await Synths[useVoice.LanguageName].SynthesizeTextToStreamAsync(text);
        Players[useVoice.LanguageName].Source = MediaSource.CreateFromStream(stream, stream.ContentType);

        // Use TaskCompletionSource to await media end.
        var tsc = new TaskCompletionSource<bool>();
        Players[useVoice.LanguageName].MediaEnded += (o, e) => { tsc.TrySetResult(true); };
        Players[useVoice.LanguageName].Play();

        // Wait for a speech synthesis to complete.
        await tsc.Task;
    }

    internal async Task SpeakEntryAsync(TextEntry entry)
    {
        var text = entry.Text;

        await SpeakTextAsync(text, entry.Language);
    }

    /// <summary>
    /// Disposes the TextToSpeechService resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var entry in Synths)
        {
            entry.Value?.Dispose();
        }
        foreach (var entry in Players)
        {
            entry.Value?.Dispose();
        }
        GC.SuppressFinalize(this);
    }   
}
