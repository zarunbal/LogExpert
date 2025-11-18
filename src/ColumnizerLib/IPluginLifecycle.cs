namespace LogExpert;

/// <summary>
/// Defines lifecycle events for plugins.
/// Plugins can optionally implement this interface to receive lifecycle notifications.
/// </summary>
public interface IPluginLifecycle
{
    /// <summary>
    /// Called when the plugin is first loaded.
    /// Use this to initialize resources, load configuration, etc.
    /// </summary>
    /// <param name="context">Context providing information about the host environment</param>
    void Initialize (IPluginContext context);

    /// <summary>
    /// Called when the application is shutting down.
    /// Use this to cleanup resources, save state, etc.
    /// </summary>
    void Shutdown ();

    /// <summary>
    /// Called when the plugin should reload its configuration.
    /// Use this to refresh settings without restarting the application.
    /// </summary>
    void Reload ();
}