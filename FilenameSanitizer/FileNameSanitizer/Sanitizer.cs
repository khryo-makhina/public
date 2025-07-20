using System.Text.RegularExpressions;

namespace FilenameSanitizer;

public static class Sanitizer
{
    // Maximum filename lengths for different systems
    private const int MaxWindowsPathLength = 260;
    private const int MaxPosixNameLength = 255;
    
    // Windows reserved names (CON, PRN, AUX, etc.)
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4",
        "LPT1", "LPT2", "LPT3", "LPT4"
    };

    /// <summary>
    /// This routine ensure that file name, which contains folder separators, e.g. an AWS S3 BLOB resource key. 
    /// The resulting filename will be sanitized to meet OS requirements.
    /// </summary>
    /// <param name="fileName">The file to have a check on.</param>
    /// <returns>Sanitized filename, without folder separators</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetSanitizedFilenameWithPathRemoved(string fileName)
    {
        var sanitizedFilename = SanitizeFileName(Path.GetFileName(fileName));
        return sanitizedFilename;
    }

    public static string SanitizeFileName(string fileName, string optionalFileNameIfEmpty = "")
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return optionalFileNameIfEmpty;

        // Define the invalid characters, including POSIX unsafe chars
        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat(new[]
            {
                '!', '&', '(', ')', '{', '}', '[', ']', '<', '>', '|',
                '?', '=', '`', '\'', '¨', '~', '^', '*', '@', '£', '€', '$', ';', '-',
                '\0', '/', '\\', // Additional POSIX unsafe characters
            })
            .ToArray();

        // Replace invalid characters with underscore
        fileName = invalidChars.Aggregate(fileName, (current, c) => current.Replace(c, '_'));

        // Clean up multiple consecutive underscores
        fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "__+", "_");

        // Remove leading and trailing whitespace
        fileName = fileName.Trim();

        // Remove spaces before and after the file extension dot
        fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"\s+\.", ".");
        fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"\.\s+", ".");

        // Remove control characters
        fileName = Regex.Replace(fileName, @"\p{C}+", string.Empty);

        // Handle spaces and dots
        fileName = fileName.Trim(); // Remove leading/trailing spaces
        fileName = Regex.Replace(fileName, @"\s+", "_"); // Replace multiple spaces with single underscore
        fileName = Regex.Replace(fileName, @"\.+", "."); // Replace multiple dots with single dot
        fileName = fileName.TrimEnd('.'); // Remove trailing dots (invalid in Windows)

        // Check for Windows reserved names
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        if (ReservedNames.Contains(fileNameWithoutExt))
        {
            fileName = "_" + fileName;
        }

        // Ensure the filename isn't too long
        if (fileName.Length > MaxPosixNameLength)
        {
            var extension = Path.GetExtension(fileName);
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = baseFileName.Substring(0, MaxPosixNameLength - extension.Length) + extension;
        }

        return fileName;
    }
}
