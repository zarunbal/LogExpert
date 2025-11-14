using System.Reflection;
using System.Security.Cryptography;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Validates plugin assemblies before loading to prevent security vulnerabilities.
/// </summary>
public class PluginValidator
{
    #region Fields

    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    // Whitelist of trusted plugin file names (shipped with LogExpert)
    private static readonly HashSet<string> _trustedPluginNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "AutoColumnizer.dll",
        "CsvColumnizer.dll",
        "JsonColumnizer.dll",
        "JsonCompactColumnizer.dll",
        "RegexColumnizer.dll",
        "Log4jXmlColumnizer.dll",
        "GlassfishColumnizer.dll",
        "DefaultPlugins.dll",
        "FlashIconHighlighter.dll",
        "SftpFileSystem.dll",
        "SftpFileSystemx86.dll",
        "SftpFileSystemx64.dll"
    };

    // Known safe dependencies (not plugins themselves)
    private static readonly HashSet<string> _knownDependencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "ColumnizerLib.dll",
        "Newtonsoft.Json.dll",
        "CsvHelper.dll",
        "Renci.SshNet.dll",
        "Microsoft.Bcl.AsyncInterfaces.dll",
        "Microsoft.Bcl.HashCode.dll",
        "System.Buffers.dll",
        "System.Memory.dll",
        "System.Numerics.Vectors.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Threading.Tasks.Extensions.dll"
    };

    #endregion

    #region Public methods

    /// <summary>
    /// Validates a plugin assembly before loading.
    /// </summary>
    /// <param name="dllPath">Path to the plugin DLL</param>
    /// <returns>True if the plugin is valid and safe to load</returns>
    public static bool ValidatePlugin (string dllPath)
    {
        return ValidatePlugin(dllPath, out _);
    }

    /// <summary>
    /// Validates a plugin assembly before loading with manifest information.
    /// </summary>
    /// <param name="dllPath">Path to the plugin DLL</param>
    /// <param name="manifest">Output manifest if found and valid, null otherwise</param>
    /// <returns>True if the plugin is valid and safe to load</returns>
    public static bool ValidatePlugin (string dllPath, out PluginManifest manifest)
    {
        manifest = null;

        try
        {
            // 1. Check if file exists
            if (!File.Exists(dllPath))
            {
                _logger.Warn("Plugin file does not exist: {DllPath}", dllPath);
                return false;
            }

            var fileName = Path.GetFileName(dllPath);
            //var pluginDir = Path.GetDirectoryName(dllPath);

            // 2. Check if it's a known dependency (not a plugin)
            if (_knownDependencies.Contains(fileName))
            {
                _logger.Debug("Skipping dependency DLL: {FileName}", fileName);
                return false; // Not a plugin, skip it
            }

            // 3. Check whitelist (trusted plugins shipped with LogExpert)
            if (!_trustedPluginNames.Contains(fileName))
            {
                _logger.Warn("Plugin not in whitelist: {FileName}. Skipping for security reasons.", fileName);
                _logger.Info("To load custom plugins, add them to the trusted plugins list in settings.");
                return false;
            }

            // 4. Try to load and validate manifest
            manifest = LoadAndValidateManifest(dllPath);
            if (manifest != null)
            {
                _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", manifest.Name, manifest.Version);

                // 4a. Check version compatibility
                if (!CheckVersionCompatibility(manifest))
                {
                    _logger.Error("Plugin {PluginName} is not compatible with current LogExpert version", manifest.Name);
                    return false;
                }

                // 4b. Extract and set permissions from manifest
                if (manifest.Permissions != null && manifest.Permissions.Count > 0)
                {
                    var permissions = PluginPermissionManager.ParsePermissions(manifest.Permissions);
                    var pluginName = Path.GetFileNameWithoutExtension(fileName);
                    PluginPermissionManager.SetPermissions(pluginName, permissions);
                    _logger.Info("Set permissions for {PluginName}: {Permissions}",
                        pluginName, PluginPermissionManager.PermissionToString(permissions));
                }
            }
            else
            {
                _logger.Debug("No manifest found for {FileName}, using default permissions", fileName);
            }

            // 5. Verify assembly can be loaded (basic validation)
            if (!CanLoadAssembly(dllPath))
            {
                _logger.Error("Plugin assembly cannot be loaded: {FileName}", fileName);
                return false;
            }

            // 6. Verify assembly is a valid .NET assembly
            if (!IsValidDotNetAssembly(dllPath))
            {
                _logger.Error("Plugin is not a valid .NET assembly: {FileName}", fileName);
                return false;
            }

            _logger.Info("Plugin validated successfully: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating plugin: {DllPath}", dllPath);
            return false;
        }
    }

    /// <summary>
    /// Checks if a plugin is in the trusted whitelist.
    /// </summary>
    public static bool IsTrustedPlugin (string fileName)
    {
        var pluginName = Path.GetFileName(fileName);
        return _trustedPluginNames.Contains(pluginName);
    }

    /// <summary>
    /// Adds a plugin to the trusted whitelist (for custom plugins).
    /// </summary>
    public static void AddTrustedPlugin (string fileName)
    {
        var pluginName = Path.GetFileName(fileName);
        if (!string.IsNullOrEmpty(pluginName))
        {
            _ = _trustedPluginNames.Add(pluginName);
            _logger.Info("Added plugin to trusted list: {PluginName}", pluginName);
        }
    }

    /// <summary>
    /// Gets the list of trusted plugin names.
    /// </summary>
    public static IReadOnlySet<string> GetTrustedPlugins ()
    {
        return _trustedPluginNames;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads and validates a plugin manifest file.
    /// </summary>
    /// <param name="dllPath">Path to the plugin DLL</param>
    /// <returns>Validated manifest or null if not found/invalid</returns>
    private static PluginManifest LoadAndValidateManifest (string dllPath)
    {
        try
        {
            // Look for manifest file: PluginName.manifest.json
            var manifestPath = Path.ChangeExtension(dllPath, ".manifest.json");

            if (!File.Exists(manifestPath))
            {
                _logger.Debug("No manifest file found at: {ManifestPath}", manifestPath);
                return null;
            }

            // Load manifest
            var manifest = PluginManifest.Load(manifestPath);
            if (manifest == null)
            {
                _logger.Error("Failed to load manifest from: {ManifestPath}", manifestPath);
                return null;
            }

            // Validate manifest
            if (!manifest.Validate(out var errors))
            {
                _logger.Error("Manifest validation failed for {ManifestPath}:", manifestPath);
                foreach (var error in errors)
                {
                    _logger.Error("  - {Error}", error);
                }
                return null;
            }

            return manifest;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading manifest for: {DllPath}", dllPath);
            return null;
        }
    }

    /// <summary>
    /// Checks if the plugin is compatible with the current LogExpert version.
    /// </summary>
    /// <param name="manifest">Plugin manifest</param>
    /// <returns>True if compatible, false otherwise</returns>
    private static bool CheckVersionCompatibility (PluginManifest manifest)
    {
        try
        {
            // Get current LogExpert version
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            if (version == null)
            {
                _logger.Warn("Could not determine LogExpert version, assuming compatible");
                return true;
            }

            // Check compatibility
            if (!manifest.IsCompatibleWith(version))
            {
                _logger.Error("Plugin {PluginName} requires LogExpert {Requirement}, but current version is {CurrentVersion}",
                    manifest.Name, manifest.Requires?.LogExpert ?? "unknown", version);
                return false;
            }

            _logger.Debug("Plugin {PluginName} is compatible with LogExpert {Version}", manifest.Name, version);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking version compatibility for plugin: {PluginName}", manifest.Name);
            // On error, assume compatible (don't block plugin loading)
            return true;
        }
    }

    /// <summary>
    /// Checks if an assembly can be loaded without throwing exceptions.
    /// </summary>
    private static bool CanLoadAssembly (string dllPath)
    {
        try
        {
            // Try to get assembly name without loading it fully
            _ = AssemblyName.GetAssemblyName(dllPath);
            return true;
        }
        catch (BadImageFormatException ex)
        {
            _logger.Debug(ex, "Plugin has invalid format (possibly wrong architecture): {DllPath}", dllPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Cannot load plugin assembly: {DllPath}", dllPath);
            return false;
        }
    }

    /// <summary>
    /// Validates that the file is a valid .NET assembly.
    /// </summary>
    private static bool IsValidDotNetAssembly (string dllPath)
    {
        try
        {
            using var stream = File.OpenRead(dllPath);
            using var reader = new BinaryReader(stream);

            // Check PE header
            if (stream.Length < 64)
            {
                return false;
            }

            // Read DOS header
            var dosHeader = reader.ReadUInt16();
            if (dosHeader != 0x5A4D) // "MZ"
            {
                return false;
            }

            // Jump to PE header offset
            stream.Seek(60, SeekOrigin.Begin);
            var peHeaderOffset = reader.ReadInt32();

            if (peHeaderOffset >= stream.Length - 4)
            {
                return false;
            }

            // Read PE signature
            stream.Seek(peHeaderOffset, SeekOrigin.Begin);
            var peSignature = reader.ReadUInt32();
            if (peSignature != 0x00004550) // "PE\0\0"
            {
                return false;
            }

            // Basic validation passed
            return true;
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Error checking PE format: {DllPath}", dllPath);
            return false;
        }
    }

    /// <summary>
    /// Calculates SHA256 hash of a file for integrity verification.
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>SHA256 hash as hex string</returns>
    public static string CalculateFileHash (string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error calculating file hash: {FilePath}", filePath);
            return string.Empty;
        }
    }

    #endregion
}
