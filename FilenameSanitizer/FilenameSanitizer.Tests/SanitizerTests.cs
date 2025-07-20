using Shouldly;
using Xunit;
using FilenameSanitizer;

namespace FilenameSanitizer.Tests;

public class SanitizerTests
{
    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
    [InlineData("test/file.txt", "test_file.txt")]
    [InlineData("test\\file.txt", "test_file.txt")]
    [InlineData("test<>file.txt", "test_file.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("PRN.doc", "_PRN.doc")]
    public void SanitizeFileName_ShouldSanitizeCorrectly(string input, string expected)
    {
        // Test
        var actual = Sanitizer.SanitizeFileName(input);

        // Verify
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(" path/to/test_file.txt", "test_file.txt")]
    [InlineData("path/to/test_file.txt ", "test_file.txt")]
    [InlineData(" path/to/test_file.txt ", "test_file.txt")]
    [InlineData("path/to/ test_file.txt", "test_file.txt")]
    [InlineData("path/to/test_file .txt", "test_file.txt")]
    [InlineData("path/to/test_file. txt", "test_file.txt")]
    public void GetSanitizedFilenameWithPathRemoved_WhenContainsSpaces_ShouldSanitize(string input, string expected)
    {
        // Test
        var actual = Sanitizer.GetSanitizedFilenameWithPathRemoved(input);

        // Verify
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("path/to/test?file.txt")]
    [InlineData("path/to/test=file.txt")]
    [InlineData("path/to/test`file.txt")]
    [InlineData("path/to/test'file.txt")]
    [InlineData("path/to/test¨file.txt")]
    [InlineData("path/to/test~file.txt")]
    [InlineData("path/to/test^file.txt")]
    [InlineData("path/to/test*file.txt")]
    [InlineData("path/to/test@file.txt")]
    [InlineData("path/to/test£file.txt")]
    [InlineData("path/to/test€file.txt")]
    [InlineData("path/to/test$file.txt")]
    [InlineData("path/to/test;file.txt")]
    [InlineData("path/to/test-file.txt")]
    [InlineData("path/to/test&file.txt")]
    [InlineData("path/to/test!file.txt")]
    [InlineData("path/to/test[file.txt")]
    [InlineData("path/to/test]file.txt")]
    [InlineData("path/to/test{file.txt")]
    [InlineData("path/to/test}file.txt")]
    [InlineData("path/to/test(file.txt")]
    [InlineData("path/to/test)file.txt")]
    [InlineData("path/to/test<file.txt")]
    [InlineData("path/to/test>file.txt")]
    [InlineData("path/to/test|file.txt")]
    public void GetSanitizedFilenameWithPathRemoved_WhenContainsSpecialCharacters_ShouldReplaceWithUnderscore(string input)
    {
        // Setup
        var expected = "test_file.txt";

        // Test
        var actual = Sanitizer.GetSanitizedFilenameWithPathRemoved(input);

        // Verify
        actual.ShouldBe(expected);
    }

    // Original test cases from FileNameSanitizer.Tests
    [Theory]
    [InlineData("file:name.txt", "file_name.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("file....txt", "file.txt")]
    [InlineData("  file  .  txt  ", "file.txt")]
    public void GetSanitizedFilenameWithPathRemoved_ShouldSanitizeCorrectly(string input, string expected)
    {
        // Setup
        // Setup

        // Test
        var actual = Sanitizer.GetSanitizedFilenameWithPathRemoved(input);
        // Verify
        actual.ShouldBe(expected);
    }
}
