using CsvHelper;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationTools.OllamaApi;
using Xunit;

namespace TranslationTools.Tests;

//namespace TranslationTools.OllamaApi.Tests
public class OllamaTranslatorTests : IDisposable
{
    private readonly string _testInputCsv = Path.Combine(AppContext.BaseDirectory, "test_input.csv");
    private readonly string _testOutputCsv = Path.Combine(AppContext.BaseDirectory, "test_output.csv");
    private readonly string _testSampleCsv = Path.Combine(AppContext.BaseDirectory, "sample_input.csv");
    private readonly OllamaTranslator _translator;
    private readonly TestTranslationService _mockService;

    public OllamaTranslatorTests()
    {
        _mockService = new TestTranslationService();
        _translator = new OllamaTranslator(_mockService);
        CreateTestFiles();
    }

    [Fact]
    public async Task TestMockTranslation()
    {
        var service = new TestTranslationService();
        var result = await service.TranslateAsync("Hello");
        Assert.Equal("Hola", result);
    }

    [Fact]
    public async Task TestUnknownTranslation()
    {
        var service = new TestTranslationService();
        var result = await service.TranslateAsync("Unknown");
        Assert.Equal("MockTranslation:Unknown", result);
    }

    [Fact]
    public async Task ProcessCsvAsync_ShouldProcessInputAndCreateOutputFile()
    {
        // Arrange
        var translator = new OllamaTranslator(new TestTranslationService());

        // Create test input file
        var testInput = Path.Combine(Path.GetTempPath(), "test_input_" + Guid.NewGuid() + ".csv");
        var testOutput = Path.Combine(Path.GetTempPath(), "test_output_" + Guid.NewGuid() + ".csv");

        try
        {
            // Setup test data with correct headers
            File.WriteAllText(testInput, "SourceText,TargetText\nhello,\nworld,\n");

            // Act
            await translator.ProcessCsvAsync(testInput, testOutput);

            // Assert
            Assert.True(File.Exists(testOutput), "Output file was not created");
            var outputContent = File.ReadAllText(testOutput);

            // Verify output has correct headers and content
            Assert.Contains("SourceText", outputContent);
            Assert.Contains("TargetText", outputContent);
            Assert.Contains("hello", outputContent);
            Assert.Contains("world", outputContent);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testInput)) File.Delete(testInput);
            if (File.Exists(testOutput)) File.Delete(testOutput);
        }
    }

    [Fact]
    public async Task BatchTranslateAsync_ShouldTranslateAllEntries()
    {
        // Arrange
        var testEntries = new List<CsvEntry>
        {
            new CsvEntry { SourceText = "Hello" },
            new CsvEntry { SourceText = "World" },
            new CsvEntry { SourceText = "Test" }
        };

        // Act
        var translated = await _translator.BatchTranslateAsync(testEntries);

        // Assert
        translated.ShouldNotBeNull();
        translated.Count.ShouldBe(3);

        translated[0].TargetText.ShouldBe("Hola");
        translated[1].TargetText.ShouldBe("Mundo");
        translated[2].TargetText.ShouldBe("Prueba");
    }


    [Fact]
    public async Task BatchTranslateAsync_ShouldRespectMaxParallelTasks()
    {
        // Arrange
        var translator = new OllamaTranslator();
        var entries = Enumerable.Range(1, 10)
            .Select(i => new CsvEntry { SourceText = $"Text {i}", TargetText = "" })
            .ToList();

        // Act
        var translatedEntries = await translator.BatchTranslateAsync(entries, maxParallelTasks: 3);

        // Assert
        translatedEntries.Count.ShouldBe(10);
        foreach (var entry in translatedEntries)
        {
            entry.TargetText.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ProcessCsvAsync_ShouldHandleEmptyInputFile()
    {
        // Arrange
        var translator = new OllamaTranslator();
        var emptyInputFile = "empty_input.csv";

        // Create empty file
        File.WriteAllText(emptyInputFile, "");

        // Act & Assert
        await Should.NotThrowAsync(() =>
            translator.ProcessCsvAsync(emptyInputFile, "empty_output.csv"));

        File.Exists("empty_output.csv").ShouldBeTrue();
    }

    [Fact]
    public async Task BatchTranslateAsync_ShouldHandleSingleEntry()
    {
        // Arrange
        var translator = new OllamaTranslator();
        var entries = new List<CsvEntry>
            {
                new CsvEntry { SourceText = "Single test", TargetText = "" }
            };

        // Act
        var translatedEntries = await translator.BatchTranslateAsync(entries);

        // Assert
        translatedEntries.Count.ShouldBe(1);
        translatedEntries[0].TargetText.ShouldNotBeNullOrWhiteSpace();
    }

    private void CreateTestFiles()
    {
        // Create sample input file
        var sampleData = @"SourceText,TargetText
Hello,,
World,,
Test,""";

        File.WriteAllText(_testSampleCsv, sampleData);

        // Create test input file with more data
        var testData = @"SourceText,TargetText
Hello,,
World,,
Test,,
Another test,,
One more,,";

        File.WriteAllText(_testInputCsv, testData);
    }


    [Fact]
    public async Task ProcessCsvAsync_WithNonExistentInput_ThrowsFileNotFoundException()
    {
        // Arrange
        var translator = new OllamaTranslator(new TestTranslationService());

        // Create a path that definitely doesn't exist
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "DefinitelyNonexistentFile12345.csv");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            translator.ProcessCsvAsync(nonExistentPath, _testOutputCsv));

        // Optional: Verify the exception message contains the expected path
        Assert.Contains(nonExistentPath, exception.Message);
    }

    public void Dispose()
    {
        // Clean up test files (synchronous)
        CleanupFiles();

    }

    private void CleanupFiles()
    {
        var filesToDelete = new[]
        {
            _testInputCsv,
            _testOutputCsv,
            _testSampleCsv,
            "empty_input.csv",
            "empty_output.csv"
        };

        foreach (var file in filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                // Log or handle cleanup errors (optional)
                Console.WriteLine($"Failed to delete {file}: {ex.Message}");
            }
        }
    }

}


