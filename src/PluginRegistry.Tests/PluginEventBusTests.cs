using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using LogExpert.PluginRegistry;
using LogExpert.PluginRegistry.Interfaces;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Tests for PluginEventBus functionality.
/// Tests event subscription, publishing, unsubscription, and thread safety.
/// </summary>
[TestFixture]
public class PluginEventBusTests
{
    private PluginEventBus _eventBus = null!;

    [SetUp]
    public void SetUp()
    {
        _eventBus = new PluginEventBus();
    }

    #region Test Event Classes

    private class TestEvent : IPluginEvent
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Source { get; init; } = "TestPlugin";
        public string Message { get; init; } = "";
    }

    private class AnotherTestEvent : IPluginEvent
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Source { get; init; } = "TestPlugin";
        public int Value { get; init; }
    }

    #endregion

    #region Subscription Tests

    [Test]
    public void Subscribe_WithValidPluginAndHandler_ShouldNotThrow()
    {
        // Arrange
        var pluginName = "TestPlugin";
        Action<TestEvent> handler = e => { };

        // Act & Assert
        Assert.DoesNotThrow(() => _eventBus.Subscribe(pluginName, handler));
    }

    [Test]
    public void Subscribe_WithNullPluginName_ShouldThrowArgumentNullException()
    {
        // Arrange
        Action<TestEvent> handler = e => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.Subscribe<TestEvent>(null!, handler));
    }

    [Test]
    public void Subscribe_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.Subscribe<TestEvent>(pluginName, null!));
    }

    [Test]
    public void Subscribe_MultiplePluginsToSameEvent_ShouldAllowBoth()
    {
        // Arrange
        var plugin1 = "Plugin1";
        var plugin2 = "Plugin2";
        Action<TestEvent> handler1 = e => { };
        Action<TestEvent> handler2 = e => { };

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _eventBus.Subscribe(plugin1, handler1);
            _eventBus.Subscribe(plugin2, handler2);
        });
    }

    [Test]
    public void Subscribe_SamePluginToDifferentEvents_ShouldAllowBoth()
    {
        // Arrange
        var pluginName = "TestPlugin";
        Action<TestEvent> handler1 = e => { };
        Action<AnotherTestEvent> handler2 = e => { };

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _eventBus.Subscribe(pluginName, handler1);
            _eventBus.Subscribe(pluginName, handler2);
        });
    }

    [Test]
    public void Subscribe_SamePluginAndEventMultipleTimes_ShouldAllowMultipleSubscriptions()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var callCount = 0;
        Action<TestEvent> handler = e => callCount++;

        // Act
        _eventBus.Subscribe(pluginName, handler);
        _eventBus.Subscribe(pluginName, handler);
        _eventBus.Publish(new TestEvent { Message = "Test" });

        // Assert
        Assert.That(callCount, Is.EqualTo(2), "Both subscriptions should be called");
    }

    #endregion

    #region Publishing Tests

    [Test]
    public void Publish_WithSubscriber_ShouldNotifySubscriber()
    {
        // Arrange
        var pluginName = "TestPlugin";
        TestEvent? receivedEvent = null;
        _eventBus.Subscribe<TestEvent>(pluginName, e => receivedEvent = e);

        var testEvent = new TestEvent { Message = "Test Message", Source = "TestSource" };

        // Act
        _eventBus.Publish(testEvent);

        // Assert
        Assert.That(receivedEvent, Is.Not.Null, "Event should be received");
        Assert.That(receivedEvent!.Message, Is.EqualTo("Test Message"));
        Assert.That(receivedEvent.Source, Is.EqualTo("TestSource"));
    }

    [Test]
    public void Publish_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };

        // Act & Assert
        Assert.DoesNotThrow(() => _eventBus.Publish(testEvent));
    }

    [Test]
    public void Publish_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.Publish<TestEvent>(null!));
    }

    [Test]
    public void Publish_WithMultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var receivedCount = 0;
        _eventBus.Subscribe<TestEvent>("Plugin1", e => receivedCount++);
        _eventBus.Subscribe<TestEvent>("Plugin2", e => receivedCount++);
        _eventBus.Subscribe<TestEvent>("Plugin3", e => receivedCount++);

        var testEvent = new TestEvent { Message = "Test" };

        // Act
        _eventBus.Publish(testEvent);

        // Assert
        Assert.That(receivedCount, Is.EqualTo(3), "All subscribers should be notified");
    }

    [Test]
    public void Publish_OnlyNotifiesSubscribersOfMatchingEventType()
    {
        // Arrange
        var testEventCount = 0;
        var anotherEventCount = 0;

        _eventBus.Subscribe<TestEvent>("Plugin1", e => testEventCount++);
        _eventBus.Subscribe<AnotherTestEvent>("Plugin1", e => anotherEventCount++);

        // Act
        _eventBus.Publish(new TestEvent { Message = "Test" });

        // Assert
        Assert.That(testEventCount, Is.EqualTo(1), "TestEvent subscriber should be notified");
        Assert.That(anotherEventCount, Is.EqualTo(0), "AnotherTestEvent subscriber should NOT be notified");
    }

    [Test]
    public void Publish_WhenHandlerThrows_ShouldNotifyOtherSubscribers()
    {
        // Arrange
        var callCount = 0;
        _eventBus.Subscribe<TestEvent>("Plugin1", e => throw new InvalidOperationException("Test exception"));
        _eventBus.Subscribe<TestEvent>("Plugin2", e => callCount++);
        _eventBus.Subscribe<TestEvent>("Plugin3", e => callCount++);

        var testEvent = new TestEvent { Message = "Test" };

        // Act
        _eventBus.Publish(testEvent);

        // Assert
        Assert.That(callCount, Is.EqualTo(2), "Other subscribers should still be notified despite exception");
    }

    [Test]
    public void Publish_PreservesEventData()
    {
        // Arrange
        TestEvent? receivedEvent = null;
        _eventBus.Subscribe<TestEvent>("TestPlugin", e => receivedEvent = e);

        var originalEvent = new TestEvent
        {
            Message = "Original Message",
            Source = "Original Source",
            Timestamp = new DateTime(2025, 11, 19, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        _eventBus.Publish(originalEvent);

        // Assert
        Assert.That(receivedEvent, Is.Not.Null);
        Assert.That(receivedEvent!.Message, Is.EqualTo(originalEvent.Message));
        Assert.That(receivedEvent.Source, Is.EqualTo(originalEvent.Source));
        Assert.That(receivedEvent.Timestamp, Is.EqualTo(originalEvent.Timestamp));
    }

    #endregion

    #region Unsubscription Tests

    [Test]
    public void Unsubscribe_WithNullPluginName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.Unsubscribe<TestEvent>(null!));
    }

    [Test]
    public void Unsubscribe_AfterSubscribing_ShouldStopReceivingEvents()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var callCount = 0;
        _eventBus.Subscribe<TestEvent>(pluginName, e => callCount++);

        // Act
        _eventBus.Publish(new TestEvent { Message = "Before unsubscribe" });
        _eventBus.Unsubscribe<TestEvent>(pluginName);
        _eventBus.Publish(new TestEvent { Message = "After unsubscribe" });

        // Assert
        Assert.That(callCount, Is.EqualTo(1), "Should only receive event before unsubscribe");
    }

    [Test]
    public void Unsubscribe_WhenNotSubscribed_ShouldNotThrow()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act & Assert
        Assert.DoesNotThrow(() => _eventBus.Unsubscribe<TestEvent>(pluginName));
    }

    [Test]
    public void Unsubscribe_OnlyUnsubscribesSpecifiedEventType()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var testEventCount = 0;
        var anotherEventCount = 0;

        _eventBus.Subscribe<TestEvent>(pluginName, e => testEventCount++);
        _eventBus.Subscribe<AnotherTestEvent>(pluginName, e => anotherEventCount++);

        // Act
        _eventBus.Unsubscribe<TestEvent>(pluginName);
        _eventBus.Publish(new TestEvent { Message = "Test" });
        _eventBus.Publish(new AnotherTestEvent { Value = 42 });

        // Assert
        Assert.That(testEventCount, Is.EqualTo(0), "TestEvent subscription should be removed");
        Assert.That(anotherEventCount, Is.EqualTo(1), "AnotherTestEvent subscription should remain");
    }

    [Test]
    public void Unsubscribe_OnlyUnsubscribesSpecifiedPlugin()
    {
        // Arrange
        var plugin1Count = 0;
        var plugin2Count = 0;

        _eventBus.Subscribe<TestEvent>("Plugin1", e => plugin1Count++);
        _eventBus.Subscribe<TestEvent>("Plugin2", e => plugin2Count++);

        // Act
        _eventBus.Unsubscribe<TestEvent>("Plugin1");
        _eventBus.Publish(new TestEvent { Message = "Test" });

        // Assert
        Assert.That(plugin1Count, Is.EqualTo(0), "Plugin1 should be unsubscribed");
        Assert.That(plugin2Count, Is.EqualTo(1), "Plugin2 should still receive events");
    }

    #endregion

    #region UnsubscribeAll Tests

    [Test]
    public void UnsubscribeAll_WithNullPluginName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.UnsubscribeAll(null!));
    }

    [Test]
    public void UnsubscribeAll_ShouldUnsubscribeFromAllEvents()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var testEventCount = 0;
        var anotherEventCount = 0;

        _eventBus.Subscribe<TestEvent>(pluginName, e => testEventCount++);
        _eventBus.Subscribe<AnotherTestEvent>(pluginName, e => anotherEventCount++);

        // Act
        _eventBus.UnsubscribeAll(pluginName);
        _eventBus.Publish(new TestEvent { Message = "Test" });
        _eventBus.Publish(new AnotherTestEvent { Value = 42 });

        // Assert
        Assert.That(testEventCount, Is.EqualTo(0), "Should not receive TestEvent after UnsubscribeAll");
        Assert.That(anotherEventCount, Is.EqualTo(0), "Should not receive AnotherTestEvent after UnsubscribeAll");
    }

    [Test]
    public void UnsubscribeAll_OnlyAffectsSpecifiedPlugin()
    {
        // Arrange
        var plugin1Count = 0;
        var plugin2Count = 0;

        _eventBus.Subscribe<TestEvent>("Plugin1", e => plugin1Count++);
        _eventBus.Subscribe<TestEvent>("Plugin2", e => plugin2Count++);

        // Act
        _eventBus.UnsubscribeAll("Plugin1");
        _eventBus.Publish(new TestEvent { Message = "Test" });

        // Assert
        Assert.That(plugin1Count, Is.EqualTo(0), "Plugin1 should be unsubscribed");
        Assert.That(plugin2Count, Is.EqualTo(1), "Plugin2 should still receive events");
    }

    [Test]
    public void UnsubscribeAll_WhenNotSubscribed_ShouldNotThrow()
    {
        // Arrange
        var pluginName = "TestPlugin";

        // Act & Assert
        Assert.DoesNotThrow(() => _eventBus.UnsubscribeAll(pluginName));
    }

    [Test]
    public void UnsubscribeAll_WithMultipleSubscriptions_ShouldRemoveAll()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var totalCount = 0;

        _eventBus.Subscribe<TestEvent>(pluginName, e => totalCount++);
        _eventBus.Subscribe<TestEvent>(pluginName, e => totalCount++);
        _eventBus.Subscribe<AnotherTestEvent>(pluginName, e => totalCount++);

        // Act
        _eventBus.UnsubscribeAll(pluginName);
        _eventBus.Publish(new TestEvent { Message = "Test" });
        _eventBus.Publish(new AnotherTestEvent { Value = 42 });

        // Assert
        Assert.That(totalCount, Is.EqualTo(0), "All subscriptions should be removed");
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public void Publish_ConcurrentPublishes_ShouldBeSafe()
    {
        // Arrange
        var callCount = 0;
        var lockObj = new object();
        _eventBus.Subscribe<TestEvent>("TestPlugin", e =>
        {
            lock (lockObj)
            {
                callCount++;
            }
        });

        // Act
        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _eventBus.Publish(new TestEvent { Message = "Test" })));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.That(callCount, Is.EqualTo(10), "All events should be received");
    }

    [Test]
    public void Subscribe_ConcurrentSubscriptions_ShouldBeSafe()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (var i = 0; i < 10; i++)
        {
            var pluginName = $"Plugin{i}";
            tasks.Add(Task.Run(() => _eventBus.Subscribe<TestEvent>(pluginName, e => { })));
        }

        // Assert
        Assert.DoesNotThrow(() => Task.WaitAll(tasks.ToArray()));
    }

    [Test]
    public void SubscribeAndPublish_Concurrent_ShouldBeSafe()
    {
        // Arrange
        var callCount = 0;
        var lockObj = new object();

        // Act
        var tasks = new List<Task>();
        
        // Subscribe tasks
        for (var i = 0; i < 5; i++)
        {
            var pluginName = $"Plugin{i}";
            tasks.Add(Task.Run(() =>
            {
                _eventBus.Subscribe<TestEvent>(pluginName, e =>
                {
                    lock (lockObj)
                    {
                        callCount++;
                    }
                });
            }));
        }

        // Give subscriptions time to register
        Thread.Sleep(50);

        // Publish tasks
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() => _eventBus.Publish(new TestEvent { Message = "Test" })));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.That(callCount, Is.GreaterThan(0), "Some events should be received");
    }

    [Test]
    public void UnsubscribeDuringPublish_ShouldBeSafe()
    {
        // Arrange
        var publishCount = 0;
        var lockObj = new object();

        for (var i = 0; i < 10; i++)
        {
            var pluginName = $"Plugin{i}";
            _eventBus.Subscribe<TestEvent>(pluginName, e =>
            {
                lock (lockObj)
                {
                    publishCount++;
                }
            });
        }

        // Act
        var tasks = new List<Task>();

        // Publish task
        tasks.Add(Task.Run(() =>
        {
            for (var i = 0; i < 100; i++)
            {
                _eventBus.Publish(new TestEvent { Message = "Test" });
                Thread.Sleep(1);
            }
        }));

        // Unsubscribe tasks
        for (var i = 0; i < 10; i++)
        {
            var pluginName = $"Plugin{i}";
            tasks.Add(Task.Run(() =>
            {
                Thread.Sleep(10);
                _eventBus.Unsubscribe<TestEvent>(pluginName);
            }));
        }

        // Assert
        Assert.DoesNotThrow(() => Task.WaitAll(tasks.ToArray()));
        Assert.That(publishCount, Is.GreaterThan(0), "Some events should be received before unsubscribe");
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void Subscribe_AfterUnsubscribe_ShouldAllowResubscription()
    {
        // Arrange
        var pluginName = "TestPlugin";
        var callCount = 0;
        Action<TestEvent> handler = e => callCount++;

        // Act
        _eventBus.Subscribe(pluginName, handler);
        _eventBus.Publish(new TestEvent { Message = "Test 1" });
        
        _eventBus.Unsubscribe<TestEvent>(pluginName);
        _eventBus.Publish(new TestEvent { Message = "Test 2" });
        
        _eventBus.Subscribe(pluginName, handler);
        _eventBus.Publish(new TestEvent { Message = "Test 3" });

        // Assert
        Assert.That(callCount, Is.EqualTo(2), "Should receive first and third events only");
    }

    [Test]
    public void Publish_WithVeryLongEventData_ShouldWork()
    {
        // Arrange
        var receivedMessage = "";
        var longMessage = new string('x', 10000);
        _eventBus.Subscribe<TestEvent>("TestPlugin", e => receivedMessage = e.Message);

        // Act
        _eventBus.Publish(new TestEvent { Message = longMessage });

        // Assert
        Assert.That(receivedMessage, Is.EqualTo(longMessage));
    }

    [Test]
    public void Publish_ManyEventsQuickly_ShouldHandleAll()
    {
        // Arrange
        var callCount = 0;
        _eventBus.Subscribe<TestEvent>("TestPlugin", e => callCount++);

        // Act
        for (var i = 0; i < 1000; i++)
        {
            _eventBus.Publish(new TestEvent { Message = $"Test {i}" });
        }

        // Assert
        Assert.That(callCount, Is.EqualTo(1000), "All events should be received");
    }

    #endregion
}
