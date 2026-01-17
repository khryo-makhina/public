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

        private Sanitizer SetUpSut(string replacementCharacter = "_", List<string>? excludedChars = null)
        {
            // Initialize any required settings or dependencies here
            var sanitzerSettingsLoader = Substitute.For<ISanitizerSettingsLoader>();
            var sanitizerSetting = Substitute.For<ISanitizerSetting>();
            sanitizerSetting.ReplacementCharacter.Returns(replacementCharacter);
            sanitizerSetting.ExcludedCharacters.Returns(excludedChars ?? new List<string>());
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
            Sanitizer sut = SetUpSut(" ");

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
        [InlineData("test_file.txt", true)]
        [InlineData("test:file.txt", false)]
        [InlineData("test<>file.txt", false)]
        [InlineData("COM1.txt", false)]
        [InlineData("test file.txt", false)]
        public void IsFilenameSanitized_ShouldReturnCorrectResult(string filename, bool expected)
        {
            //Setup
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.IsFilenameSanitized(filename);

            // Verify
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
            // Setup
            var expected = "test_file.txt";
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.SanitizeFileName(input);

            // Verify
            actual.ShouldBe(expected);
            sut.IsFilenameSanitized(actual).ShouldBeTrue();
        }

        // Original test cases from FileNameSanitizer.Tests
        [Theory]
        [InlineData("")]
        public void SanitizeFileName_WithEmpty_ShouldReturnEmpty(string input)
        {
            // Setup
            Sanitizer sut = SetUpSut();

            // Test
            var actual = sut.SanitizeFileName(input);

            // Verify
            actual.ShouldBe("");
            sut.IsFilenameSanitized(actual).ShouldBeTrue();
        }

        [Fact]
        public void SanitizeFileName_WithNull_ShouldReturnEmpty()
        {
            // Setup
            Sanitizer sut = SetUpSut();
            string? nullString = null;

            // Test
            var actual = sut.SanitizeFileName(nullString);

            // Verify
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
            // Setup
            var sut = SetUpSut("_", new List<string>() { excludedChar.ToString() });

            // Test
            var actual = sut.SanitizeFileName(input);

            // Verify
            actual.ShouldBe(expected);
        }

        [Fact]
        public void SanitizeFileName_WhenReplacementCharacterInInvalidChars_ShouldNotReplaceItself()
        {
            // Setup
            var sut = SetUpSut("_", new List<string>());

            // Test
            var actual = sut.SanitizeFileName("test_file_name.txt");

            // Verify
            actual.ShouldBe("test_file_name.txt"); // Underscores should be preserved
        }

        [Fact]
        public void SanitizeFileName_WithMultipleExcludedCharacters_ShouldPreserveAll()
        {
            // Setup
            var sut = SetUpSut("_", new List<string> { "-", "#" });

            // Test
            var actual = sut.SanitizeFileName("test-file#name@.txt");

            // Verify
            actual.ShouldBe("test-file#name.txt"); // '-' and '#' should be preserved, '@' replaced
        }
    }
}
