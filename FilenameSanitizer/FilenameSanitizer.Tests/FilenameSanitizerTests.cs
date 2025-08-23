using Shouldly;
using Xunit;
using NSubstitute;

namespace FilenameSanitizer.Tests;

public partial class FilenameSanitizerTests
{
    private readonly IFileSystem _fileSystem;
    private const string TestFolder = @"C:\TestFolder";

    private ISanitizer _sanitizer;

    public FilenameSanitizerTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.DirectoryExists(TestFolder).Returns(true);
        _sanitizer = Substitute.For<ISanitizer>();
        SetUpSut();
    }

    private IFilenameSanitizer SetUpSut()
    {
        var sut = new FilenameSanitizer(TestFolder, _sanitizer, _fileSystem);
        return sut;
    }

    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
    [InlineData("test<>file.txt", "test_file.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("PRN.doc", "_PRN.doc")]
    public void RenameFilesToMeetOsRequirements_ShouldSanitizeFilenames(string originalName, string expectedName)
    {
        // Setup
        var filePathSource = Path.Combine(TestFolder, originalName);
        var filePathDestination = Path.Combine(TestFolder, expectedName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePathSource });
        _fileSystem.FileExists(filePathDestination).Returns(false);
        _sanitizer.SanitizeFileName(originalName).Returns(expectedName);
        _sanitizer.IsFilenameSanitized(originalName).Returns(false);

        var sut = SetUpSut();

        // Test
        sut.RenameFilesToMeetOsRequirements();

        // Verify
        sut.Logger.HasErrors.ShouldBeFalse();
        _fileSystem.Received(1).MoveFile(filePathSource, filePathDestination);
    }

    [Theory]
    [InlineData("prefix-test.txt", "test.txt")]
    [InlineData("test_old.txt", "test.txt")]
    [InlineData("test.bak.txt", "test.txt")]
    public void RenameFilesRemovingPatterns_ShouldRemoveSpecifiedPatterns(string originalName, string expectedName)
    {
        // Setup
        var filePathSource = Path.Combine(TestFolder, originalName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePathSource });
        var filePathDestination = Path.Combine(TestFolder, expectedName);

        _fileSystem.FileExists(filePathDestination).Returns(false);
        var patterns = @"prefix-
_old
.bak";
        _sanitizer.SanitizeFileName(originalName).Returns(expectedName);
        _sanitizer.IsFilenameSanitized(originalName).Returns(false);
        var sut = SetUpSut();

        // Test
        sut.RenameFilesRemovingPatterns(patterns);

        // Verify
        sut.Logger.HasErrors.ShouldBeFalse();
        _fileSystem.Received(1).MoveFile(
            Arg.Is<string>(x => x == filePathSource),
            Arg.Is<string>(x => x == filePathDestination));
    }

    [Fact]
    public void RenameFilesToMeetOsRequirements_WhenFolderDoesNotExist_ShouldLogError()
    {
        // Setup
        var nonExistentFolder = Path.Combine(TestFolder, "NonExistent");
        _fileSystem.DirectoryExists(nonExistentFolder).Returns(false);
        var sut = new FilenameSanitizer(nonExistentFolder, _sanitizer, _fileSystem);

        // Test
        sut.RenameFilesToMeetOsRequirements();

        // Verify
        sut.Logger.HasErrors.ShouldBeTrue();
        sut.Logger.Errors.ShouldContain(e => e.Contains("does not exist"));
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("normalfile.doc")]
    public void RenameFilesToMeetOsRequirements_WhenFilenameIsAlreadyValid_ShouldSkip(string fileName)
    {
        // Setup
        var filePath = Path.Combine(TestFolder, fileName);
        _fileSystem.GetFiles(TestFolder).Returns(new[] { filePath });
        _sanitizer.IsFilenameSanitized(fileName).Returns(true);
        _sanitizer.SanitizeFileName(fileName).Returns(fileName);
        var sut = SetUpSut();

        // Test
        sut.RenameFilesToMeetOsRequirements();

        // Verify
        sut.Logger.HasWarnings.ShouldBeTrue();
        sut.Logger.Warnings.ShouldContain(w => w.Contains("already sanitized"));
        _fileSystem.DidNotReceive().MoveFile(Arg.Any<string>(), Arg.Any<string>());
    }
}