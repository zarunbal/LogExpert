# Priority 3: Architectural Improvements Implementation Guide

## Overview

**Timeline:** Weeks 7-9  
**Risk Level:** ?? LOW  
**Effort:** High  
**Impact:** MEDIUM  

This guide focuses on architectural improvements that enhance maintainability, testability, and extensibility of the plugin system.

---

## Prerequisites

? **Priority 1 & 2 must be completed**

- [ ] Hash verification working
- [ ] Plugin trust UI operational
- [ ] Progress reporting functional
- [ ] Error messages user-friendly
- [ ] All P1 & P2 tests passing

---

## Week 7: Separate Plugin Loading from Validation

### Task 3.1: Extract Interfaces

**Estimated Time:** 2 days

#### Create Core Interfaces

**File:** `src/PluginRegistry/Interfaces/IPluginLoader.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;

namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Responsible for loading plugin assemblies.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Loads a plugin from the specified path.
    /// </summary>
    PluginLoadResult LoadPlugin(string assemblyPath);
    
    /// <summary>
    /// Loads a plugin asynchronously.
    /// </summary>
    Task<PluginLoadResult> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken);
}

/// <summary>
/// Result of a plugin load operation.
/// </summary>
public class PluginLoadResult
{
    public bool Success { get; set; }
    public object? Plugin { get; set; }
    public PluginManifest? Manifest { get; set; }
    public string? ErrorMessage { get; set; }
    public System.Exception? Exception { get; set; }
}
```

**File:** `src/PluginRegistry/Interfaces/IPluginValidator.cs`

```csharp
namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Responsible for validating plugins before loading.
/// </summary>
public interface IPluginValidator
{
    /// <summary>
    /// Validates a plugin at the specified path.
    /// </summary>
    ValidationResult ValidatePlugin(string pluginPath, PluginManifest? manifest);
}

/// <summary>
/// Result of plugin validation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? UserFriendlyError { get; set; }
}
```

---

### Task 3.2: Implement Default Loader

**File:** `src/PluginRegistry/DefaultPluginLoader.cs`

```csharp
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LogExpert.PluginRegistry.Interfaces;
using NLog;

namespace LogExpert.PluginRegistry;

public class DefaultPluginLoader : IPluginLoader
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public PluginLoadResult LoadPlugin(string assemblyPath)
    {
        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // Load manifest if available
            var manifestPath = Path.ChangeExtension(assemblyPath, ".manifest.json");
            var manifest = File.Exists(manifestPath) 
                ? PluginManifest.Load(manifestPath) 
                : null;
            
            // Find plugin types
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(ILogLineColumnizer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
            
            if (pluginTypes.Count == 0)
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = "No plugin types found in assembly"
                };
            }
            
            // Instantiate first plugin type
            var pluginType = pluginTypes.First();
            var plugin = Activator.CreateInstance(pluginType);
            
            return new PluginLoadResult
            {
                Success = true,
                Plugin = plugin,
                Manifest = manifest
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load plugin: {Path}", assemblyPath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Exception = ex
            };
        }
    }

    public async Task<PluginLoadResult> LoadPluginAsync(string assemblyPath, CancellationToken cancellationToken)
    {
        return await Task.Run(() => LoadPlugin(assemblyPath), cancellationToken);
    }
}
```

---

## Week 8: Plugin Lifecycle Management

### Task 3.3: Create Lifecycle Interface

**File:** `src/ColumnizerLib/IPluginLifecycle.cs`

```csharp
namespace LogExpert;

/// <summary>
/// Defines lifecycle events for plugins.
/// </summary>
public interface IPluginLifecycle
{
    /// <summary>
    /// Called when the plugin is first loaded.
    /// </summary>
    void Initialize(IPluginContext context);
    
    /// <summary>
    /// Called when the application is shutting down.
    /// </summary>
    void Shutdown();
    
    /// <summary>
    /// Called when the plugin should reload its configuration.
    /// </summary>
    void Reload();
}

/// <summary>
/// Provides context information to plugins.
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// Logger for the plugin to use.
    /// </summary>
    ILogExpertLogger Logger { get; }
    
    /// <summary>
    /// Directory where the plugin is located.
    /// </summary>
    string PluginDirectory { get; }
    
    /// <summary>
    /// Version of the host application.
    /// </summary>
    Version HostVersion { get; }
    
    /// <summary>
    /// Configuration directory for the plugin.
    /// </summary>
    string ConfigurationDirectory { get; }
}
```

