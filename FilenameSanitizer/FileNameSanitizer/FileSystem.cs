namespace FilenameSanitizer;

/// <summary>
///    A simple implementation of IFileSystem that interacts with the actual file system.
/// </summary>
public class FileSystem : IFileSystem
{
    /// <summary>
    ///   Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <summary>
    ///  Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public bool FileExists(string path) => File.Exists(path);

    /// <summary>
    ///   Gets the files in the specified directory.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns>An array of file names.</returns>
    public string[] GetFiles(string path) => Directory.GetFiles(path);

    /// <summary>
    ///   Moves a file from the source path to the destination path.
    /// </summary>
    /// <param name="sourceFileName">The path to the source file.</param>
    /// <param name="destFileName">The path to the destination file.</param>
    public void MoveFile(string sourceFileName, string destFileName) => File.Move(sourceFileName, destFileName);
 
    /// <summary>
    ///   Reads all text from the specified file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The contents of the file as a string.</returns>
    public string ReadAllText(string path) => File.ReadAllText(path);
}