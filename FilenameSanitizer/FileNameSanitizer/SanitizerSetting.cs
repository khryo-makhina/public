using System.Text.Json.Serialization;

namespace FilenameSanitizer;

public class SanitizerSetting : ISanitizerSetting
{
    public static List<string> DefaultExcludedCharacters = new();

    public SanitizerSetting()
    {
        ReplacementCharacter = DefaultCharacter;
        ExcludedCharacters = DefaultExcludedCharacters;
    }

    /// <summary>
    /// Default character used to replace invalid characters in filenames.
    /// Underscore ('_').
    /// </summary>
    public static string DefaultCharacter
    {
        get 
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return " "; // Default for Windows
            }
            else
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    return "-"; // Default for POSIX systems
                }
            }
            return "_"; 
        }
    }

    /// <summary>
    /// By default, the default character setting for the sanitizer to replace invalid characters.
    /// </summary>
    [JsonPropertyName("ReplacementCharacter")]
    public string ReplacementCharacter { get; set; } = string.Empty;
   
    /// <summary>
    /// Gets the array of characters that are excluded from processing.
    /// </summary>
    [JsonPropertyName("ExcludedCharacters")]
    public List<string> ExcludedCharacters { get; set; } = new();

    /// <summary>
    /// Checks if the provided setting is empty, meaning it has the default replacement character and no excluded characters.
    /// </summary>
    /// <param name="setting"><see cref="SanitizerSetting"/></param>
    /// <returns></returns>
    public bool IsEmpty() => 
        string.IsNullOrEmpty(ReplacementCharacter) && 
        (ExcludedCharacters == null || ExcludedCharacters.Count == 0);

    public static ISanitizerSetting EmptyInstance => new SanitizerSetting();
}