**File:** `src/PluginRegistry/PluginContext.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry;

public class PluginContext : IPluginContext
{
    public ILogExpertLogger Logger { get; init; }
    public string PluginDirectory { get; init; }
    public Version HostVersion { get; init; }
    public string ConfigurationDirectory { get; init; }
}
```

---

## Week 9: Plugin Event System

### Task 3.4: Create Event Bus

**File:** `src/PluginRegistry/Interfaces/IPluginEventBus.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry.Interfaces;

/// <summary>
/// Provides pub/sub event system for plugins.
/// </summary>
public interface IPluginEventBus
{
    /// <summary>
    /// Subscribe to an event type.
    /// </summary>
    void Subscribe<TEvent>(string pluginName, Action<TEvent> handler) where TEvent : IPluginEvent;
    
    /// <summary>
    /// Unsubscribe from an event type.
    /// </summary>
    void Unsubscribe<TEvent>(string pluginName) where TEvent : IPluginEvent;
    
    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    void Publish<TEvent>(TEvent pluginEvent) where TEvent : IPluginEvent;
}

/// <summary>
/// Base interface for plugin events.
/// </summary>
public interface IPluginEvent
{
    DateTime Timestamp { get; }
    string Source { get; }
}
```

**File:** `src/PluginRegistry/PluginEventBus.cs`

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LogExpert.PluginRegistry.Interfaces;
using NLog;

namespace LogExpert.PluginRegistry;

public class PluginEventBus : IPluginEventBus
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<Type, List<Subscription>> _subscriptions = new();

    public void Subscribe<TEvent>(string pluginName, Action<TEvent> handler) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        var subscription = new Subscription
        {
            PluginName = pluginName,
            Handler = (obj) => handler((TEvent)obj)
        };

        _subscriptions.AddOrUpdate(
            eventType,
            new List<Subscription> { subscription },
            (key, list) =>
            {
                list.Add(subscription);
                return list;
            });

        _logger.Debug("Plugin {Plugin} subscribed to {Event}", pluginName, eventType.Name);
    }

    public void Unsubscribe<TEvent>(string pluginName) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        
        if (_subscriptions.TryGetValue(eventType, out var subscriptions))
        {
            subscriptions.RemoveAll(s => s.PluginName == pluginName);
            _logger.Debug("Plugin {Plugin} unsubscribed from {Event}", pluginName, eventType.Name);
        }
    }

    public void Publish<TEvent>(TEvent pluginEvent) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        
        if (_subscriptions.TryGetValue(eventType, out var subscriptions))
        {
            foreach (var subscription in subscriptions)
            {
                try
                {
                    subscription.Handler(pluginEvent);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error handling event {Event} in plugin {Plugin}",
                        eventType.Name, subscription.PluginName);
                }
            }
        }
    }

    private class Subscription
    {
        public string PluginName { get; set; }
        public Action<object> Handler { get; set; }
    }
}
```

**Define Common Events:**

**File:** `src/PluginRegistry/Events/LogFileLoadedEvent.cs`

```csharp
using System;
using LogExpert.PluginRegistry.Interfaces;

namespace LogExpert.PluginRegistry.Events;

public class LogFileLoadedEvent : IPluginEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Source { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
}
```

---

## Testing Checklist

### Unit Tests

```csharp
[TestFixture]
public class ArchitecturalTests
{
    [Test]
    public void PluginLoader_LoadsPlugin_Successfully()
    {
        var loader = new DefaultPluginLoader();
        var result = loader.LoadPlugin("path/to/plugin.dll");
        Assert.IsTrue(result.Success);
    }
    
    [Test]
    public void EventBus_PublishSubscribe_Works()
    {
        var bus = new PluginEventBus();
        var received = false;
        
        bus.Subscribe<LogFileLoadedEvent>("TestPlugin", e => received = true);
        bus.Publish(new LogFileLoadedEvent { FileName = "test.log" });
        
        Assert.IsTrue(received);
    }
}
```

---

## Completion Criteria

- [ ] Interfaces defined and documented
- [ ] Default implementations working
- [ ] Lifecycle events fire correctly
- [ ] Event bus pub/sub works
- [ ] Existing functionality not broken
- [ ] Tests cover new architecture

---

## Next Steps

After completing Priority 3:
? Proceed to `PRIORITY_4_IMPLEMENTATION_GUIDE.md`
