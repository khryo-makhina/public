namespace FilenameSanitizer;

/// <summary>
/// Sanitizes filenames to ensure they are valid for use across different operating systems.
/// Handles removal of invalid characters and ensures OS compatibility.
/// </summary>
public class FilenameSanitizer
{
    // Maximum filename lengths for different systems
    private const int MaxWindowsPathLength = 260;
    private const int MaxPosixNameLength = 255;

    // Windows reserved names (CON, PRN, AUX, etc.)
    private static readonly HashSet<string> WindowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4",
        "LPT1", "LPT2", "LPT3", "LPT4"
    };

    private readonly string _folder;
    private readonly IFileSystem _fileSystem;
    private readonly ISanitizerSettingsLoader _sanitizerSettingsLoader;
    public OperationLogger Logger;


    /// <summary>
    /// Initializes a new instance of the FilenameSanitizer class.
    /// </summary>
    /// <param name="folder">Optional working folder. If not specified, uses current directory.</param>
    /// <param name="sanitizerSettingsLoader">Optional sanitizer settings loader. If not specified, uses default settings loader.</param>
    /// <param name="fileSystem">Optional file system implementation. If not specified, uses default file system.</param>
    public FilenameSanitizer(string? folder = null, ISanitizerSettingsLoader? sanitizerSettingsLoader = null, IFileSystem? fileSystem = null)
    {
        _folder = ResolveFolderPath(folder);
        Logger = new OperationLogger(folder ?? Directory.GetCurrentDirectory());
        _sanitizerSettingsLoader = sanitizerSettingsLoader ?? new SanitizerSettingsLoader();
        _fileSystem = fileSystem ?? new FileSystem();
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

    /// <summary>
    /// Renames files in the working folder to meet OS filename requirements.
    /// </summary>
    public void RenameFilesToMeetOsRequirements()
    {        
        var filePaths = GetFilePathsFromWorkingFolder();
        Logger.Info.Add("Starting to sanitize filenames in folder: " + _folder);
        if (filePaths.Any())
        {
            Logger.Info.Add($"Found {filePaths.Count()} files in folder: {_folder}");
            foreach (var filePath in filePaths)
            {
                Logger.Info.Add($"Processing file: {filePath}");
                RenameFileIfNeeded(filePath);
            }
        }
    }

    private IEnumerable<string> GetFilePathsFromWorkingFolder()
    {
        if (!_fileSystem.DirectoryExists(_folder))
        {
            Logger.Errors.Add($"Folder '{_folder}' does not exist.");
            return Enumerable.Empty<string>();
        }
        
        var allFilePaths = _fileSystem.GetFiles(_folder);
        var filePaths = new List<string>();
        foreach(var filePath in allFilePaths)
        { 
            if(FilenameSanitizer.IsReservedName(filePath))
            {
                Logger.Info.Add($"Skipping reserved file name: {filePath}");
                continue;
            }
            filePaths.Add(filePath);
        }
        return filePaths;
    }

    private static bool IsReservedName(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (fileName == Sanitizer.SanitizerReplacePatternsFile)
        {
            return true;
        }

        if (fileName == Sanitizer.SanitizerSettingsFile)
        {
            return true;
        }

        if (fileName.StartsWith(OperationLogger.LogFileNameTemplate))
        {
            return true;
        }

        return false;
    }

    private void RenameFileIfNeeded(string filePath)
    {
        var sanitizedFileName = GetSanitizedFilenameWithPathRemoved(filePath);

        if (!ShouldRenameFile(filePath, sanitizedFileName))
        {
            Logger.Info.Add($"Skipping file: {filePath}");
            return;
        }

        TryRenameFile(filePath, sanitizedFileName);
    }

    private void TryRenameFile(string originalFile, string sanitizedFileName)
    {
        var newFilePath = Path.Combine(_folder, sanitizedFileName);

        if (_fileSystem.FileExists(newFilePath))
        {
            Logger.Errors.Add($"File '{newFilePath}' already exists. Skipping rename for '{originalFile}'.");
            return;
        }

        try
        {
            _fileSystem.MoveFile(originalFile, newFilePath);
        }
        catch (Exception ex)
        {
            Logger.Errors.Add($"Error renaming file '{originalFile}' to '{newFilePath}': {ex.Message}");
        }
    }

    private string GetSanitizedFilenameWithPathRemoved(string filePath)
    {
        Logger.Info.Add($"Sanitizing filename: {filePath}");
        var filename = Path.GetFileName(filePath);

        var sanitizer = new Sanitizer(_sanitizerSettingsLoader);
        var sanitizedFilename = sanitizer.SanitizeFileName(filename);

        if (sanitizedFilename.Length > MaxPosixNameLength)
        {
            sanitizedFilename = sanitizedFilename[..MaxPosixNameLength];
        }

        Logger.Info.Add($"Sanitized filename: {sanitizedFilename}");
        return sanitizedFilename;
    }

    private bool ShouldRenameFile(string originalFile, string sanitizedFileName)
    {
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            Logger.Errors.Add($"File '{originalFile}' has an empty sanitized filename. Skipping.");
            return false;
        }

        // Compare just the filenames, not the full paths
        var originalFileName = Path.GetFileName(originalFile);
        if (sanitizedFileName == originalFileName)
        {
            Logger.Warnings.Add($"File '{originalFileName}' is already sanitized. Skipping.");
            return false;
        }

        return true;
    }      

    /// <summary>
    /// Renames files by removing specified text patterns from filenames.
    /// </summary>
    /// <param name="patterns">New-line separated list of text patterns to remove from filenames</param>
    public void RenameFilesRemovingPatterns(string patterns)
    {
        Logger.Info.Add($"Removing patterns from filenames in folder: {_folder}");
        var files = GetFilePathsFromWorkingFolder();
        if (files.Any())
        {
            var patternsList = patterns.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var file in files)
            {
                Logger.Info.Add($"Processing file: {file} with patters {patternsList}");
                RenameFileRemovingPatternsIfNeeded(file, patternsList, Logger);
            }
        }
    }

    private void RenameFileRemovingPatternsIfNeeded(string file, string[] patterns, OperationLogger logger)
    {        
        var fileName = Path.GetFileName(file);
        var sanitizedFileName = RemovePatternsFromFilename(fileName, patterns);

        if (!ShouldRenameFile(file, sanitizedFileName))
        {
            Logger.Info.Add($"Skipping file: {file}");
            return;
        }

        TryRenameFile(file, sanitizedFileName);
    }

    private string RemovePatternsFromFilename(string fileName, string[] patterns)
    {
        Logger.Info.Add($"Removing patterns from filename: {fileName}");
        string filenamePatternRemoved = patterns.Aggregate(seed: fileName, func: (currentSeed, pattern) =>
        {
            string filenameProcessed = currentSeed.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
            return filenameProcessed;
        });

        Logger.Info.Add($"Filename after removing patterns: {filenamePatternRemoved}"); 
        return filenamePatternRemoved;
    }
}


