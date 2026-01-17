using System.Text.RegularExpressions;

namespace FilenameSanitizer;

/// <inheritdoc />
public class Sanitizer : ISanitizer
{
    private readonly ISanitizerSettingsLoader _sanitizerSettingsLoader;

    /// <summary>
    /// Initializes a new instance of the Sanitizer class.
    /// </summary>
    /// <param name="sanitizerSettings">The settings loader to use for sanitization configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when sanitizerSettings is null.</exception>
    public Sanitizer(ISanitizerSettingsLoader sanitizerSettings)
    {
        _sanitizerSettingsLoader = sanitizerSettings ?? throw new ArgumentNullException(nameof(sanitizerSettings));
    }

    /// <inheritdoc />
    public bool IsFilenameSanitized(string? fileName)
    {
        // Empty or null filenames are considered sanitized as they'll be handled by optionalFileNameIfEmpty
        if (string.IsNullOrWhiteSpace(fileName))
            return true;

        var sanitized = SanitizeFileName(fileName);
        return sanitized == fileName;
    }

    /// <inheritdoc />
    public string SanitizeFileName(string? filename, string optionalFileNameIfEmpty = "")
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return optionalFileNameIfEmpty;
        }

        var settings = _sanitizerSettingsLoader.LoadFromFile(SanitizerConstants.SanitizerSettingsFile);

        // Remove leading and trailing whitespace
        filename = filename.Trim();

        filename = SanitizeFilenameFromInvalidCharacters(filename, settings);

        // Clean up multiple consecutive underscores
        filename = Regex.Replace(filename, "__+", settings.ReplacementCharacter.ToString());

        // Handle spaces and dots
        filename = filename.Trim(); // Remove leading/trailing spaces
        filename = Regex.Replace(filename, @"\s+", settings.ReplacementCharacter.ToString()); // Replace multiple spaces with single underscore
        filename = Regex.Replace(filename, @"\.+", "."); // Replace multiple dots with single dot
        filename = filename.TrimEnd('.'); // Remove trailing dots (invalid in Windows)

        // Remove control characters
        filename = Regex.Replace(filename, @"\p{C}+", string.Empty);

        filename = FilterOutNonPrintableCharacters(filename);
        filename = SanitizeWithSettings(filename);

        filename = TidyFileExtensionFromSpaces(filename);
        filename = TidyFileExtensionFromCharacter('_', filename);

        // Check for Windows reserved names
        filename = IncludeUnderscoreForWindowsReservedNames(filename);

        // Ensure the filename isn't too long
        filename = CutOffTooLong(filename);

        return filename;
    }

    /// <summary>
    /// Removes spaces before and after file extension dots.
    /// </summary>
    /// <param name="filename">The filename to process</param>
    /// <returns>The filename with cleaned up spaces around extension dots</returns>
    private static string TidyFileExtensionFromSpaces(string filename)
    {
        bool hasOccurenceBeforeDot = Regex.IsMatch(filename, @"\s+\.");
        while (hasOccurenceBeforeDot)
        {
            // Remove spaces before the file extension dot
            filename = Regex.Replace(filename, @"\s+\.", ".");
            hasOccurenceBeforeDot = Regex.IsMatch(filename, @"\s+\.");
        }
        bool hasOccurenceAfterDot = Regex.IsMatch(filename, @"\.\s+");
        while (hasOccurenceAfterDot) {
            // Remove spaces after the file extension dot
            filename = Regex.Replace(filename, @"\.\s+", ".");
            hasOccurenceAfterDot = Regex.IsMatch(filename, @"\.\s+");
        }          

        return filename;
    }

    /// <summary>
    /// Removes specified characters before and after file extension dots.
    /// </summary>
    /// <param name="character">The character to remove</param>
    /// <param name="filename">The filename to process</param>
    /// <returns>The filename with cleaned up characters around extension dots</returns>
    private static string TidyFileExtensionFromCharacter(char character, string filename)
    {
        bool hasOccurenceBeforeDot = Regex.IsMatch(filename, character + @"+\.");
        while (hasOccurenceBeforeDot)
        {
            // Remove underscores before the file extension dot
            filename = Regex.Replace(filename, character + @"+\.", ".");
            hasOccurenceBeforeDot = Regex.IsMatch(filename, character + @"+\.");
        }

        bool hasOccurenceAfterDot = Regex.IsMatch(filename, @"\." + character + "+");
        while (hasOccurenceAfterDot)
        {
            // Remove underscores after the file extension dot
            filename = Regex.Replace(filename, @"\." + character + "+", ".");
            hasOccurenceAfterDot = Regex.IsMatch(filename, @"\." + character + "+");
        }

        return filename;
    }

    /// <summary>
    /// Adds an underscore prefix to Windows reserved filenames when running on Windows.
    /// </summary>
    /// <param name="filename">The filename to check</param>
    /// <returns>The filename with an underscore prefix if needed</returns>
    private static string IncludeUnderscoreForWindowsReservedNames(string filename)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        { 
            return filename; // Only apply this check for Windows
        }

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filename);
        if (SanitizerConstants.ReservedNames.Contains(fileNameWithoutExt))
        {
            filename = "_" + filename;
        }

        return filename;
    }

    /// <summary>
    /// Cuts off the filename if it exceeds the maximum length for POSIX systems.
    /// </summary>
    /// <remarks>File extensions are preserved, but the base filename is truncated if necessary.</remarks>
    /// <param name="filename"></param>
    /// <returns>The original file name if not too long, or the cut off filename.</returns>
    private static string CutOffTooLong(string filename)
    {
        if (filename.Length > SanitizerConstants.MaxPosixNameLength)
        {
            var extension = Path.GetExtension(filename);
            var baseFileName = Path.GetFileNameWithoutExtension(filename);
            filename = baseFileName.Substring(0, SanitizerConstants.MaxPosixNameLength - extension.Length) + extension;
        }

        return filename;
    }

    /// <summary>
    /// Replaces invalid filename characters with the configured replacement character.
    /// </summary>
    /// <param name="filename">The filename to sanitize</param>
    /// <param name="settings">The sanitizer settings to use</param>
    /// <returns>The sanitized filename</returns>
    private static string SanitizeFilenameFromInvalidCharacters(string filename, ISanitizerSetting settings)
    {
        // Define the invalid characters, including POSIX unsafe chars
        char[] invalidChars = Path.GetInvalidFileNameChars()
            .Concat(new[]
            {
                '!', '&', '(', ')', '{', '}', '[', ']', '<', '>', '|',
                '?', '=', '`', '\'', '¨', '~', '^', '*', '@', '£', '€', '$', ';', '-',
                '\0', '/', '\\', // Additional POSIX unsafe characters
            })
            .Where(c => c.ToString() != settings.ReplacementCharacter && !settings.ExcludedCharacters.Contains(c.ToString())) // Remove the replacement character from the list
            .ToArray();

        // Replace invalid characters with settings.ReplacementCharacter
        filename = invalidChars.Aggregate(filename, (current, c) => current.Replace(c.ToString(), settings.ReplacementCharacter));
        filename = CutOffTooLong(filename);
        return filename;
    }

    /// <summary>
    /// Sanitizes the filename using a default settings file.
    /// The file should contain replacement patterns in the format "pattern;replacement".
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string SanitizeWithSettings(string fileName)
    {
        var replacePatternsSettingsFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SanitizerConstants.SanitizerReplacePatternsFile);
        if (!File.Exists(replacePatternsSettingsFilename))
        {
            return fileName; // Return original filename if settings file does not exist
        }        

        // Check if the file is writable and readable
        // Read replacement patterns from the file
        List<string> allLines = File.ReadAllLines(replacePatternsSettingsFilename)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) // Ignore empty lines and comments
            .Select(line => line.Trim())
            .ToList();

        if (!allLines.Any())
        {
            return fileName; // Return original filename if no valid patterns are found
        }

        foreach (var line in allLines)
        {
            var toBeReplaced = line.Trim();
            var settings = _sanitizerSettingsLoader.LoadFromFile(SanitizerConstants.SanitizerSettingsFile);
            fileName = fileName.Replace(toBeReplaced, settings.ReplacementCharacter.ToString()); // Replace the pattern with the replacement character
        }
        return fileName;
    }

    private static string FilterOutNonPrintableCharacters(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty; // Return an empty string for null or empty input
        }

        // Filter out non-printable characters by removing any character that has a value less than space
        return new string(fileName
            .Where(ch => ch >= SanitizerConstants.FirstPrintableAsciiChar)
            .ToArray());
    }
}
