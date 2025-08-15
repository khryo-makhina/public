namespace FilenameSanitizer
{
    public interface ISanitizerSettingsLoader
    {
        ISanitizerSetting LoadFromFile(string sanitizerSettingsFile);
    }
}