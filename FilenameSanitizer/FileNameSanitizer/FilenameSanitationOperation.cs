namespace FilenameSanitizer;

public class FilenameSanitationOperation
{
    public FilenameSanitationOperationLog Log { get; } = new();
    public string WorkingFolder { get; }

    public FilenameSanitationOperation(string workingFolder)
    {
        WorkingFolder = workingFolder;
    }
}
