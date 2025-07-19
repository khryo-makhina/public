namespace FileNameSanitizer;

/// <summary>
/// Represents a file entry with its original path and sanitized final path.
/// The `FinalPath` and `FinalFileName` properties are used to construct the final file path.
/// The `FinalFilePath` property returns the complete path if the file does not already exist
/// </summary>
public record FileEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileEntry"/> class with the specified original file path.
    /// </summary>
    /// <param name="originalFilePath">The original file path.</param>
    public FileEntry(string originalFilePath)
    {
        OriginalFilePath = originalFilePath;
    }

    /// <summary>
    /// The original file path before any sanitization or modifications.
    /// This property is required and must be set when creating an instance of FileEntry.
    /// </summary>
    public required string OriginalFilePath;

    /// <summary>
    /// The sanitized final path where the file will be stored.
    /// </summary>
    public string FinalPath = string.Empty;
    /// <summary>
    /// The sanitized final file name after processing.
    /// </summary>
    public string FinalFileName = string.Empty;
    private string _filePath = string.Empty;

    /// <summary>
    /// Gets the final file path by combining the FinalPath and FinalFileName.
    /// </summary>
    public string FinalFilePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                return _filePath;
            }
            _filePath = Path.Combine(FinalPath, FinalFileName);            
            return _filePath;
        }
    }
}