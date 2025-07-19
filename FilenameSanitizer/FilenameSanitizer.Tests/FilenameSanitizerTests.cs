using Shouldly;
using Xunit;

namespace FilenameSanitizer.Tests;

public class FilenameSanitizerTests
{
    private readonly string _testFolder;

    public FilenameSanitizerTests()
    {
        // Setup test folder
        _testFolder = Path.Combine(Path.GetTempPath(), "FilenameSanitizerTests");
        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
        Directory.CreateDirectory(_testFolder);
    }

    [Theory]
    [InlineData("test:file.txt", "test_file.txt")]
    [InlineData("test/file.txt", "test_file.txt")]
    [InlineData("test\\file.txt", "test_file.txt")]
    [InlineData("test<>file.txt", "test_file.txt")]
    [InlineData("COM1.txt", "_COM1.txt")]
    [InlineData("PRN.doc", "_PRN.doc")]
    public async Task RenameFilesToMeetOsRequirements_ShouldSanitizeFilenames(string originalName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(_testFolder, originalName);
        await File.WriteAllTextAsync(filePath, "test content");
        var sut = new FilenameSanitizer(_testFolder);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasErrors.ShouldBeFalse();
        File.Exists(Path.Combine(_testFolder, expectedName)).ShouldBeTrue();
        File.Exists(filePath).ShouldBeFalse();
    }

    [Theory]
    [InlineData("prefix-test.txt", "test.txt")]
    [InlineData("test_old.txt", "test.txt")]
    [InlineData("test.bak.txt", "test.txt")]
    public async Task RenameFilesRemovingPatterns_ShouldRemoveSpecifiedPatterns(string originalName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(_testFolder, originalName);
        await File.WriteAllTextAsync(filePath, "test content");
        var patterns = @"prefix-
_old
.bak";
        var sut = new FilenameSanitizer(_testFolder);

        // Test
        var operation = sut.RenameFilesRemovingPatterns(patterns);

        // Verify
        operation.Log.HasErrors.ShouldBeFalse();
        File.Exists(Path.Combine(_testFolder, expectedName)).ShouldBeTrue();
        File.Exists(filePath).ShouldBeFalse();
    }

    [Fact]
    public void RenameFilesToMeetOsRequirements_WhenFolderDoesNotExist_ShouldLogError()
    {
        // Setup
        var nonExistentFolder = Path.Combine(_testFolder, "NonExistent");
        var sut = new FilenameSanitizer(nonExistentFolder);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasErrors.ShouldBeTrue();
        operation.Log.Errors.ShouldContain(e => e.Contains("does not exist"));
    }

    [Theory]
    [InlineData("test.txt", "test.txt")]
    [InlineData("normalfile.doc", "normalfile.doc")]
    public async Task RenameFilesToMeetOsRequirements_WhenFilenameIsAlreadyValid_ShouldSkip(string fileName, string expectedName)
    {
        // Setup
        var filePath = Path.Combine(_testFolder, fileName);
        await File.WriteAllTextAsync(filePath, "test content");
        var sut = new FilenameSanitizer(_testFolder);

        // Test
        var operation = sut.RenameFilesToMeetOsRequirements();

        // Verify
        operation.Log.HasWarnings.ShouldBeTrue();
        operation.Log.Warnings.ShouldContain(w => w.Contains("already sanitized"));
        File.Exists(Path.Combine(_testFolder, expectedName)).ShouldBeTrue();
    }
}
