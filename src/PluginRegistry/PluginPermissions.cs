using System.Security;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Manages plugin permissions and validates permission requests.
/// </summary>
public static class PluginPermissionManager
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
        return string.IsNullOrWhiteSpace(pluginName)
            ? PluginPermission.None
            : _pluginPermissions.TryGetValue(pluginName, out var config)
                ? config.GrantedPermissions
                : DEFAULT_PERMISSIONS;
    }

    /// <summary>
    /// Parses permission string (from manifest) to PluginPermission enum.
    /// </summary>
    /// <param name="permissionString">Permission string (e.g., "filesystem:read")</param>
    /// <returns>PluginPermission enum value</returns>
    public static PluginPermission ParsePermission (string permissionString)
    {
        return string.IsNullOrWhiteSpace(permissionString)
            ? PluginPermission.None
            : permissionString.ToUpperInvariant() switch
            {
                "FILESYSTEM:READ" => PluginPermission.FileSystemRead,
                "FILESYSTEM:WRITE" => PluginPermission.FileSystemWrite,
                "NETWORK:CONNECT" => PluginPermission.NetworkConnect,
                "CONFIG:READ" => PluginPermission.ConfigRead,
                "CONFIG:WRITE" => PluginPermission.ConfigWrite,
                "REGISTRY:READ" => PluginPermission.RegistryRead,
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
            var permissionsFile = Path.Join(configDir, "plugin-permissions.json");

            if (!File.Exists(permissionsFile))
            {
                _logger.Debug("Plugin permissions file not found, using defaults");
                return;
            }

            var json = File.ReadAllText(permissionsFile);
            var permissions = JsonConvert.DeserializeObject<Dictionary<string, PluginPermissionConfig>>(json);

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
        catch (Exception ex) when (ex is IOException or
                                         JsonException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         PathTooLongException or
                                         DirectoryNotFoundException or
                                         FileNotFoundException or
                                         NotSupportedException or
                                         SecurityException)
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
            var permissionsFile = Path.Join(configDir, "plugin-permissions.json");
            var json = JsonConvert.SerializeObject(_pluginPermissions, Formatting.Indented);

            File.WriteAllText(permissionsFile, json);

            _logger.Info("Saved permissions for {Count} plugins", _pluginPermissions.Count);
        }
        catch (Exception ex) when (ex is IOException or
                                         JsonException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         PathTooLongException or
                                         DirectoryNotFoundException or
                                         FileNotFoundException or
                                         NotSupportedException or
                                         SecurityException)
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
    [JsonProperty("pluginName")]
    public required string PluginName { get; set; }

    /// <summary>
    /// Granted permissions.
    /// </summary>
    [JsonProperty("grantedPermissions")]
    public PluginPermission GrantedPermissions { get; set; }

    /// <summary>
    /// Whether the plugin is trusted by the user.
    /// </summary>
    [JsonProperty("trusted")]
    public bool Trusted { get; set; }

    /// <summary>
    /// When permissions were last modified.
    /// </summary>
    [JsonProperty("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
