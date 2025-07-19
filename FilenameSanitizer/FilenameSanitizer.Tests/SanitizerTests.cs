using Shouldly;
using Xunit;

namespace FileNameSanitizer.Tests;

public class SanitizerTests
{
    [Theory]
    [InlineData(" path/to/test_file.txt", "test_file.txt")]
    [InlineData("path/to/test_file.txt ", "test_file.txt")]
    [InlineData(" path/to/test_file.txt ", "test_file.txt")]
    [InlineData("path/to/ test_file.txt", "test_file.txt")]
    [InlineData("path/to/test_file .txt", "test_file.txt")]
    [InlineData("path/to/test_file. txt", "test_file.txt")]
    public void GetSanitizedFilenameWithPathRemoved_WhenContainsSpaces_ShouldSanitize(string input, string expected)
    {
        // Setup
        var sut = new Sanitizer();

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
        var sut = new Sanitizer();
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
        var sut = new Sanitizer();

        // Test
        var actual = Sanitizer.GetSanitizedFilenameWithPathRemoved(input);

        // Verify
        actual.ShouldBe(expected);
    }
}
