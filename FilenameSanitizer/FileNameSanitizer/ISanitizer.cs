namespace FilenameSanitizer
{
    public interface ISanitizer
    {
        string GetSanitizedFilenameWithPathRemoved(string fileName);
        string SanitizeFileName(string fileName, string optionalFileNameIfEmpty = "");
    }
}