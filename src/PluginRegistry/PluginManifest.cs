using Newtonsoft.Json;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Represents a plugin manifest file that declares plugin metadata, requirements, and permissions.
/// </summary>
public class PluginManifest
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Properties

    /// <summary>
    /// Plugin name (must match DLL name without extension).
    /// </summary>
    /// <value>
    /// The name of the plugin. This value should match the plugin's DLL file name without the .dll extension.
    /// </value>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Plugin version (semantic versioning: major.minor.patch).
    /// </summary>
    /// <value>
    /// The version string following semantic versioning format (e.g., "1.0.0" or "2.1.5").
    /// </value>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Plugin author or organization.
    /// </summary>
    /// <value>
    /// The name of the individual or organization that authored the plugin.
    /// </value>
    [JsonProperty("author")]
    public string Author { get; set; }

    /// <summary>
    /// Brief description of plugin functionality.
    /// </summary>
    /// <value>
    /// A human-readable description explaining what the plugin does and its purpose.
    /// </value>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// LogExpert plugin API version this plugin targets.
    /// </summary>
    /// <value>
    /// The API version string indicating which LogExpert plugin API this plugin is designed to work with.
    /// </value>
    [JsonProperty("apiVersion")]
    public string ApiVersion { get; set; }

    /// <summary>
    /// Requirements for running this plugin (LogExpert version, .NET version, etc.).
    /// </summary>
    /// <value>
    /// An object containing version requirements for LogExpert and .NET runtime. May be null if no specific requirements exist.
    /// </value>
    [JsonProperty("requires")]
    public PluginRequirements Requires { get; set; }

    /// <summary>
    /// Permissions required by this plugin.
    /// </summary>
    /// <value>
    /// A list of permission strings (e.g., "filesystem:read", "network:connect") that the plugin requires to function.
    /// Defaults to an empty list.
    /// </value>
    [JsonProperty("permissions")]
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// External dependencies required by this plugin.
    /// </summary>
    /// <value>
    /// A dictionary mapping dependency names to their version requirements.
    /// Defaults to an empty dictionary.
    /// </value>
    [JsonProperty("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    /// <summary>
    /// Main DLL file name.
    /// </summary>
    /// <value>
    /// The name of the primary DLL file that contains the plugin implementation.
    /// </value>
    [JsonProperty("main")]
    public string Main { get; set; }

    /// <summary>
    /// Optional: Plugin website or repository URL.
    /// </summary>
    /// <value>
    /// A URL pointing to the plugin's homepage, documentation, or source code repository. May be null.
    /// </value>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Optional: Plugin license (e.g., "MIT", "Apache-2.0").
    /// </summary>
    /// <value>
    /// The license identifier under which the plugin is distributed (e.g., "MIT", "Apache-2.0", "GPL-3.0"). May be null.
    /// </value>
    [JsonProperty("license")]
    public string License { get; set; }

    #endregion

    #region Public methods

    /// <summary>
    /// Loads a plugin manifest from a JSON file.
    /// </summary>
    /// <param name="manifestPath">Path to the manifest file</param>
    /// <returns>Parsed manifest object if successful; otherwise, null</returns>
    /// <remarks>
    /// This method reads the JSON file, deserializes it into a <see cref="PluginManifest"/> object,
    /// and logs the operation result. If the file doesn't exist or deserialization fails, null is returned.
    /// </remarks>
    /// <exception cref="Exception">Logs any exceptions that occur during file reading or deserialization but returns null instead of throwing.</exception>
    public static PluginManifest Load (string manifestPath)
    {
        try
        {
            if (!File.Exists(manifestPath))
            {
                _logger.Debug("Manifest file not found: {ManifestPath}", manifestPath);
                return null;
            }

            var json = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);

            if (manifest == null)
            {
                _logger.Error("Failed to deserialize manifest: {ManifestPath}", manifestPath);
                return null;
            }

            _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", manifest.Name, manifest.Version);
            return manifest;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading manifest from: {ManifestPath}", manifestPath);
            return null;
        }
    }

    /// <summary>
    /// Validates the manifest for required fields and correct values.
    /// </summary>
    /// <param name="errors">Output list of validation errors. Will be populated with error messages if validation fails.</param>
    /// <returns>True if the manifest is valid; otherwise, false</returns>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// <list type="bullet">
    /// <item><description>Checking for required fields (name, version, main, apiVersion)</description></item>
    /// <item><description>Validating version format (semantic versioning)</description></item>
    /// <item><description>Validating version requirements for LogExpert and .NET</description></item>
    /// <item><description>Validating permission strings against known permission types</description></item>
    /// </list>
    /// </remarks>
    public bool Validate (out List<string> errors)
    {
        errors = [];

        // Required fields
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Missing required field: name");
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            errors.Add("Missing required field: version");
        }
        else if (!IsValidVersion(Version))
        {
            errors.Add($"Invalid version format: {Version} (expected: major.minor.patch)");
        }

        if (string.IsNullOrWhiteSpace(Main))
        {
            errors.Add("Missing required field: main");
        }

        if (string.IsNullOrWhiteSpace(ApiVersion))
        {
            errors.Add("Missing required field: apiVersion");
        }

        // Validate requirements if present
        if (Requires != null)
        {
            if (!string.IsNullOrWhiteSpace(Requires.LogExpert) && !IsValidVersionRequirement(Requires.LogExpert))
            {
                errors.Add($"Invalid LogExpert version requirement: {Requires.LogExpert}");
            }

            if (!string.IsNullOrWhiteSpace(Requires.DotNet) && !IsValidVersionRequirement(Requires.DotNet))
            {
                errors.Add($"Invalid .NET version requirement: {Requires.DotNet}");
            }
        }

        // Validate permissions if present
        if (Permissions != null && Permissions.Count > 0)
        {
            foreach (var permission in Permissions)
            {
                if (!IsValidPermission(permission))
                {
                    errors.Add($"Invalid permission: {permission}");
                }
            }
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Checks if this plugin is compatible with the current LogExpert version.
    /// </summary>
    /// <param name="logExpertVersion">Current LogExpert version to check against</param>
    /// <returns>True if the plugin is compatible with the specified LogExpert version; otherwise, false</returns>
    /// <remarks>
    /// This method supports various version constraint operators:
    /// <list type="bullet">
    /// <item><description><c>&gt;=</c> - Greater than or equal to</description></item>
    /// <item><description><c>&gt;</c> - Greater than</description></item>
    /// <item><description><c>&lt;=</c> - Less than or equal to</description></item>
    /// <item><description><c>&lt;</c> - Less than</description></item>
    /// <item><description><c>~</c> - Tilde range (allows patch-level changes, e.g., ~1.2.3 matches 1.2.3, 1.2.4, but not 1.3.0)</description></item>
    /// <item><description><c>^</c> - Caret range (allows minor-level changes, e.g., ^1.2.3 matches 1.2.3, 1.3.0, but not 2.0.0)</description></item>
    /// <item><description>No operator - Exact version match</description></item>
    /// </list>
    /// If no requirement is specified in the manifest, the plugin is assumed to be compatible.
    /// </remarks>
    /// <exception cref="Exception">Logs any exceptions that occur during version parsing but returns false instead of throwing.</exception>
    public bool IsCompatibleWith (Version logExpertVersion)
    {
        if (Requires == null || string.IsNullOrWhiteSpace(Requires.LogExpert))
        {
            // No requirement specified, assume compatible
            return true;
        }

        try
        {
            var requirement = Requires.LogExpert;

            // Parse version requirement (e.g., ">=1.10.0", "~1.10.0", "1.10.0")
            if (requirement.StartsWith(">="))
            {
                var requiredVersion = System.Version.Parse(requirement[2..].Trim());
                return logExpertVersion >= requiredVersion;
            }

            if (requirement.StartsWith('>'))
            {
                var requiredVersion = System.Version.Parse(requirement[1..].Trim());
                return logExpertVersion > requiredVersion;
            }

            if (requirement.StartsWith("<="))
            {
                var requiredVersion = System.Version.Parse(requirement[2..].Trim());
                return logExpertVersion <= requiredVersion;
            }

            if (requirement.StartsWith('<'))
            {
                var requiredVersion = System.Version.Parse(requirement[1..].Trim());
                return logExpertVersion < requiredVersion;
            }

            if (requirement.StartsWith('~'))
            {
                // Tilde range: allows patch-level changes
                var requiredVersion = System.Version.Parse(requirement[1..].Trim());
                return logExpertVersion.Major == requiredVersion.Major &&
                       logExpertVersion.Minor == requiredVersion.Minor &&
                       logExpertVersion >= requiredVersion;
            }

            if (requirement.StartsWith('^'))
            {
                // Caret range: allows minor-level changes
                var requiredVersion = System.Version.Parse(requirement[1..].Trim());
                return logExpertVersion.Major == requiredVersion.Major &&
                       logExpertVersion >= requiredVersion;
            }

            // Exact version match
            var exactVersion = System.Version.Parse(requirement);
            return logExpertVersion == exactVersion;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error parsing version requirement: {Requirement}", Requires.LogExpert);
            return false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Validates if a version string follows semantic versioning format.
    /// </summary>
    /// <param name="versionString">The version string to validate</param>
    /// <returns>True if the version string is valid (major.minor or major.minor.patch); otherwise, false</returns>
    /// <remarks>
    /// Accepts semantic versioning in the format "major.minor" or "major.minor.patch" where each component is a valid integer.
    /// </remarks>
    private static bool IsValidVersion (string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return false;
        }

        // Accept semantic versioning: major.minor.patch or major.minor
        var parts = versionString.Split('.');
        return parts.Length is not < 2 and not > 3 && parts.All(part => int.TryParse(part, out _));
    }

    /// <summary>
    /// Validates if a version requirement string is properly formatted.
    /// </summary>
    /// <param name="requirement">The version requirement string to validate (may include operators like &gt;=, ~, ^, etc.)</param>
    /// <returns>True if the requirement string is valid; otherwise, false</returns>
    /// <remarks>
    /// This method strips any operator prefix and validates that the remaining version string can be parsed as a valid <see cref="System.Version"/>.
    /// </remarks>
    private static bool IsValidVersionRequirement (string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
        {
            return false;
        }

        // Remove operator prefix
        var versionPart = requirement.TrimStart('>', '<', '=', '~', '^').Trim();

        try
        {
            _ = System.Version.Parse(versionPart);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or
                                        FormatException or
                                        ArgumentNullException or
                                        ArgumentOutOfRangeException or
                                        OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a permission string is recognized as a valid permission type.
    /// </summary>
    /// <param name="permission">The permission string to validate</param>
    /// <returns>True if the permission is in the list of valid permissions; otherwise, false</returns>
    /// <remarks>
    /// Valid permissions include:
    /// <list type="bullet">
    /// <item><description>filesystem:read - Permission to read from the file system</description></item>
    /// <item><description>filesystem:write - Permission to write to the file system</description></item>
    /// <item><description>network:connect - Permission to make network connections</description></item>
    /// <item><description>config:read - Permission to read configuration data</description></item>
    /// <item><description>config:write - Permission to write configuration data</description></item>
    /// <item><description>registry:read - Permission to read from the Windows registry</description></item>
    /// </list>
    /// The comparison is case-insensitive.
    /// </remarks>
    private static bool IsValidPermission (string permission)
    {
        var validPermissions = new[]
        {
            "filesystem:read",
            "filesystem:write",
            "network:connect",
            "config:read",
            "config:write",
            "registry:read"
        };

        return validPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    #endregion
}

/// <summary>
/// Represents version requirements for a plugin, including LogExpert and .NET runtime versions.
/// </summary>
/// <param name="LogExpert">
/// The LogExpert version requirement string. May include operators like &gt;=, ~, ^, etc.
/// Example: "&gt;=1.10.0" or "~2.0.0"
/// </param>
/// <param name="DotNet">
/// The .NET runtime version requirement string. May include operators like &gt;=, ~, ^, etc.
/// Example: "&gt;=8.0.0"
/// </param>
/// <remarks>
/// This record is used within <see cref="PluginManifest"/> to specify minimum or compatible versions
/// of the host application and runtime environment required by a plugin.
/// </remarks>
public record PluginRequirements ([property: JsonProperty("logExpert")] string LogExpert, [property: JsonProperty("dotnet")] string DotNet);