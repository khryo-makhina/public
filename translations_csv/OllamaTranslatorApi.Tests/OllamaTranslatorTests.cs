using Shouldly;

namespace OllamaTranslatorApi.Tests;

//namespace TranslationTools.OllamaApi.Tests
public class OllamaTranslatorTests : IDisposable
{
    private readonly string _testInputCsv = Path.Combine(AppContext.BaseDirectory, "test_input.csv");
    private readonly string _testOutputCsv = Path.Combine(AppContext.BaseDirectory, "test_output.csv");
    private readonly string _testSampleCsv = Path.Combine(AppContext.BaseDirectory, "sample_input.csv");
    private readonly OllamaTranslator _translator;
    private bool _disposed;

    public OllamaTranslatorTests()
    {
        var mockService = new TestTranslationService();
        _translator = new OllamaTranslator(mockService);
        CreateTestFiles();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Clean up managed resources
            CleanupFiles();
        }
        // Clean up unmanaged resources if any (none in this case)
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task TestMockTranslation()
    {
        // Given a test translation service
        var service = new TestTranslationService();

        // When translating "Hello"
        var result = await service.TranslateAsync("Hello");

        // Then the result should be the Spanish greeting
        Assert.Equal("Hola", result);
    }

    [Fact]
    public async Task TestUnknownTranslation()
    {
        // Given a test translation service
        var service = new TestTranslationService();

        // When translating an unknown token
        var result = await service.TranslateAsync("Unknown");

        // Then the service should return the mock translation indicator
        Assert.Equal("MockTranslation:Unknown", result);
    }

    [Fact]
    public async Task ProcessCsvAsync_ShouldProcessInputAndCreateOutputFile()
    {
        // Given a translator using the test translation service and a temp CSV input
        var translator = new OllamaTranslator(new TestTranslationService());
        var (testInput, testOutput) = CreateTempInputOutputCsv("SourceText,TargetText\nhello,\nworld,\n");

        try
        {
            // When processing the CSV
            await translator.ProcessCsvAsync(testInput, testOutput);

            // Then an output file should be created containing the translated lines
            Assert.True(File.Exists(testOutput), "Output file was not created");
            var outputContent = await File.ReadAllTextAsync(testOutput);
            Assert.Contains("SourceText", outputContent);
            Assert.Contains("TargetText", outputContent);
            Assert.Contains("hello", outputContent);
            Assert.Contains("world", outputContent);
        }
        finally
        {
            DeleteIfExists(testInput);
            DeleteIfExists(testOutput);
        }
    }

    [Fact]
    public async Task BatchTranslateAsync_ShouldTranslateAllEntries()
    {
        // Given a set of CSV entries to translate
        var testEntries = new List<CsvEntry>
        {
            new() { SourceText = "Hello" }, new() { SourceText = "World" }, new() { SourceText = "Test" }
        };

        // When translating the batch
        List<CsvEntry> translated = await _translator.BatchTranslateAsync(testEntries);

        // Then all entries should be translated
        translated.ShouldNotBeNull();
        translated.Count.ShouldBe(3);
        translated[0].TargetText.ShouldBe("Hola");
        translated[1].TargetText.ShouldBe("Mundo");
        translated[2].TargetText.ShouldBe("Prueba");
    }


    [Fact]
    public async Task BatchTranslateAsync_ShouldRespectMaxParallelTasks()
    {
        // Given a translator and 10 entries
        var translator = new OllamaTranslator();
        List<CsvEntry> entries = Enumerable.Range(1, 10)
            .Select(i => new CsvEntry { SourceText = $"Text {i}", TargetText = "" })
            .ToList();

        // When translating with a max parallelism of 3
        List<CsvEntry> translatedEntries = await translator.BatchTranslateAsync(entries, 3);

        // Then all entries should be translated
        translatedEntries.Count.ShouldBe(10);
        foreach (CsvEntry entry in translatedEntries)
        {
            entry.TargetText.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ProcessCsvAsync_ShouldHandleEmptyInputFile()
    {
        // Given a translator and an empty input file
        var translator = new OllamaTranslator();
        var emptyInputFile = CreateTempEmptyFile();

        // When processing the empty file
        await Should.NotThrowAsync(() =>
            translator.ProcessCsvAsync(emptyInputFile, "empty_output.csv"));

        // Then an output file should still be created
        File.Exists("empty_output.csv").ShouldBeTrue();
    }

    [Fact]
    public async Task BatchTranslateAsync_ShouldHandleSingleEntry()
    {
        // Given a translator and a single entry
        var translator = new OllamaTranslator();
        var entries = new List<CsvEntry> { new() { SourceText = "Single test", TargetText = "" } };

        // When translating the single entry
        List<CsvEntry> translatedEntries = await translator.BatchTranslateAsync(entries);

        // Then the entry should be translated
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
        // Given a translator using the test translation service and a non-existent input path
        var translator = new OllamaTranslator(new TestTranslationService());
        var nonExistentPath = GetPathForFile("DefinitelyNonexistentFile12345.csv");

        // When processing the non-existent file
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            translator.ProcessCsvAsync(nonExistentPath, _testOutputCsv));

        // Then a FileNotFoundException should be thrown referencing the expected path
        Assert.Contains(nonExistentPath, exception.Message);
    }

    private void CleanupFiles()
    {
        var filesToDelete = new[]
        {
            _testInputCsv, _testOutputCsv, _testSampleCsv, "empty_input.csv", "empty_output.csv"
        };

        foreach (var file in filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // Log or handle cleanup errors (optional)
                Console.WriteLine($"Failed to delete {file}: {ex.Message}");
            }
        }
    }

    // Helper to create temp input and output file paths and write provided content to the input
    private (string inputPath, string outputPath) CreateTempInputOutputCsv(string content)
    {
        var testInput = "test_input_" + Guid.NewGuid() + ".csv";
        var input = GetPathForFile(testInput);
        var testOutput = "test_output_" + Guid.NewGuid() + ".csv";
        var output = GetPathForFile(testOutput);
        File.WriteAllText(input, content);
        return (input, output);
    }

    private static string GetPathForFile(string testInput)
    {
        return Path.Combine(Path.GetTempPath(), testInput);
    }

    // Helper to create an empty temp file
    private string CreateTempEmptyFile()
    {
        var emptyInput = "empty_input_" + Guid.NewGuid() + ".csv";
        var path = GetPathForFile(emptyInput);
        File.WriteAllText(path, string.Empty);
        return path;
    }

    // Helper to delete a file if it exists
    private void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore cleanup failures in tests
        }
    }
}