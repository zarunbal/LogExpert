using System.Runtime.Versioning;

using Newtonsoft.Json;

using NUnit.Framework;

using RegexColumnizer;

[assembly: SupportedOSPlatform("windows")]
namespace LogExpert.RegexColumnizer.Tests;

[TestFixture]
public class RegexColumnizerConfigSerializationTests
{
    [Test]
    public void SerializeToJson_ProducesValidJson ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Name = "Test Config",
            Expression = @"^(?<time>\d+)\s+(?<message>.*)$"
        };

        // Act
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);

        // Assert
        Assert.That(json, Does.Contain("Test Config"));
        // JSON escapes backslashes, so \d becomes \\d
        Assert.That(json, Does.Contain(@"^(?<time>\\d+)\\s+(?<message>.*)$"));

        // Verify it's valid JSON
        var deserialized = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);
        Assert.That(deserialized, Is.Not.Null);
    }

    [Test]
    public void DeserializeFromJson_RestoresConfiguration ()
    {
        // Arrange
        string json = @"{
  ""Name"": ""Apache Log"",
  ""Expression"": ""^(?<ip>\\S+)\\s+""
}";

        // Act
        var config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Name, Is.EqualTo("Apache Log"));
        Assert.That(config.Expression, Is.EqualTo(@"^(?<ip>\S+)\s+"));
    }

    [Test]
    public void DeserializeFromJson_EmptyObject_UsesDefaults ()
    {
        // Arrange
        string json = "{}";

        // Act
        var config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Expression, Is.EqualTo(@"(?<text>.*)")); // Default expression
    }

    [Test]
    public void DeserializeFromJson_InvalidJson_ThrowsException ()
    {
        // Arrange
        string invalidJson = "{ invalid json }";

        // Act & Assert
        _ = Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<RegexColumnizerConfig>(invalidJson));
    }

    [Test]
    public void DeserializeFromJson_Null_ReturnsNull ()
    {
        // Arrange
        string json = "null";

        // Act
        var config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(config, Is.Null);
    }

    [Test]
    public void RoundTrip_PreservesAllProperties ()
    {
        // Arrange
        var original = new RegexColumnizerConfig
        {
            Name = "Round Trip Test",
            Expression = @"^(?<col1>\w+)\s+(?<col2>\w+)$"
        };

        // Act
        string json = JsonConvert.SerializeObject(original);
        var restored = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(restored.Name, Is.EqualTo(original.Name));
        Assert.That(restored.Expression, Is.EqualTo(original.Expression));
    }

    [Test]
    public void SerializeToJson_WithIndentation_ProducesReadableOutput ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Name = "Formatted Test",
            Expression = @"(?<text>.*)"
        };

        // Act
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);

        // Assert
        Assert.That(json, Does.Contain("\n")); // Should have newlines
        Assert.That(json, Does.Contain("  ")); // Should have indentation
    }

    [Test]
    public void DeserializeFromJson_MissingName_AllowsNull ()
    {
        // Arrange
        string json = @"{ ""Expression"": ""(?<text>.*)""  }";

        // Act
        var config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Name, Is.Null);
        Assert.That(config.Expression, Is.EqualTo(@"(?<text>.*)"));
    }

    [Test]
    public void DeserializeFromJson_MissingExpression_UsesDefault ()
    {
        // Arrange
        string json = @"{ ""Name"": ""Test"" }";

        // Act
        var config = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Name, Is.EqualTo("Test"));
        Assert.That(config.Expression, Is.EqualTo(@"(?<text>.*)")); // Default value
    }

    [Test]
    public void SerializeToJson_SpecialCharacters_EscapedProperly ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Name = "Test \"Quotes\" and \\Backslashes\\",
            Expression = @"\\d+\s+\w+"
        };

        // Act
        string json = JsonConvert.SerializeObject(config);
        var restored = JsonConvert.DeserializeObject<RegexColumnizerConfig>(json);

        // Assert
        Assert.That(restored.Name, Is.EqualTo(config.Name));
        Assert.That(restored.Expression, Is.EqualTo(config.Expression));
    }
}
