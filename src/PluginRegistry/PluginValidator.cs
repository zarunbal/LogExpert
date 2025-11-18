using System.Reflection;
using System.Security;

using Newtonsoft.Json;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Validates plugin assemblies before loading to prevent security vulnerabilities.
/// </summary>
public static class PluginValidator
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static TrustedPluginConfig _trustedPluginConfig;
    private static readonly Lock _configLock = new();
    private static readonly string _configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogExpert");
    private static readonly string _configPath = Path.Combine(_configDirectory, "trusted-plugins.json");

    // Whitelist of trusted plugin file names (shipped with LogExpert) - used as defaults
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

    #region Constructor

    static PluginValidator ()
    {
        LoadTrustedPluginConfiguration();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Loads trusted plugin configuration from disk.
    /// </summary>
    private static void LoadTrustedPluginConfiguration ()
    {
        lock (_configLock)
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = File.ReadAllText(_configPath);
                    _trustedPluginConfig = JsonConvert.DeserializeObject<TrustedPluginConfig>(json);
                    _logger.Info("Loaded trusted plugin configuration from {ConfigPath}", _configPath);

                    // Validate configuration
                    if (_trustedPluginConfig == null)
                    {
                        _logger.Warn("Deserialized config is null, creating default");
                        _trustedPluginConfig = CreateDefaultConfiguration();
                        SaveTrustedPluginConfiguration();
                    }
                }
                catch (Exception ex) when (ex is IOException or
                                                 UnauthorizedAccessException or
                                                 ArgumentException or
                                                 ArgumentNullException or
                                                 PathTooLongException or
                                                 DirectoryNotFoundException or
                                                 NotSupportedException or
                                                 SecurityException or
                                                 JsonSerializationException)
                {
                    _logger.Error(ex, "Failed to load trusted plugin configuration, using defaults");
                    _trustedPluginConfig = CreateDefaultConfiguration();
                    SaveTrustedPluginConfiguration();
                }
            }
            else
            {
                _logger.Info("No trusted plugin configuration found, creating default");
                _trustedPluginConfig = CreateDefaultConfiguration();
                SaveTrustedPluginConfiguration();
            }
        }
    }

    /// <summary>
    /// Creates default configuration with built-in trusted plugins.
    /// </summary>
    private static TrustedPluginConfig CreateDefaultConfiguration ()
    {
        return new TrustedPluginConfig
        {
            PluginNames = [.. _trustedPluginNames],
            PluginHashes = GetBuiltInPluginHashes(),
            AllowUserTrustedPlugins = true,
            HashAlgorithm = "SHA256",
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Saves trusted plugin configuration to disk.
    /// </summary>
    private static void SaveTrustedPluginConfiguration ()
    {
        lock (_configLock)
        {
            try
            {
                _ = Directory.CreateDirectory(_configDirectory);
                var json = JsonConvert.SerializeObject(_trustedPluginConfig, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                _logger.Info("Saved trusted plugin configuration to {ConfigPath}", _configPath);
            }
            catch (Exception ex) when (ex is IOException or
                                             UnauthorizedAccessException or
                                             ArgumentException or
                                             ArgumentNullException or
                                             PathTooLongException or
                                             DirectoryNotFoundException or
                                             NotSupportedException or
                                             SecurityException or
                                             JsonSerializationException)
            {
                _logger.Error(ex, "Failed to save trusted plugin configuration");
            }
        }
    }

    /// <summary>
    /// Adds a plugin to the trusted list and saves the configuration.
    /// </summary>
    /// <param name="dllPath">Path to the plugin DLL</param>
    /// <param name="errorMessage">Error message if operation fails</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool AddTrustedPlugin (string dllPath, out string errorMessage)
    {
        errorMessage = null;

        try
        {
            if (!File.Exists(dllPath))
            {
                errorMessage = $"Plugin file not found: {dllPath}";
                return false;
            }

            var fileName = Path.GetFileName(dllPath);
            var hash = PluginHashCalculator.CalculateHash(dllPath);

            lock (_configLock)
            {
                if (!_trustedPluginConfig.AllowUserTrustedPlugins)
                {
                    errorMessage = "User-added trusted plugins are not allowed by policy";
                    return false;
                }

                if (!_trustedPluginConfig.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    _trustedPluginConfig.PluginNames.Add(fileName);
                }

                _trustedPluginConfig.PluginHashes[fileName] = hash;
                _trustedPluginConfig.LastUpdated = DateTime.UtcNow;

                SaveTrustedPluginConfiguration();
            }

            _logger.Info("Added trusted plugin: {FileName}, Hash: {Hash}", fileName, hash);
            return true;
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         ArgumentNullException or
                                         PathTooLongException or
                                         DirectoryNotFoundException or
                                         NotSupportedException or
                                         SecurityException or
                                         JsonSerializationException)
        {
            errorMessage = $"Error adding trusted plugin: {ex.Message}";
            _logger.Error(ex, "Error adding trusted plugin: {DllPath}", dllPath);
            return false;
        }
    }

    /// <summary>
    /// Removes a plugin from the trusted list.
    /// </summary>
    /// <param name="fileName">Plugin file name</param>
    /// <returns>True if removed, false if not found</returns>
    public static bool RemoveTrustedPlugin (string fileName)
    {
        lock (_configLock)
        {
            var removed = _trustedPluginConfig.PluginNames.Remove(fileName);
            if (removed)
            {
                _ = _trustedPluginConfig.PluginHashes.Remove(fileName);
                _trustedPluginConfig.LastUpdated = DateTime.UtcNow;
                SaveTrustedPluginConfiguration();
                _logger.Info("Removed trusted plugin: {FileName}", fileName);
            }

            return removed;
        }
    }

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

            // 2. Check if it's a known dependency (not a plugin)
            if (_knownDependencies.Contains(fileName))
            {
                _logger.Debug("Skipping dependency DLL: {FileName}", fileName);
                return false; // Not a plugin, skip it
            }

            // 3. Calculate file hash using PluginHashCalculator
            string fileHash;
            try
            {
                fileHash = PluginHashCalculator.CalculateHash(dllPath);
                _logger.Debug("Plugin {FileName} hash: {Hash}", fileName, fileHash);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to calculate hash for plugin: {FileName}", fileName);
                return false;
            }

            // 4. Check trust status
            var isTrustedByName = _trustedPluginConfig.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
            var isTrustedByHash = _trustedPluginConfig.PluginHashes.ContainsValue(fileHash);

            if (!isTrustedByName && !isTrustedByHash)
            {
                _logger.Warn("Plugin not trusted: {FileName}, Hash: {Hash}", fileName, fileHash);
                _logger.Info("To trust this plugin, add it via Settings > Plugin Management");
                return false;
            }

            // 5. Verify hash for known plugins using PluginHashCalculator
            if (isTrustedByName && _trustedPluginConfig.PluginHashes.TryGetValue(fileName, out var expectedHash))
            {
                if (!PluginHashCalculator.VerifyHash(dllPath, expectedHash))
                {
                    _logger.Error("SECURITY: Plugin hash mismatch for {FileName}", fileName);
                    _logger.Error("  Expected: {Expected}", expectedHash);
                    _logger.Error("  Actual:   {Actual}", fileHash);
                    _logger.Error("  This could indicate file tampering or corruption!");
                    return false;
                }

                _logger.Debug("Plugin hash verified: {FileName}", fileName);
            }
            else if (isTrustedByHash)
            {
                _logger.Info("Plugin {FileName} trusted by hash: {Hash}", fileName, fileHash);
            }

            // 6. Try to load and validate manifest
            manifest = LoadAndValidateManifest(dllPath);
            if (manifest != null)
            {
                _logger.Info("Loaded manifest for plugin: {PluginName} v{Version}", manifest.Name, manifest.Version);

                // 6a. Check version compatibility
                if (!CheckVersionCompatibility(manifest))
                {
                    _logger.Error("Plugin {PluginName} is not compatible with current LogExpert version", manifest.Name);
                    return false;
                }

                // 6b. Validate manifest paths for security
                var pluginDirectory = Path.GetDirectoryName(dllPath);
                if (!ValidateManifestPaths(manifest, pluginDirectory))
                {
                    _logger.Error("Manifest path validation failed for {Plugin}", manifest.Name);
                    return false;
                }

                // 6c. Extract and set permissions from manifest
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

            // 7. Verify assembly can be loaded (basic validation)
            if (!CanLoadAssembly(dllPath))
            {
                _logger.Error("Plugin assembly cannot be loaded: {FileName}", fileName);
                return false;
            }

            // 8. Verify assembly is a valid .NET assembly
            if (!IsValidDotNetAssembly(dllPath))
            {
                _logger.Error("Plugin is not a valid .NET assembly: {FileName}", fileName);
                return false;
            }

            _logger.Info("Plugin validated successfully: {FileName}", fileName);
            return true;
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         BadImageFormatException)
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

    #endregion

    #region Private Methods

    /// <summary>
    /// Validates that manifest paths don't escape the plugin directory (path traversal protection).
    /// </summary>
    /// <param name="manifest">Plugin manifest</param>
    /// <param name="pluginDirectory">Plugin directory path</param>
    /// <returns>True if paths are safe, false if path traversal detected</returns>
    private static bool ValidateManifestPaths (PluginManifest manifest, string pluginDirectory)
    {
        try
        {
            var pluginDir = Path.GetFullPath(pluginDirectory);

            // Validate main file path
            var mainPath = Path.GetFullPath(Path.Combine(pluginDirectory, manifest.Main));

            if (!mainPath.StartsWith(pluginDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error("SECURITY: Plugin main file outside plugin directory");
                _logger.Error("  Plugin: {Plugin}", manifest.Name);
                _logger.Error("  Main path: {MainPath}", mainPath);
                _logger.Error("  Expected directory: {PluginDir}", pluginDir);
                return false;
            }

            // Validate dependency paths if they contain file references
            if (manifest.Dependencies != null)
            {
                foreach (var (key, value) in manifest.Dependencies)
                {
                    // Check for suspicious path patterns
                    if (key.Contains("..", StringComparison.OrdinalIgnoreCase) || key.Contains('~', StringComparison.OrdinalIgnoreCase) || value.Contains("..", StringComparison.OrdinalIgnoreCase) || value.Contains('~', StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warn("Suspicious path in manifest dependencies: {Key} = {Value}", key, value);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating manifest paths for {Plugin}", manifest.Name);
            return false;
        }
    }

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
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         ArgumentException)
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
        catch (Exception ex) when (ex is FileNotFoundException or
                                         FileLoadException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         IOException or
                                         SecurityException)
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
            _ = stream.Seek(60, SeekOrigin.Begin);
            var peHeaderOffset = reader.ReadInt32();

            if (peHeaderOffset >= stream.Length - 4)
            {
                return false;
            }

            // Read PE signature
            _ = stream.Seek(peHeaderOffset, SeekOrigin.Begin);
            var peSignature = reader.ReadUInt32();
            if (peSignature != 0x00004550) // "PE\0\0"
            {
                return false;
            }

            // Basic validation passed
            return true;
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         ObjectDisposedException)
        {
            _logger.Debug(ex, "Error checking PE format: {DllPath}", dllPath);
            return false;
        }
    }

    /// <summary>
    /// Gets pre-calculated SHA256 hashes for built-in plugins.
    /// Generated: 2025-11-17 22:16:14
    /// Note: SftpFileSystem.dll exists in both 'plugins' (x64) and 'pluginsx86' (x86) folders
    /// </summary>
    public static Dictionary<string, string> GetBuiltInPluginHashes ()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AutoColumnizer.dll"] = "F64B30FB8A4DF1C587DB12E98792F3F4179707420FDF88FE7C0DABC21A95BE81",
            ["CsvColumnizer.dll"] = "19B94F2396423345BD45A9ACA830E6608F824CC7D24287ED39C409AB1FE4F921",
            ["JsonColumnizer.dll"] = "9830A440DC43AAF8B765BA520B3CB157C38517FEF2BB6EAB251D8914A6813AF2",
            ["JsonCompactColumnizer.dll"] = "9E2E13DE6E130265DDE01D5BDE7D150F159A6B6736512FEA07193821608364CD",
            ["RegexColumnizer.dll"] = "780310BF53E9D1E91D900AC76E731DC951FB4740B4875B33ABDBA7734131F9F8",
            ["Log4jXmlColumnizer.dll"] = "EB2C2C764473BE3DAA5C263E82FD3BF0210E43818776DB888F1CE53054C74D90",
            ["GlassfishColumnizer.dll"] = "34C4823A8507A63CEE9B5F199FE4272DC72AA3053486DB44312408132BAB3D4E",
            ["DefaultPlugins.dll"] = "7351E7491BD038B07956A0BD3BCF259B4E5B016F9C3972984D19837B9892E3D5",
            ["FlashIconHighlighter.dll"] = "56C87943C9676BEBBD700BABFCB80D282C9D63A763503839D4048C1F981EA624",
            ["SftpFileSystem.dll"] = "B28133BA01B21419494203C6948C388FC7A511EA995EB9F8B20F8B84EAFD4B94",
            ["SftpFileSystem.dll (x86)"] = "963BD8000C35A04F06D9644240E8F600A93BB12ABDA49A8FBFA220644C1A49A8",
        };
    }

    #endregion
}
