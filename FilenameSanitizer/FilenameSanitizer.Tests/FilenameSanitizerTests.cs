using Shouldly;
using Xunit;
using NSubstitute;

namespace FilenameSanitizer.Tests;

public partial class FilenameSanitizerTests
{
    private readonly IFileSystem _fileSystem;
    private const string TestFolder = @"C:\TestFolder";

    private readonly ISanitizerSettingsLoader _sanitzerSettingsLoader;


    public FilenameSanitizerTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _fileSystem.DirectoryExists(TestFolder).Returns(true);

        var sanitizerSetting = Substitute.For<ISanitizerSetting>();
        sanitizerSetting.ReplacementCharacter.Returns('_');
        _sanitzerSettingsLoader = Substitute.For<ISanitizerSettingsLoader>();
        _sanitzerSettingsLoader.LoadFromFile(Arg.Any<string>()).Returns(sanitizerSetting);
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
        var sut = new FilenameSanitizer(TestFolder, _sanitzerSettingsLoader, _fileSystem);

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
        var sut = new FilenameSanitizer(TestFolder, _sanitzerSettingsLoader, _fileSystem);

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
        var sut = new FilenameSanitizer(nonExistentFolder, _sanitzerSettingsLoader, _fileSystem);

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
        var sut = new FilenameSanitizer(TestFolder, _sanitzerSettingsLoader, _fileSystem);

        // Test
        sut.RenameFilesToMeetOsRequirements();

        // Verify
        sut.Logger.HasWarnings.ShouldBeTrue();
        sut.Logger.Warnings.ShouldContain(w => w.Contains("already sanitized"));
        _fileSystem.DidNotReceive().MoveFile(Arg.Any<string>(), Arg.Any<string>());
    }
}