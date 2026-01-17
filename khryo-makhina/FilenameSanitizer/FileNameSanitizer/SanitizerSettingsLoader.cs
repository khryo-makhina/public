using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilenameSanitizer;

/// <summary>
/// Default implementation for loading sanitizer settings from JSON files.
/// </summary>
public class SanitizerSettingsLoader : ISanitizerSettingsLoader
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _baseDirectory;

    /// <summary>
    /// Initializes a new instance of the SanitizerSettingsLoader class.
    /// </summary>
    /// <param name="fileSystem">The file system to use for file operations</param>
    /// <param name="logger">The logger to use for logging</param>
    /// <param name="baseDirectory">Optional base directory. Defaults to current AppDomain's base directory</param>
    /// <exception cref="ArgumentNullException">Thrown when fileSystem or logger is null</exception>
    public SanitizerSettingsLoader(IFileSystem fileSystem, ILogger logger, string? baseDirectory = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <inheritdoc />
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