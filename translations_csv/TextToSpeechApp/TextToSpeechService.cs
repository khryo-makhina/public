using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using TranslationTools;

namespace TextToSpeechApp;

/// <summary>
///     Provides text-to-speech functionality using specified language voices.
/// </summary>
/// <summary>
///     Provides text-to-speech services for multiple languages and manages
///     associated synthesizers and media players.
/// </summary>
internal class TextToSpeechService : IDisposable
{
    private readonly List<TextEntryRow> _rowEntries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextToSpeechService"/> class
    ///     using the supplied <see cref="VoiceLanguageList"/>.
    /// </summary>
    /// <param name="voiceLanguages">The collection of voice languages to initialize.</param>
    public TextToSpeechService(VoiceLanguageList voiceLanguages)
    {
        VoiceLanguages = voiceLanguages;

        Initialize(voiceLanguages);

        _rowEntries ??= [];
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextToSpeechService"/> class
    ///     using the supplied <see cref="VoiceLanguageList"/> and pre-populated rows.
    /// </summary>
    /// <param name="voiceLanguages">The collection of voice languages to initialize.</param>
    /// <param name="rowEntries">A list of <see cref="TextEntryRow"/> entries to use.</param>
    public TextToSpeechService(VoiceLanguageList voiceLanguages, List<TextEntryRow> rowEntries) : this(voiceLanguages)
    {
        _rowEntries = rowEntries;
    }

    /// <summary>
    ///     The configured voice languages and defaults used by this service.
    /// </summary>
    public VoiceLanguageList VoiceLanguages { get; }

    /// <summary>
    ///     Mapping of language name to its <see cref="SpeechSynthesizer"/> instance.
    /// </summary>
    public Dictionary<string, SpeechSynthesizer> Synths { get; } = new();

    /// <summary>
    ///     Mapping of language name to its <see cref="MediaPlayer"/> instance.
    /// </summary>
    public Dictionary<string, MediaPlayer> Players { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (KeyValuePair<string, SpeechSynthesizer> entry in Synths)
        {
            entry.Value.Dispose();
        }

        foreach (KeyValuePair<string, MediaPlayer> entry in Players)
        {
            entry.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }


    /// <summary>
    ///     Initializes synthesizers and players for each language in the provided list.
    /// </summary>
    /// <param name="languageList">The list of languages to initialize.</param>
    public void Initialize(VoiceLanguageList languageList)
    {
        foreach (VoiceLanguage language in languageList)
        {
            InitializeVoiceLanguage(language);
        }

        if (Synths.Count == 0 || Players.Count == 0)
        {
            InitializeVoiceLanguage(VoiceLanguage.System);
        }
    }

    /// <summary>
    ///     Initializes a new instance of the TextToSpeechService class with the specified language.
    /// </summary>
    /// <param name="language">The language to use for speech synthesis (e.g., "en-US", "fi-FI")</param>
    /// <summary>
    ///     Creates and configures the speech synthesizer and media player for a single language.
    /// </summary>
    /// <param name="language">The language to initialize.</param>
    private void InitializeVoiceLanguage(VoiceLanguage language)
    {
        var synth = new SpeechSynthesizer();
        Synths[language.LanguageName] = synth;
        var player = new MediaPlayer();

        Players[language.LanguageName] = player;

        VoiceInformation? defaultVoice = SpeechSynthesizer.DefaultVoice;

        // Select voice by language tag (e.g., "en-US", "fi-FI")
        var twoLetterIsoLanguageName = language.LanguageCulture.TwoLetterISOLanguageName;
        VoiceInformation? voice = SpeechSynthesizer.AllVoices
            .FirstOrDefault(v =>
                v.Language[..2].Equals(twoLetterIsoLanguageName, StringComparison.OrdinalIgnoreCase));

        if (voice != null)
        {
            if (voice.Language[..2] == defaultVoice.Language[..2] && voice != defaultVoice)
            {
                voice = defaultVoice;
            }

            Console.WriteLine(
                $"Using voice: {voice.DisplayName} for language: {language.LanguageName[..2]}");
            synth.Voice = voice;
        }
        else
        {
            Console.WriteLine("Available voices:");
            foreach (VoiceInformation? v in SpeechSynthesizer.AllVoices)
            {
                Console.WriteLine($"- {v.DisplayName} ({v.Language[..2]})");
            }

            Console.WriteLine(
                $"No voice found for language: {language.LanguageName[..2]}. Using default voice.");
        }
    }

    /// <summary>
    ///     Synthesizes and plays the supplied <paramref name="text"/> asynchronously
    ///     using the specified <paramref name="voiceLanguage"/>, or the default voice.
    /// </summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="voiceLanguage">Optional language override to use for this utterance.</param>
    /// <returns>A task that completes when playback finishes.</returns>
    public async Task SpeakTextAsync(string text, VoiceLanguage? voiceLanguage = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        VoiceLanguage useVoice = voiceLanguage ?? VoiceLanguages.DefaultVoice;

        SpeechSynthesisStream? stream = await Synths[useVoice.LanguageName].SynthesizeTextToStreamAsync(text);
        Players[useVoice.LanguageName].Source = MediaSource.CreateFromStream(stream, stream.ContentType);

        // Use TaskCompletionSource to await media end.
        var tsc = new TaskCompletionSource<bool>();
        Players[useVoice.LanguageName].MediaEnded += (_, _) => { tsc.TrySetResult(true); };
        Players[useVoice.LanguageName].Play();

        // Wait for a speech synthesis to complete.
        await tsc.Task;
    }

    /// <summary>
    ///     Speaks the text contained in the supplied <see cref="TextEntry"/>.
    /// </summary>
    /// <param name="entry">The text entry to speak.</param>
    /// <returns>A task that completes when playback finishes.</returns>
    internal async Task SpeakEntryAsync(TextEntry entry)
    {
        var text = entry.Text;

        await SpeakTextAsync(text, entry.Language);
    }
}