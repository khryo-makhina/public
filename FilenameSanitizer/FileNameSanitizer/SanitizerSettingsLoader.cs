namespace FilenameSanitizer;

public class SanitizerSettingsLoader : ISanitizerSettingsLoader
{
    public ISanitizerSetting LoadFromFile(string sanitizerSettingsFile)
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sanitizerSettingsFile);
        var settings = DeserializeSettings(filePath);
        return settings;
    }

    private static ISanitizerSetting DeserializeSettings(string filePath)
    {
       var settings = new SanitizerSetting();

        if (!File.Exists(filePath))
        {
            return settings; // Return default settings if file does not exist
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            var deserializedSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<SanitizerSetting>(jsonContent);
            settings = deserializedSettings ?? settings; // Use deserialized settings or default if deserialization fails             
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from {filePath}: {ex.Message}");
        }

        return settings;
    }    
}