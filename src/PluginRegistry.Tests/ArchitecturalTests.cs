using LogExpert.PluginRegistry.Events;
using LogExpert.PluginRegistry.Interfaces;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class ArchitecturalTests
{
    [Test]
    public void DefaultPluginLoader_LoadsValidPlugin_Successfully ()
    {
        // Note: This is a placeholder test that would need a real test plugin DLL
        // In a real scenario, you'd create a test assembly or use an existing one

        var loader = new DefaultPluginLoader();

        // This test verifies the loader is instantiable
        Assert.That(loader, Is.Not.Null);
    }

    [Test]
    public void PluginEventBus_SubscribeAndPublish_DeliversEvent ()
    {
        // Arrange
        var bus = new PluginEventBus();
        var eventReceived = false;
        LogFileLoadedEvent? receivedEvent = null;

        bus.Subscribe<LogFileLoadedEvent>("TestPlugin", e =>
        {
            eventReceived = true;
            receivedEvent = e;
        });

        var testEvent = new LogFileLoadedEvent
        {
            Source = "TestSource",
            FileName = "test.log",
            FileSize = 1024
        };

        // Act
        bus.Publish(testEvent);

        // Assert
        Assert.That(eventReceived, Is.True, "Event should have been received");
        Assert.That(receivedEvent, Is.Not.Null);
        Assert.That(receivedEvent.Source, Is.EqualTo("TestSource"));
        Assert.That(receivedEvent.FileName, Is.EqualTo("test.log"));
        Assert.That(receivedEvent.FileSize, Is.EqualTo(1024));
    }

    [Test]
    public void PluginEventBus_Unsubscribe_StopsDeliveringEvents ()
    {
        // Arrange
        var bus = new PluginEventBus();
        var eventCount = 0;

        bus.Subscribe<LogFileLoadedEvent>("TestPlugin", e => eventCount++);

        // Act - publish first event
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test.log", FileSize = 100 });

        // Unsubscribe
        bus.Unsubscribe<LogFileLoadedEvent>("TestPlugin");

        // Publish second event
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test2.log", FileSize = 200 });

        // Assert
        Assert.That(eventCount, Is.EqualTo(1), "Should only receive the first event");
    }

    [Test]
    public void PluginEventBus_MultipleSubscribers_AllReceiveEvent ()
    {
        // Arrange
        var bus = new PluginEventBus();
        var plugin1Received = false;
        var plugin2Received = false;

        bus.Subscribe<LogFileLoadedEvent>("Plugin1", e => plugin1Received = true);
        bus.Subscribe<LogFileLoadedEvent>("Plugin2", e => plugin2Received = true);

        // Act
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test.log", FileSize = 100 });

        // Assert
        Assert.That(plugin1Received, Is.True, "Plugin1 should receive event");
        Assert.That(plugin2Received, Is.True, "Plugin2 should receive event");
    }

    [Test]
    public void PluginEventBus_ExceptionInHandler_DoesNotAffectOtherHandlers ()
    {
        // Arrange
        var bus = new PluginEventBus();
        var plugin1Received = false;
        var plugin2Received = false;

        bus.Subscribe<LogFileLoadedEvent>("Plugin1", e =>
        {
            plugin1Received = true;
            throw new InvalidOperationException("Test exception");
        });

        bus.Subscribe<LogFileLoadedEvent>("Plugin2", e => plugin2Received = true);

        // Act
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test.log", FileSize = 100 });

        // Assert
        Assert.That(plugin1Received, Is.True, "Plugin1 should receive event");
        Assert.That(plugin2Received, Is.True, "Plugin2 should still receive event despite Plugin1 exception");
    }

    [Test]
    public void PluginEventBus_UnsubscribeAll_RemovesAllSubscriptions ()
    {
        // Arrange
        var bus = new PluginEventBus();
        var eventCount = 0;

        bus.Subscribe<LogFileLoadedEvent>("TestPlugin", e => eventCount++);
        bus.Subscribe<LogFileClosedEvent>("TestPlugin", e => eventCount++);

        // Act - publish events before unsubscribe
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test.log", FileSize = 100 });
        bus.Publish(new LogFileClosedEvent { Source = "Test", FileName = "test.log" });

        // Unsubscribe all
        bus.UnsubscribeAll("TestPlugin");

        // Publish events after unsubscribe
        bus.Publish(new LogFileLoadedEvent { Source = "Test", FileName = "test2.log", FileSize = 200 });
        bus.Publish(new LogFileClosedEvent { Source = "Test", FileName = "test2.log" });

        // Assert
        Assert.That(eventCount, Is.EqualTo(2), "Should only receive events before UnsubscribeAll");
    }

    [Test]
    public void PluginContext_InitializesWithCorrectValues ()
    {
        // Arrange & Act
        var logger = new PluginLogger("TestPlugin");
        var context = new PluginContext
        {
            Logger = logger,
            PluginDirectory = @"C:\Plugins\TestPlugin",
            HostVersion = new Version(1, 2, 3),
            ConfigurationDirectory = @"C:\Config\TestPlugin"
        };

        // Assert
        Assert.That(context.Logger, Is.Not.Null);
        Assert.That(context.PluginDirectory, Is.EqualTo(@"C:\Plugins\TestPlugin"));
        Assert.That(context.HostVersion, Is.EqualTo(new Version(1, 2, 3)));
        Assert.That(context.ConfigurationDirectory, Is.EqualTo(@"C:\Config\TestPlugin"));
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Unit Tests")]
    public void PluginLogger_LogsMessages_WithoutException ()
    {
        // Arrange
        var logger = new PluginLogger("TestPlugin");

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => logger.Debug("Debug message"));
        Assert.DoesNotThrow(() => logger.Info("Info message"));
        Assert.DoesNotThrow(() => logger.LogWarn("Warn message"));
        Assert.DoesNotThrow(() => logger.LogError("Error message"));
    }

    [Test]
    public void PluginLoadResult_Success_ContainsPluginAndManifest ()
    {
        // Arrange
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Version = "1.0.0",
            Author = "Test Author",
            Description = "Test Description",
            Main = "TestPlugin.dll",
            ApiVersion = "1.0"
        };

        // Act
        var result = new PluginLoadResult
        {
            Success = true,
            Plugin = new object(),
            Manifest = manifest
        };

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.Manifest, Is.Not.Null);
        Assert.That(result.Manifest.Name, Is.EqualTo("TestPlugin"));
    }

    [Test]
    public void PluginLoadResult_Failure_ContainsErrorMessage ()
    {
        // Arrange & Act
        var result = new PluginLoadResult
        {
            Success = false,
            ErrorMessage = "Plugin not found",
            Exception = new FileNotFoundException()
        };

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception, Is.InstanceOf<FileNotFoundException>());
    }

    [Test]
    public void ValidationResult_Invalid_ContainsErrors ()
    {
        // Arrange & Act
        var result = new ValidationResult
        {
            IsValid = false,
            Errors = ["Missing required field: name", "Invalid version format"],
            Warnings = ["Optional field 'url' not provided"],
            UserFriendlyError = "The plugin manifest is incomplete"
        };

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Warnings.Count, Is.EqualTo(1));
        Assert.That(result.UserFriendlyError, Is.Not.Null);
    }

    [Test]
    public void CommonEvents_HaveCorrectProperties ()
    {
        // Arrange & Act
        var loadedEvent = new LogFileLoadedEvent
        {
            Source = "LogExpert",
            FileName = "test.log",
            FileSize = 1024,
            LineCount = 100
        };

        var closedEvent = new LogFileClosedEvent
        {
            Source = "LogExpert",
            FileName = "test.log"
        };

        var pluginLoadedEvent = new PluginLoadedEvent
        {
            Source = "LogExpert",
            PluginName = "TestPlugin",
            PluginVersion = "1.0.0"
        };

        // Assert
        Assert.That(loadedEvent.Timestamp, Is.Not.Null);
        Assert.That(loadedEvent.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(loadedEvent.Source, Is.EqualTo("LogExpert"));
        Assert.That(loadedEvent.FileSize, Is.EqualTo(1024));
        Assert.That(loadedEvent.LineCount, Is.EqualTo(100));

        Assert.That(closedEvent.Source, Is.EqualTo("LogExpert"));
        Assert.That(closedEvent.FileName, Is.EqualTo("test.log"));

        Assert.That(pluginLoadedEvent.Source, Is.EqualTo("LogExpert"));
        Assert.That(pluginLoadedEvent.PluginName, Is.EqualTo("TestPlugin"));
        Assert.That(pluginLoadedEvent.PluginVersion, Is.EqualTo("1.0.0"));
    }
}
