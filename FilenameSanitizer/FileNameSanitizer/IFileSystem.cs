namespace FilenameSanitizer;

/// <summary>
/// Represents a file system abstraction for file and directory operations.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Determines whether the specified path refers to an existing directory.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>true if path refers to an existing directory; otherwise, false</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>true if path refers to an existing file; otherwise, false</returns>
    bool FileExists(string path);

    /// <summary>
    /// Returns the names of files in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search</param>
    /// <returns>An array of file names in the directory</returns>
    string[] GetFiles(string path);

    /// <summary>
    /// Moves a specified file to a new location.
    /// </summary>
    /// <param name="sourceFileName">The name of the file to move</param>
    /// <param name="destFileName">The new path for the file</param>
    void MoveFile(string sourceFileName, string destFileName);

    /// <summary>
    /// Opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to read</param>
    /// <returns>A string containing all lines of the file</returns>
    string ReadAllText(string path);
}
