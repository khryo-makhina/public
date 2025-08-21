using System;
using NSubstitute;
using Shouldly;
using Xunit;

namespace FilenameSanitizer.Tests
{
    public class SanitizerTests
    {
        public SanitizerTests()
        {
            SetUpSut();
        }

        private Sanitizer SetUpSut(char replacementCharacter = '_', char[]? excludedChars = null)
        {
            // Initialize any required settings or dependencies here
            var sanitzerSettingsLoader = Substitute.For<ISanitizerSettingsLoader>();
            var sanitizerSetting = Substitute.For<ISanitizerSetting>();
            sanitizerSetting.ReplacementCharacter.Returns(replacementCharacter);
            sanitizerSetting.ExcludedCharacters.Returns(excludedChars ?? Array.Empty<char>());
            sanitzerSettingsLoader.LoadFromFile(Arg.Any<string>()).Returns(sanitizerSetting);

            return new Sanitizer(sanitzerSettingsLoader);
        }

        [Theory]
        [InlineData("test:file.txt", "test file.txt")]
        [InlineData("test/file.txt", "test file.txt")]
        [InlineData("test\\file.txt", "test file.txt")]
        [InlineData("test<>file.txt", "test file.txt")]
        [InlineData("COM1 .txt", "_COM1.txt")]
        [InlineData("PRN .doc", "_PRN.doc")]
        public void SanitizeFileName_SpaceIsReplacementCharacter_SanitizedFilenameContainsSpace(string input, string expected)
        {
            //Setup
            Sanitizer sut = SetUpSut(' ');

            // Test
            var actual = sut.SanitizeFileName(input);

            // Verify
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData("test:file.txt", "test_file.txt")]
        [InlineData("test/file.txt", "test_file.txt")]
        [InlineData("test\\file.txt", "test_file.txt")]
        [InlineData("test<>file.txt", "test_file.txt")]
        [InlineData("COM1.txt", "_COM1.txt")]
        [InlineData("PRN.doc", "_PRN.doc")]
        public void SanitizeFileName_UnderscoreIsReplacementCharacterAsDefault_SanitizedFilenameContainsUnderscore(string input, string expected)
        {
            //Setup
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.SanitizeFileName(input);

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
            //Setup
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.GetSanitizedFilenameWithPathRemoved(input);

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
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.GetSanitizedFilenameWithPathRemoved(input);

            // Verify
            actual.ShouldBe(expected);
        }

        // Original test cases from FileNameSanitizer.Tests
        [Theory]
        [InlineData("file:name.txt", "file_name.txt")]
        [InlineData("COM1.txt", "_COM1.txt")]//Windows reserved name: COM1
        [InlineData("file....txt", "file.txt")]
        [InlineData("  file  .  txt  ", "file.txt")]
        public void GetSanitizedFilenameWithPathRemoved_ShouldSanitizeCorrectly(string input, string expected)
        {
            // Setup
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.GetSanitizedFilenameWithPathRemoved(input);

            // Verify
            actual.ShouldBe(expected);
        }   

        [Theory]
        [InlineData("test-file.txt", "test-file.txt", '-')] // Excluded character should be preserved
        [InlineData("test#file.txt", "test#file.txt", '#')] // Excluded character should be preserved
        [InlineData("test@file.txt", "test_file.txt", '-')] // Non-excluded character should be replaced
        public void SanitizeFileName_WhenUsingExcludedCharacters_ShouldPreserveExcludedAndReplaceOthers(
            string input, string expected, char excludedChar)
        {
            // Setup
            var sut = SetUpSut('_', new[] { excludedChar });

            // Test
            var actual = sut.SanitizeFileName(input);

            // Verify
            actual.ShouldBe(expected);
        }

        [Fact]
        public void SanitizeFileName_WhenReplacementCharacterInInvalidChars_ShouldNotReplaceItself()
        {
            // Setup
            var sut = SetUpSut('_', Array.Empty<char>());

            // Test
            var actual = sut.SanitizeFileName("test_file_name.txt");

            // Verify
            actual.ShouldBe("test_file_name.txt"); // Underscores should be preserved
        }

        [Fact]
        public void SanitizeFileName_WithMultipleExcludedCharacters_ShouldPreserveAll()
        {
            // Setup
            var sut = SetUpSut('_', new[] { '-', '#' });

            // Test
            var actual = sut.SanitizeFileName("test-file#name@.txt");

            // Verify
            actual.ShouldBe("test-file#name.txt"); // '-' and '#' should be preserved, '@' replaced
        }
    }
}
