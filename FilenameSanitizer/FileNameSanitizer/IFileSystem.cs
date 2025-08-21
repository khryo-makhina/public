namespace FilenameSanitizer;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    string[] GetFiles(string path);
    void MoveFile(string sourceFileName, string destFileName);
    string ReadAllText(string path);
}
