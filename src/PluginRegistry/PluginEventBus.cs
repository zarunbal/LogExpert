using System.Collections.Concurrent;

using LogExpert.PluginRegistry.Interfaces;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Default implementation of plugin event bus using in-memory pub/sub.
/// Thread-safe and supports multiple subscribers per event type.
/// </summary>
public class PluginEventBus : IPluginEventBus
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<Type, List<Subscription>> _subscriptions = new();
    private readonly Lock _lockObj = new();

    /// <summary>
    /// Subscribe to an event type.
    /// </summary>
    public void Subscribe<TEvent> (string pluginName, Action<TEvent> handler) where TEvent : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(pluginName);
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        var subscription = new Subscription
        {
            PluginName = pluginName,
            EventType = eventType,
            Handler = (obj) => handler((TEvent)obj)
        };

        lock (_lockObj)
        {
            _ = _subscriptions.AddOrUpdate(
                eventType,
                [subscription],
                (key, list) =>
                {
                    list.Add(subscription);
                    return list;
                });
        }

        _logger.Debug("Plugin '{Plugin}' subscribed to event '{Event}'", pluginName, eventType.Name);
    }

    /// <summary>
    /// Unsubscribe from a specific event type.
    /// </summary>
    public void Unsubscribe<TEvent> (string pluginName) where TEvent : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(pluginName);

        var eventType = typeof(TEvent);

        lock (_lockObj)
        {
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                var removed = subscriptions.RemoveAll(s => s.PluginName == pluginName);
                if (removed > 0)
                {
                    _logger.Debug("Plugin '{Plugin}' unsubscribed from event '{Event}'", pluginName, eventType.Name);
                }
            }
        }
    }

    /// <summary>
    /// Unsubscribe a plugin from all events.
    /// </summary>
    public void UnsubscribeAll (string pluginName)
    {
        ArgumentNullException.ThrowIfNull(pluginName);

        lock (_lockObj)
        {
            var removedCount = 0;
            foreach (var kvp in _subscriptions)
            {
                removedCount += kvp.Value.RemoveAll(s => s.PluginName == pluginName);
            }

            if (removedCount > 0)
            {
                _logger.Info("Plugin '{Plugin}' unsubscribed from all events ({Count} subscriptions removed)", pluginName, removedCount);
            }
        }
    }

    /// <summary>
    /// Publish an event to all subscribers.
    /// Exceptions in handlers are caught and logged to prevent one plugin from affecting others.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally Catch All")]
    public void Publish<TEvent> (TEvent pluginEvent) where TEvent : IPluginEvent
    {
        ArgumentNullException.ThrowIfNull(pluginEvent);

        var eventType = typeof(TEvent);
        List<Subscription> subscriptionsToNotify;

        lock (_lockObj)
        {
            if (!_subscriptions.TryGetValue(eventType, out var subscriptions) || subscriptions.Count == 0)
            {
                _logger.Trace("No subscribers for event '{Event}'", eventType.Name);
                return;
            }

            // Create a copy to avoid lock contention during notification
            subscriptionsToNotify = [.. subscriptions];
        }

        _logger.Debug("Publishing event '{Event}' from '{Source}' to {Count} subscriber(s)",
            eventType.Name, pluginEvent.Source, subscriptionsToNotify.Count);

        foreach (var subscription in subscriptionsToNotify)
        {
            try
            {
                subscription.Handler(pluginEvent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling event '{Event}' in plugin '{Plugin}'", eventType.Name, subscription.PluginName);
            }
        }
    }

    /// <summary>
    /// Internal subscription record.
    /// </summary>
    private class Subscription
    {
        public string PluginName { get; set; }
        public Type EventType { get; set; }
        public Action<object> Handler { get; set; }
    }
}
