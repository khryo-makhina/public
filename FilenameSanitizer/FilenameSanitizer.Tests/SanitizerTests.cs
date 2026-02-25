using NSubstitute;
using Shouldly;
using Xunit;
using System.Collections.Generic;

namespace FilenameSanitizer.Tests;

public class SanitizerTests
{
    public SanitizerTests()
    {
        SetUpSut();
    }

    private Sanitizer SetUpSut(string replacementCharacter = "_", List<string>? excludedChars = null)
    {
        // Initialize any required settings or dependencies here
        var sanitizerSettingsLoader = Substitute.For<ISanitizerSettingsLoader>();
        var sanitizerSetting = Substitute.For<ISanitizerSetting>();
        sanitizerSetting.ReplacementCharacter.Returns(replacementCharacter);
        sanitizerSetting.ExcludedCharacters.Returns(excludedChars ?? new List<string>());
        sanitizerSettingsLoader.LoadFromFile(Arg.Any<string>()).Returns(sanitizerSetting);

        return new Sanitizer(sanitizerSettingsLoader);
    }

    [Theory]
    [InlineData("test:file.txt", "test file.txt")]
    [InlineData("test/file.txt", "test file.txt")]
    [InlineData("test\\file.txt", "test file.txt")]
    [InlineData("test<>file.txt", "test file.txt")]
    [InlineData("COM1 .txt", "_COM1.txt")]
    [InlineData("PRN .doc", "_PRN.doc")]
    public void SanitizeFileName_SpaceIsReplacementCharacter_SanitizedFilenameContainsSpace(string input,
        string expected)
    {
        // Given: sanitizer configured with replacement character ' ' (space)
        Sanitizer sut = SetUpSut(" ");

        // When: sanitizing the input filename
        var actual = sut.SanitizeFileName(input);

        // Then: the sanitized filename should match the expected value
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
    [InlineData("test/file.txt", "test_file.txt")]
    [InlineData("test\\file.txt", "test_file.txt")]
    [InlineData("test<>file.txt", "test_file.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("PRN.doc", "_PRN.doc")]
    public void SanitizeFileName_UnderscoreIsReplacementCharacterAsDefault_SanitizedFilenameContainsUnderscore(
        string input, string expected)
    {
        // Given: sanitizer using default replacement character '_'
        Sanitizer sut = SetUpSut();

        // When: sanitizing the input filename
        var actual = sut.SanitizeFileName(input);

        // Then: the sanitized filename should match the expected value
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("test_file.txt", true)]
    [InlineData("test:file.txt", false)]
    [InlineData("test<>file.txt", false)]
    [InlineData("COM1.txt", false)]
    [InlineData("test file.txt", false)]
    public void IsFilenameSanitized_ShouldReturnCorrectResult(string filename, bool expected)
    {
        // Given: sanitizer using default settings
        Sanitizer sut = SetUpSut();

        // When: checking whether the filename is already sanitized
        var actual = sut.IsFilenameSanitized(filename);

        // Then: the result should match the expected boolean
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("test?file.txt")]
    [InlineData("test=file.txt")]
    [InlineData("test`file.txt")]
    [InlineData("test'file.txt")]
    [InlineData("test¨file.txt")]
    [InlineData("test~file.txt")]
    [InlineData("test^file.txt")]
    [InlineData("test*file.txt")]
    [InlineData("test@file.txt")]
    [InlineData("test£file.txt")]
    [InlineData("test€file.txt")]
    [InlineData("test$file.txt")]
    [InlineData("test;file.txt")]
    [InlineData("test-file.txt")]
    [InlineData("test&file.txt")]
    [InlineData("test!file.txt")]
    [InlineData("test[file.txt")]
    [InlineData("test]file.txt")]
    [InlineData("test{file.txt")]
    [InlineData("test}file.txt")]
    [InlineData("test(file.txt")]
    [InlineData("test)file.txt")]
    [InlineData("test<file.txt")]
    [InlineData("test>file.txt")]
    [InlineData("test|file.txt")]
    public void SanitizeFileName_WhenContainsSpecialCharacters_ShouldReplaceWithUnderscore(string input)
    {
        // Given: sanitizer using default replacement character '_'
        var expected = "test_file.txt";
        Sanitizer sut = SetUpSut();

        // When: sanitizing an input that contains special characters
        var actual = sut.SanitizeFileName(input);

        // Then: the output should use underscores and be considered sanitized
        actual.ShouldBe(expected);
        sut.IsFilenameSanitized(actual).ShouldBeTrue();
    }

    // Original test cases from FileNameSanitizer.Tests
    [Theory]
    [InlineData("")]
    public void SanitizeFileName_WithEmpty_ShouldReturnEmpty(string input)
    {
        // Given: sanitizer using default settings
        Sanitizer sut = SetUpSut();

        // When: sanitizing an empty string
        var actual = sut.SanitizeFileName(input);

        // Then: result should be empty and considered sanitized
        actual.ShouldBe("");
        sut.IsFilenameSanitized(actual).ShouldBeTrue();
    }

    [Fact]
    public void SanitizeFileName_WithNull_ShouldReturnEmpty()
    {
        // Given: sanitizer using default settings and a null input
        Sanitizer sut = SetUpSut();
        string? nullString = null;

        // When: sanitizing a null filename
        var actual = sut.SanitizeFileName(nullString);

        // Then: result should be empty and considered sanitized
        actual.ShouldBe("");
        sut.IsFilenameSanitized(actual).ShouldBeTrue();
    }

    [Theory]
    [InlineData("test-file.txt", "test-file.txt", '-')] // Excluded character should be preserved
    [InlineData("test#file.txt", "test#file.txt", '#')] // Excluded character should be preserved
    [InlineData("test@file.txt", "test_file.txt", '-')] // Non-excluded character should be replaced
    public void SanitizeFileName_WhenUsingExcludedCharacters_ShouldPreserveExcludedAndReplaceOthers(
        string input, string expected, char excludedChar)
    {
        // Given: sanitizer configured to preserve the excluded character
        var sut = SetUpSut("_", new List<string> { excludedChar.ToString() });

        // When: sanitizing the input with the excluded character
        var actual = sut.SanitizeFileName(input);

        // Then: excluded character is preserved and other invalid chars replaced
        actual.ShouldBe(expected);
    }

    [Fact]
    public void SanitizeFileName_WhenReplacementCharacterInInvalidChars_ShouldNotReplaceItself()
    {
        // Given: sanitizer with no excluded characters and replacement character '_'
        var sut = SetUpSut("_", new List<string>());

        // When: sanitizing a filename that already contains underscores
        var actual = sut.SanitizeFileName("test_file_name.txt");

        // Then: underscores should be preserved
        actual.ShouldBe("test_file_name.txt");
    }

    [Fact]
    public void SanitizeFileName_WithMultipleExcludedCharacters_ShouldPreserveAll()
    {
        // Given: sanitizer configured to preserve '-' and '#'
        var sut = SetUpSut("_", new List<string> { "-", "#" });

        // When: sanitizing a filename with multiple excluded characters
        var actual = sut.SanitizeFileName("test-file#name@.txt");

        // Then: '-' and '#' are preserved; '@' is replaced by the replacement character
        actual.ShouldBe("test-file#name.txt");
    }
}