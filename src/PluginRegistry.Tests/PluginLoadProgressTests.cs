using NUnit.Framework;
using LogExpert.PluginRegistry;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class PluginLoadProgressTests
{
    [Test]
    public void PluginLoadProgressEventArgs_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var pluginPath = @"C:\Plugins\TestPlugin.dll";
        var pluginName = "TestPlugin.dll";
        var currentIndex = 5;
        var totalPlugins = 10;
        var status = PluginLoadStatus.Loading;
        var message = "Test message";

        // Act
        var args = new PluginLoadProgressEventArgs(
            pluginPath,
            pluginName,
            currentIndex,
            totalPlugins,
            status,
            message);

        // Assert
        Assert.That(args.PluginPath, Is.EqualTo(pluginPath));
        Assert.That(args.PluginName, Is.EqualTo(pluginName));
        Assert.That(args.CurrentIndex, Is.EqualTo(currentIndex));
        Assert.That(args.TotalPlugins, Is.EqualTo(totalPlugins));
        Assert.That(args.Status, Is.EqualTo(status));
        Assert.That(args.Message, Is.EqualTo(message));
        Assert.That(args.Timestamp, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void PluginLoadProgressEventArgs_PercentComplete_CalculatesCorrectly()
    {
        // Arrange & Act
        var args1 = new PluginLoadProgressEventArgs("path", "name", 0, 10, PluginLoadStatus.Started);
        var args2 = new PluginLoadProgressEventArgs("path", "name", 4, 10, PluginLoadStatus.Loading);
        var args3 = new PluginLoadProgressEventArgs("path", "name", 9, 10, PluginLoadStatus.Loaded);

        // Assert
        Assert.That(args1.PercentComplete, Is.EqualTo(10.0).Within(0.01)); // (0+1)/10 * 100 = 10%
        Assert.That(args2.PercentComplete, Is.EqualTo(50.0).Within(0.01)); // (4+1)/10 * 100 = 50%
        Assert.That(args3.PercentComplete, Is.EqualTo(100.0).Within(0.01)); // (9+1)/10 * 100 = 100%
    }

    [Test]
    public void PluginLoadProgressEventArgs_PercentComplete_ZeroTotalReturnsZero()
    {
        // Arrange & Act
        var args = new PluginLoadProgressEventArgs("path", "name", 0, 0, PluginLoadStatus.Started);

        // Assert
        Assert.That(args.PercentComplete, Is.EqualTo(0.0));
    }

    [Test]
    public void PluginLoadProgressEventArgs_ToString_ReturnsFormattedString()
    {
        // Arrange
        var args = new PluginLoadProgressEventArgs(
            @"C:\Plugins\TestPlugin.dll",
            "TestPlugin.dll",
            2,
            5,
            PluginLoadStatus.Loading,
            "Loading plugin assembly");

        // Act
        var result = args.ToString();

        // Assert
        Assert.That(result, Does.Contain("[3/5]")); // currentIndex + 1
        Assert.That(result, Does.Contain("Loading"));
        Assert.That(result, Does.Contain("TestPlugin.dll"));
        Assert.That(result, Does.Contain("Loading plugin assembly"));
    }

    [Test]
    public void PluginLoadProgressEventArgs_NullMessage_HandledGracefully()
    {
        // Arrange & Act
        var args = new PluginLoadProgressEventArgs(
            @"C:\Plugins\TestPlugin.dll",
            "TestPlugin.dll",
            0,
            1,
            PluginLoadStatus.Started,
            null);

        // Assert
        Assert.That(args.Message, Is.Null);
        var result = args.ToString();
        Assert.That(result, Does.Contain("(no details)"));
    }

    [Test]
    public void PluginLoadStatus_AllValuesAreDefined()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Started), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Validating), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Validated), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Loading), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Loaded), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Skipped), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Failed), Is.True);
        Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), PluginLoadStatus.Completed), Is.True);
    }

    [Test]
    public void PluginLoadProgress_MultiplePlugins_CalculatesProgressCorrectly()
    {
        // Arrange
        var totalPlugins = 20;

        // Act & Assert - Calculate progress for each plugin
        for (int i = 0; i < totalPlugins; i++)
        {
            var args = new PluginLoadProgressEventArgs(
                $"Plugin{i}.dll",
                $"Plugin{i}.dll",
                i,
                totalPlugins,
                PluginLoadStatus.Loading);

            var expectedPercent = ((double)(i + 1) / totalPlugins) * 100;
            Assert.That(args.PercentComplete, Is.EqualTo(expectedPercent).Within(0.01));
        }
    }

    [Test]
    public void PluginLoadProgress_EventArgs_TimestampIsRecent()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var args = new PluginLoadProgressEventArgs(
            "path",
            "name",
            0,
            1,
            PluginLoadStatus.Started);

        var after = DateTime.UtcNow;

        // Assert
        Assert.That(args.Timestamp, Is.GreaterThanOrEqualTo(before));
        Assert.That(args.Timestamp, Is.LessThanOrEqualTo(after));
    }

    [Test]
    public void PluginLoadProgress_StatusFlow_IsLogical()
    {
        // This test documents the expected status flow
        var expectedFlow = new[]
        {
            PluginLoadStatus.Started,      // Plugin loading begins
            PluginLoadStatus.Validating,   // Checking security
            PluginLoadStatus.Validated,    // Security OK
            PluginLoadStatus.Loading,      // Loading into memory
            PluginLoadStatus.Loaded,       // Successfully loaded
            PluginLoadStatus.Completed     // All plugins done
        };

        // Assert all statuses are in the enum
        foreach (var status in expectedFlow)
        {
            Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), status), Is.True);
        }
    }

    [Test]
    public void PluginLoadProgress_AlternateStatusFlow_SkippedScenario()
    {
        // Document alternate flow when plugin is skipped
        var skippedFlow = new[]
        {
            PluginLoadStatus.Started,
            PluginLoadStatus.Validating,
            PluginLoadStatus.Skipped      // Failed validation
        };

        foreach (var status in skippedFlow)
        {
            Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), status), Is.True);
        }
    }

    [Test]
    public void PluginLoadProgress_AlternateStatusFlow_FailedScenario()
    {
        // Document alternate flow when plugin fails to load
        var failedFlow = new[]
        {
            PluginLoadStatus.Started,
            PluginLoadStatus.Validating,
            PluginLoadStatus.Validated,
            PluginLoadStatus.Loading,
            PluginLoadStatus.Failed       // Load error
        };

        foreach (var status in failedFlow)
        {
            Assert.That(Enum.IsDefined(typeof(PluginLoadStatus), status), Is.True);
        }
    }
}
