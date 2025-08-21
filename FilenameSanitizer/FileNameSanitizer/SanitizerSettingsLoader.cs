using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilenameSanitizer;

public class SanitizerSettingsLoader : ISanitizerSettingsLoader
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _baseDirectory;

    public SanitizerSettingsLoader(IFileSystem fileSystem, ILogger logger, string? baseDirectory = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    public ISanitizerSetting LoadFromFile(string sanitizerSettingsFile)
    {
        var filePath = Path.Combine(_baseDirectory, sanitizerSettingsFile);
        var settings = DeserializeSettings(filePath);
        return settings;
    }

    private ISanitizerSetting DeserializeSettings(string filePath)
    {
        var settings = new SanitizerSetting();

        if (!_fileSystem.FileExists(filePath))
        {
            return settings; // Return default settings if file does not exist
        }

        try
        {
            var jsonContent = _fileSystem.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var deserializedSettings = JsonSerializer.Deserialize<SanitizerSetting>(jsonContent, options);
            if(deserializedSettings == null || deserializedSettings.IsEmpty())
            {
                _logger.LogError($"Deserialization of settings from {filePath} returned null for JSON: " + jsonContent);
                return SanitizerSetting.EmptyInstance; // Return empty instance if deserialization fails
            }
            settings = deserializedSettings; // Use deserialized settings or default if deserialization fails
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading settings from {filePath}", ex);
        }

        return settings;
    }
}