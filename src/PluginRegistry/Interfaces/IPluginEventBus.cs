namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Provides pub/sub event system for plugins.
/// Allows plugins to communicate without direct dependencies.
/// </summary>
public interface IPluginEventBus
{
    /// <summary>
    /// Subscribe to an event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to subscribe to</typeparam>
    /// <param name="pluginName">Name of the subscribing plugin</param>
    /// <param name="handler">Handler to call when event is published</param>
    void Subscribe<TEvent>(string pluginName, Action<TEvent> handler) where TEvent : IPluginEvent;
    
    /// <summary>
    /// Unsubscribe from an event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to unsubscribe from</typeparam>
    /// <param name="pluginName">Name of the plugin to unsubscribe</param>
    void Unsubscribe<TEvent>(string pluginName) where TEvent : IPluginEvent;
    
    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to publish</typeparam>
    /// <param name="pluginEvent">Event instance to publish</param>
    void Publish<TEvent>(TEvent pluginEvent) where TEvent : IPluginEvent;
    
    /// <summary>
    /// Unsubscribe a plugin from all events.
    /// </summary>
    /// <param name="pluginName">Name of the plugin to unsubscribe</param>
    void UnsubscribeAll(string pluginName);
}

/// <summary>
/// Base interface for plugin events.
/// All events must implement this interface.
/// </summary>
public interface IPluginEvent
{
    /// <summary>
    /// When the event was created.
    /// </summary>
    DateTime Timestamp { get; }
    
    /// <summary>
    /// Source of the event (plugin name or "LogExpert").
    /// </summary>
    string Source { get; }
}
