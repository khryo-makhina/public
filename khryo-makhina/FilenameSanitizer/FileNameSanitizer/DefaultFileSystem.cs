using System;
using System.IO;

namespace FilenameSanitizer;

/// <summary>
/// Default implementation of IFileSystem that uses the actual file system.
/// </summary>
public class DefaultFileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public string[] GetFiles(string path) => Directory.GetFiles(path);

    /// <inheritdoc />
    public void MoveFile(string sourceFileName, string destFileName) => File.Move(sourceFileName, destFileName);

    /// <inheritdoc />
    public string ReadAllText(string path) => File.ReadAllText(path);
}
