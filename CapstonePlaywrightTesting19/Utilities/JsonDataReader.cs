using System.Text.Json;

namespace CapstonePlaywrightTesting19.Utilities;

/// <summary>Reads data-driven test cases from JSON files under TestData/.</summary>
public static class JsonDataReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static T Read<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test data file not found: {path}");
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
               ?? throw new InvalidDataException($"Test data file is empty: {path}");
    }

    /// <summary>
    /// Builds NUnit <see cref="TestCaseData"/> from a JSON array, naming each case by its "Id"
    /// so every data row shows up as its own test in the report.
    /// </summary>
    public static IEnumerable<TestCaseData> Cases<T>(string fileName) where T : IDataCase
    {
        foreach (var item in Read<List<T>>(fileName))
        {
            yield return new TestCaseData(item).SetName($"{item.Id}_{item.Title}");
        }
    }
}

/// <summary>Every data row carries a test-case id (matches the spreadsheet) and a short title.</summary>
public interface IDataCase
{
    string Id { get; }
    string Title { get; }
}
