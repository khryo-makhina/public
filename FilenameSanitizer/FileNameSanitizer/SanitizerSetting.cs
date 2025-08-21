using System.Text.Json.Serialization;

namespace FilenameSanitizer;

public class SanitizerSetting : ISanitizerSetting
{
    /// <summary>
    /// Default character used to replace invalid characters in filenames.
    /// Underscore ('_').
    /// </summary>
    public static char DefaultCharacter
    {
        get 
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return ' '; // Default for Windows
            }
            else
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    return '-'; // Default for POSIX systems
                }
            }
            return '_'; 
        }
    }

    /// <summary>
    /// By default, the default character setting for the sanitizer to replace invalid characters.
    /// </summary>
    [JsonPropertyName("ReplacementCharacter")]
    public char ReplacementCharacter { get; internal set; } = DefaultCharacter;
}
