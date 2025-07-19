namespace FilenameSanitizer;

/// <summary>
/// Sanitizes filenames to ensure they are valid for use across different operating systems.
/// Handles removal of invalid characters and ensures OS compatibility.
/// </summary>
public partial class FilenameSanitizer
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
    private readonly string _folder;

    /// <summary>
    /// Initializes a new instance of the FilenameSanitizer class.
    /// </summary>
    /// <param name="folder">Optional working folder. If not specified, uses current directory.</param>
    public FilenameSanitizer(string? folder = null)
    {
        _folder = ResolveFolderPath(folder);
    }

    /// <summary>
    /// Resolves the working folder path.
    /// </summary>
    /// <param name="folder">Input folder path, can be null, relative or absolute.</param>
    /// <returns>Absolute path to the working folder.</returns>
    private static string ResolveFolderPath(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return Environment.CurrentDirectory;
        }

        return Path.GetFullPath(folder);
    }

    public FilenameSanitationOperationLog OperationLog { get; private set; } = new();

    /// <summary>
    /// Renames files in the working folder to meet OS filename requirements.
    /// </summary>
    public FilenameSanitationOperation RenameFilesToMeetOsRequirements()
    {
        var operation = new FilenameSanitationOperation(_folder);
        
        var files = GetFilesFromWorkingFolder(operation.Log);
        if (files.Any())
        {
            foreach (var file in files)
            {
                RenameFileIfNeeded(file, operation.Log);
            }
        }

        return operation;
    }

    private IEnumerable<string> GetFilesFromWorkingFolder(FilenameSanitationOperationLog log)
    {
        if (!Directory.Exists(_folder))
        {
            log.Errors.Add($"Folder '{_folder}' does not exist.");
            return Enumerable.Empty<string>();
        }

        return Directory.GetFiles(_folder);
    }

    private void RenameFileIfNeeded(string file, FilenameSanitationOperationLog log)
    {
        var sanitizedFileName = GetSanitizedFilenameWithPathRemoved(file);
        
        if (!ShouldRenameFile(file, sanitizedFileName, log))
            return;

        TryRenameFile(file, sanitizedFileName, log);
    }

    private bool ShouldRenameFile(string originalFile, string sanitizedFileName, FilenameSanitationOperationLog log)
    {
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            log.Errors.Add($"File '{originalFile}' has an empty sanitized filename. Skipping.");
            return false;
        }

        if (sanitizedFileName == originalFile)
        {
            log.Warnings.Add($"File '{originalFile}' is already sanitized. Skipping.");
            return false;
        }

        return true;
    }

    private void TryRenameFile(string originalFile, string sanitizedFileName, FilenameSanitationOperationLog log)
    {
        var newFilePath = Path.Combine(_folder, sanitizedFileName);

        if (File.Exists(newFilePath))
        {
            log.Errors.Add($"File '{newFilePath}' already exists. Skipping rename for '{originalFile}'.");
            return;
        }

        try
        {
            File.Move(originalFile, newFilePath);
        }
        catch (Exception ex)
        {
            log.Errors.Add($"Error renaming file '{originalFile}' to '{newFilePath}': {ex.Message}");
        }
    }

    private static string GetSanitizedFilenameWithPathRemoved(string file)
    {
        var sanitizedFilename = Sanitizer.SanitizeFileName(Path.GetFileName(file));
        
        if (sanitizedFilename.Length > MaxPosixNameLength)
        {
            sanitizedFilename = sanitizedFilename[..MaxPosixNameLength];
        }
        return sanitizedFilename;
    }

    /// <summary>
    /// Renames files by removing specified text patterns from filenames.
    /// </summary>
    /// <param name="patterns">New-line separated list of text patterns to remove from filenames</param>
    public FilenameSanitationOperation RenameFilesRemovingPatterns(string patterns)
    {
        var operation = new FilenameSanitationOperation(_folder);
        
        var files = GetFilesFromWorkingFolder(operation.Log);
        if (files.Any())
        {
            var patternsList = patterns.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var file in files)
            {
                RenameFileRemovingPatternsIfNeeded(file, patternsList, operation.Log);
            }
        }

        return operation;
    }

    private void RenameFileRemovingPatternsIfNeeded(string file, string[] patterns, FilenameSanitationOperationLog log)
    {
        var fileName = Path.GetFileName(file);
        var sanitizedFileName = RemovePatternsFromFilename(fileName, patterns);
        
        if (!ShouldRenameFile(file, sanitizedFileName, log))
            return;

        TryRenameFile(file, sanitizedFileName, log);
    }

    private static string RemovePatternsFromFilename(string fileName, string[] patterns)
    {
        return patterns.Aggregate(fileName, (current, pattern) => 
            current.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase));
    }
}


