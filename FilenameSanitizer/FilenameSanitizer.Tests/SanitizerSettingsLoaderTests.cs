using System;
using NSubstitute;
using Shouldly;
using Xunit;

namespace FilenameSanitizer.Tests;

public class SanitizerSettingsLoaderTests
{
    [Fact]
    public void LoadFromFile_ReturnsDefaultSettings_WhenFileDoesNotExist()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger>();
        fileSystem.FileExists(Arg.Any<string>()).Returns(false);
        var loader = new SanitizerSettingsLoader(fileSystem, logger);

        var result = loader.LoadFromFile("nonexistent.json");

        result.ShouldNotBeNull();
        result.ReplacementCharacter.ShouldBe(SanitizerSetting.DefaultCharacter);
        result.ExcludedCharacters.ShouldBeEmpty();
    }

    [Fact]
    public void LoadFromFile_ReturnsDeserializedSettings_WhenFileExistsAndIsValid()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger>();
        fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        string json = """{"ReplacementCharacter": "#", "ExcludedCharacters": ["-", "@"]}""";
        fileSystem.ReadAllText(Arg.Any<string>()).Returns(json);
        var loader = new SanitizerSettingsLoader(fileSystem, logger);

        var result = loader.LoadFromFile("test_file_name_does_not_matter_what_is.json");

        result.ShouldNotBeNull();
        result.ReplacementCharacter.ShouldBe("#");
        result.ExcludedCharacters.ShouldBe(new List<string>() { "-", "@" });
    }

    [Fact]
    public void LoadFromFile_LogsErrorAndReturnsDefault_WhenDeserializationFails()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger>();
        fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        fileSystem.ReadAllText(Arg.Any<string>()).Returns("not valid json");
        var loader = new SanitizerSettingsLoader(fileSystem, logger);

        var result = loader.LoadFromFile("settings.json");

        result.ShouldNotBeNull();
        result.ReplacementCharacter.ShouldBe(SanitizerSetting.DefaultCharacter);
        result.ExcludedCharacters.ShouldBeEmpty();
        logger.Received().LogError(Arg.Any<string>(), Arg.Any<Exception>());
    }

    [Fact]
    public void LoadFromFile_UsesProvidedBaseDirectory()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger>();
        fileSystem.FileExists(Arg.Any<string>()).Returns(false);
        string customBaseDir = "/custom/base/dir";
        var loader = new SanitizerSettingsLoader(fileSystem, logger, customBaseDir);

        loader.LoadFromFile("settings.json");

        fileSystem.Received().FileExists(System.IO.Path.Combine(customBaseDir, "settings.json"));
    }
}
