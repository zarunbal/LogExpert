using NLog;

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

/// <summary>
/// Manages plugin permissions and validates permission requests.
/// </summary>
public class PluginPermissionManager
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // Plugin permission configuration (loaded from file)
    private static readonly Dictionary<string, PluginPermissionConfig> _pluginPermissions = [];

    // Default permissions for plugins without manifest (backward compatibility)
    private const PluginPermission DEFAULT_PERMISSIONS = PluginPermission.FileSystemRead | PluginPermission.ConfigRead;

    #endregion

    #region Public methods

    /// <summary>
    /// Checks if a plugin has a specific permission.
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <param name="permission">Permission to check</param>
    /// <returns>True if plugin has permission, false otherwise</returns>
    public static bool HasPermission (string pluginName, PluginPermission permission)
    {
        if (string.IsNullOrWhiteSpace(pluginName))
        {
            _logger.Warn("HasPermission called with null/empty plugin name");
            return false;
        }

        // Check if plugin has explicit permission configuration
        if (_pluginPermissions.TryGetValue(pluginName, out var config))
        {
            var hasPermission = config.GrantedPermissions.HasFlag(permission);

            if (!hasPermission)
            {
                _logger.Debug("Plugin {PluginName} lacks permission: {Permission}", pluginName, permission);
            }

            return hasPermission;
        }

        // No explicit configuration, use default permissions
        var hasDefaultPermission = DEFAULT_PERMISSIONS.HasFlag(permission);

        if (!hasDefaultPermission)
        {
            _logger.Debug("Plugin {PluginName} lacks default permission: {Permission}", pluginName, permission);
        }

        return hasDefaultPermission;
    }

    /// <summary>
    /// Sets permissions for a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <param name="permissions">Permissions to grant</param>
    public static void SetPermissions (string pluginName, PluginPermission permissions)
    {
        if (string.IsNullOrWhiteSpace(pluginName))
        {
            throw new ArgumentNullException(nameof(pluginName));
        }

        if (!_pluginPermissions.TryGetValue(pluginName, out PluginPermissionConfig? value))
        {
            _pluginPermissions[pluginName] = new PluginPermissionConfig
            {
                PluginName = pluginName,
                GrantedPermissions = permissions
            };
        }
        else
        {
            value.GrantedPermissions = permissions;
        }

        _logger.Info("Set permissions for plugin {PluginName}: {Permissions}", pluginName, permissions);
    }

    /// <summary>
    /// Gets the permissions for a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <returns>Plugin permissions or default permissions if not configured</returns>
    public static PluginPermission GetPermissions (string pluginName)
    {
        if (string.IsNullOrWhiteSpace(pluginName))
        {
            return PluginPermission.None;
        }

        if (_pluginPermissions.TryGetValue(pluginName, out var config))
        {
            return config.GrantedPermissions;
        }

        return DEFAULT_PERMISSIONS;
    }

    /// <summary>
    /// Parses permission string (from manifest) to PluginPermission enum.
    /// </summary>
    /// <param name="permissionString">Permission string (e.g., "filesystem:read")</param>
    /// <returns>PluginPermission enum value</returns>
    public static PluginPermission ParsePermission (string permissionString)
    {
        if (string.IsNullOrWhiteSpace(permissionString))
        {
            return PluginPermission.None;
        }

        return permissionString.ToLowerInvariant() switch
        {
            "filesystem:read" => PluginPermission.FileSystemRead,
            "filesystem:write" => PluginPermission.FileSystemWrite,
            "network:connect" => PluginPermission.NetworkConnect,
            "config:read" => PluginPermission.ConfigRead,
            "config:write" => PluginPermission.ConfigWrite,
            "registry:read" => PluginPermission.RegistryRead,
            _ => PluginPermission.None
        };
    }

    /// <summary>
    /// Parses a list of permission strings to combined PluginPermission flags.
    /// </summary>
    /// <param name="permissionStrings">List of permission strings</param>
    /// <returns>Combined PluginPermission flags</returns>
    public static PluginPermission ParsePermissions (IEnumerable<string> permissionStrings)
    {
        if (permissionStrings == null)
        {
            return PluginPermission.None;
        }

        var permissions = PluginPermission.None;

        foreach (var permissionString in permissionStrings)
        {
            permissions |= ParsePermission(permissionString);
        }

        return permissions;
    }

    /// <summary>
    /// Converts PluginPermission enum to human-readable string.
    /// </summary>
    /// <param name="permission">Permission to convert</param>
    /// <returns>Human-readable permission string</returns>
    public static string PermissionToString (PluginPermission permission)
    {
        if (permission == PluginPermission.None)
        {
            return "None";
        }

        if (permission == PluginPermission.All)
        {
            return "All";
        }

        var permissions = new List<string>();

        if (permission.HasFlag(PluginPermission.FileSystemRead))
        {
            permissions.Add("File System Read");
        }

        if (permission.HasFlag(PluginPermission.FileSystemWrite))
        {
            permissions.Add("File System Write");
        }

        if (permission.HasFlag(PluginPermission.NetworkConnect))
        {
            permissions.Add("Network Connect");
        }

        if (permission.HasFlag(PluginPermission.ConfigRead))
        {
            permissions.Add("Config Read");
        }

        if (permission.HasFlag(PluginPermission.ConfigWrite))
        {
            permissions.Add("Config Write");
        }

        if (permission.HasFlag(PluginPermission.RegistryRead))
        {
            permissions.Add("Registry Read");
        }

        return string.Join(", ", permissions);
    }

    /// <summary>
    /// Loads plugin permissions from configuration file.
    /// </summary>
    /// <param name="configDir">Configuration directory path</param>
    public static void LoadPermissions (string configDir)
    {
        try
        {
            var permissionsFile = Path.Combine(configDir, "plugin-permissions.json");

            if (!File.Exists(permissionsFile))
            {
                _logger.Debug("Plugin permissions file not found, using defaults");
                return;
            }

            var json = File.ReadAllText(permissionsFile);
            var permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PluginPermissionConfig>>(json);

            if (permissions != null)
            {
                _pluginPermissions.Clear();

                foreach (var kvp in permissions)
                {
                    _pluginPermissions[kvp.Key] = kvp.Value;
                }

                _logger.Info("Loaded permissions for {Count} plugins", _pluginPermissions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading plugin permissions from {ConfigDir}", configDir);
        }
    }

    /// <summary>
    /// Saves plugin permissions to configuration file.
    /// </summary>
    /// <param name="configDir">Configuration directory path</param>
    public static void SavePermissions (string configDir)
    {
        try
        {
            var permissionsFile = Path.Combine(configDir, "plugin-permissions.json");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_pluginPermissions, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(permissionsFile, json);

            _logger.Info("Saved permissions for {Count} plugins", _pluginPermissions.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving plugin permissions to {ConfigDir}", configDir);
        }
    }

    #endregion
}

/// <summary>
/// Represents plugin permission configuration.
/// </summary>
public class PluginPermissionConfig
{
    /// <summary>
    /// Plugin name.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("pluginName")]
    public string PluginName { get; set; }

    /// <summary>
    /// Granted permissions.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("grantedPermissions")]
    public PluginPermission GrantedPermissions { get; set; }

    /// <summary>
    /// Whether the plugin is trusted by the user.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("trusted")]
    public bool Trusted { get; set; }

    /// <summary>
    /// When permissions were last modified.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
