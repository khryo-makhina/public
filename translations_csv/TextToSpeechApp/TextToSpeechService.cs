using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.Media.Playback;
using Windows.Media.Core;

namespace TextToSpeechApp.TextToSpeechService;

/// <summary>
/// Provides text-to-speech functionality using specified language voices.
/// </summary>
public class TextToSpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synth;
    private readonly MediaPlayer _player;

    /// <summary>
    /// Initializes a new instance of the TextToSpeechService class with the specified language.
    /// </summary>
    /// <param name="language">The language to use for speech synthesis (e.g., "en-US", "fi-FI")</param>
    public TextToSpeechService(string language = "en-US")
    {
        _synth = new SpeechSynthesizer();
        _player = new MediaPlayer();

        var defaultVoice = SpeechSynthesizer.DefaultVoice;
        // Select voice by language tag (e.g., "en-US", "fi-FI")
        var voice = SpeechSynthesizer.AllVoices
            .FirstOrDefault(v => v.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

        if (voice != null)
        {
            if (voice.Language == defaultVoice.Language && voice != defaultVoice)
            {
                voice = defaultVoice;
            }

            Console.WriteLine($"Using voice: {voice.DisplayName} for language: {language}");
            _synth.Voice = voice;
        }
        else
        {
            Console.WriteLine($"Available voices:");
            foreach (var v in SpeechSynthesizer.AllVoices)
            {
                Console.WriteLine($"- {v.DisplayName} ({v.Language})");
            }
            Console.WriteLine($"No voice found for language: {language}. Using default voice.");
        }
    }

    /// <summary>
    /// Speaks the given text asynchronously.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>A Task representing the asynchronous operation</returns>
    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var stream = await _synth.SynthesizeTextToStreamAsync(text);
        _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);

        // Use TaskCompletionSource to await media end.
        var tsc = new TaskCompletionSource<bool>();
        _player.MediaEnded += (o, e) => { tsc.TrySetResult(true); };
        _player.Play();

        // Wait for a speech synthesis to complete.
        await tsc.Task;
    }

    /// <summary>
    /// Disposes the TextToSpeechService resources.
    /// </summary>
    public void Dispose()
    {
        _player?.Dispose();
        _synth?.Dispose();
    }
}
