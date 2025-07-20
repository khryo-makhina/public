namespace FilenameSanitizer;

public class FileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public bool FileExists(string path) => File.Exists(path);
    public string[] GetFiles(string path) => Directory.GetFiles(path);
    public void MoveFile(string sourceFileName, string destFileName) => 
        File.Move(sourceFileName, destFileName);
}
