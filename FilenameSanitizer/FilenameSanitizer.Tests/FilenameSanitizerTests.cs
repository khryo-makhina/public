using Shouldly;
using Xunit;
using NSubstitute;

namespace FilenameSanitizer.Tests;

public class FilenameSanitizerTests
{
    private readonly IFileSystem _fileSystem;
    private const string TestFolder = @"C:\TestFolder";

    public FilenameSanitizerTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.DirectoryExists(TestFolder).Returns(true);
    }

    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
    [InlineData("test<>file.txt", "test_file.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("PRN.doc", "_PRN.doc")]
    public void RenameFilesToMeetOsRequirements_ShouldSanitizeFilenames(string originalName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(TestFolder, originalName);
        var expectedPath = Path.Combine(TestFolder, expectedName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePath });
        _fileSystem.FileExists(expectedPath).Returns(false);
        var sut = new FilenameSanitizer(TestFolder, _fileSystem);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasErrors.ShouldBeFalse();
        _fileSystem.Received(1).MoveFile(filePath, expectedPath);
    }

    [Theory]
    [InlineData("prefix-test.txt", "test.txt")]
    [InlineData("test_old.txt", "test.txt")]
    [InlineData("test.bak.txt", "test.txt")]
    public void RenameFilesRemovingPatterns_ShouldRemoveSpecifiedPatterns(string originalName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(TestFolder, originalName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePath });
        _fileSystem.FileExists(Path.Combine(TestFolder, expectedName)).Returns(false);
        var patterns = @"prefix-
_old
.bak";
        var sut = new FilenameSanitizer(TestFolder, _fileSystem);

        // Test
        var operation = sut.RenameFilesRemovingPatterns(patterns);

        // Verify
        operation.Log.HasErrors.ShouldBeFalse();
        _fileSystem.Received(1).MoveFile(
            Arg.Is<string>(s => s == filePath),
            Arg.Is<string>(s => s == Path.Combine(TestFolder, expectedName)));
    }

    [Fact]
    public void RenameFilesToMeetOsRequirements_WhenFolderDoesNotExist_ShouldLogError()
    {
        // Setup
        var nonExistentFolder = Path.Combine(TestFolder, "NonExistent");
        _fileSystem.DirectoryExists(nonExistentFolder).Returns(false);
        var sut = new FilenameSanitizer(nonExistentFolder, _fileSystem);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasErrors.ShouldBeTrue();
        operation.Log.Errors.ShouldContain(e => e.Contains("does not exist"));
    }

    [Theory]
    [InlineData("test.txt", "test.txt")]
    [InlineData("normalfile.doc", "normalfile.doc")]
    public void RenameFilesToMeetOsRequirements_WhenFilenameIsAlreadyValid_ShouldSkip(string fileName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(TestFolder, fileName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePath });
        var sut = new FilenameSanitizer(TestFolder, _fileSystem);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasWarnings.ShouldBeTrue();
        operation.Log.Warnings.ShouldContain(w => w.Contains("already sanitized"));
        _fileSystem.DidNotReceive().MoveFile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
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
}