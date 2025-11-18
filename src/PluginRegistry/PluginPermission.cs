namespace LogExpert.PluginRegistry;

/// <summary>
/// Defines permissions that plugins can request and use.
/// </summary>
[Flags]
public enum PluginPermission
{
    /// <summary>
    /// No permissions.
    /// </summary>
    None = 0,

    /// <summary>
    /// Permission to read files from the file system (config, log files).
    /// </summary>
    FileSystemRead = 1 << 0,

    /// <summary>
    /// Permission to write files to the file system (config, exports).
    /// </summary>
    FileSystemWrite = 1 << 1,

    /// <summary>
    /// Permission to make network connections (HTTP, SFTP, etc.).
    /// </summary>
    NetworkConnect = 1 << 2,

    /// <summary>
    /// Permission to read application configuration.
    /// </summary>
    ConfigRead = 1 << 3,

    /// <summary>
    /// Permission to write application configuration.
    /// </summary>
    ConfigWrite = 1 << 4,

    /// <summary>
    /// Permission to read from Windows registry.
    /// </summary>
    RegistryRead = 1 << 5,

    /// <summary>
    /// All permissions (for trusted plugins).
    /// </summary>
    All = FileSystemRead | FileSystemWrite | NetworkConnect | ConfigRead | ConfigWrite | RegistryRead
}
