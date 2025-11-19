namespace LogExpert.PluginRegistry;

/// <summary>
/// Provides user-friendly error messages for plugin operations.
/// </summary>
public static class PluginErrorMessages
{
    #region Validation Errors

    /// <summary>
    /// Gets an error message for when a plugin file is not found.
    /// </summary>
    public static string PluginFileNotFound (string pluginPath)
    {
        return $"Plugin file not found:\n\n{pluginPath}\n\n" +
               "Please verify the file exists and the path is correct.";
    }

    /// <summary>
    /// Gets an error message for when a plugin is not trusted.
    /// </summary>
    public static string PluginNotTrusted (string pluginName, string hash)
    {
        return $"Plugin '{pluginName}' is not in the trusted plugins list.\n\n" +
               $"Hash: {hash[..Math.Min(32, hash.Length)]}...\n\n" +
               "To trust this plugin:\n" +
               "1. Go to Options > Plugin Trust Management\n" +
               "2. Click 'Add Plugin' and select the plugin file\n" +
               "3. Confirm the trust operation\n" +
               "4. Restart LogExpert\n\n" +
               "Only trust plugins from sources you know and trust!";
    }

    /// <summary>
    /// Gets an error message for when a plugin's hash doesn't match the expected value.
    /// </summary>
    public static string PluginHashMismatch (string pluginName, string expectedHash, string actualHash)
    {
        return $"SECURITY ALERT: Plugin '{pluginName}' has been modified!\n\n" +
               $"Expected hash: {expectedHash[..Math.Min(32, expectedHash.Length)]}...\n" +
               $"Actual hash:   {actualHash[..Math.Min(32, actualHash.Length)]}...\n\n" +
               "This plugin file may have been tampered with or corrupted.\n\n" +
               "For your security:\n" +
               "• Do NOT load this plugin\n" +
               "• Download a fresh copy from a trusted source\n" +
               "• Scan your system for malware\n" +
               "• Remove the plugin from the trusted list if needed";
    }

    /// <summary>
    /// Gets an error message for when a plugin manifest is invalid.
    /// </summary>
    public static string InvalidManifest (string pluginName, List<string> errors)
    {
        var errorList = string.Join("\n• ", errors);
        return $"Plugin '{pluginName}' has an invalid manifest:\n\n" +
               $"• {errorList}\n\n" +
               "The plugin cannot be loaded. Contact the plugin developer for an updated version.";
    }

    /// <summary>
    /// Gets an error message for when a plugin manifest file is missing.
    /// </summary>
    public static string ManifestNotFound (string pluginName)
    {
        return $"Plugin '{pluginName}' is missing its manifest file.\n\n" +
               "Expected file: {pluginName}.manifest.json\n\n" +
               "The manifest file is required for security validation. " +
               "Contact the plugin developer or download the complete plugin package.";
    }

    /// <summary>
    /// Gets an error message for path traversal attempts.
    /// </summary>
    public static string PathTraversalDetected (string pluginName, string suspiciousPath)
    {
        return $"SECURITY: Plugin '{pluginName}' attempted to access files outside its directory.\n\n" +
               $"Suspicious path: {suspiciousPath}\n\n" +
               "This plugin has been blocked for your security. " +
               "Only trust plugins from verified sources.";
    }

    #endregion

    #region Loading Errors

    /// <summary>
    /// Gets an error message for when a plugin assembly cannot be loaded.
    /// </summary>
    public static string AssemblyLoadFailed (string pluginName, string reason)
    {
        return $"Failed to load plugin '{pluginName}':\n\n{reason}\n\n" +
               "Possible causes:\n" +
               "• Missing dependencies (DLL files)\n" +
               "• Incorrect .NET version (requires .NET 8 or later)\n" +
               "• Corrupted plugin file\n" +
               "• Architecture mismatch (x86 vs x64)\n\n" +
               "Try reinstalling the plugin or contact the developer.";
    }

    /// <summary>
    /// Gets an error message for bad image format exceptions.
    /// </summary>
    public static string BadImageFormat (string pluginName, bool is64BitProcess)
    {
        var architecture = is64BitProcess ? "64-bit (x64)" : "32-bit (x86)";
        var requiredArchitecture = is64BitProcess ? "x64" : "x86";

        return $"Plugin '{pluginName}' has an incompatible format.\n\n" +
               $"LogExpert is running as: {architecture}\n" +
               $"Plugin must be compiled for: {requiredArchitecture}\n\n" +
               "Download the correct version of the plugin for your system architecture.";
    }

    /// <summary>
    /// Gets an error message for missing dependencies.
    /// </summary>
    public static string MissingDependency (string pluginName, string dependencyName)
    {
        return $"Plugin '{pluginName}' requires '{dependencyName}' which is missing.\n\n" +
               "To fix this:\n" +
               "1. Download the complete plugin package\n" +
               "2. Ensure all DLL files are in the plugins folder\n" +
               "3. Restart LogExpert\n\n" +
               "Contact the plugin developer if the problem persists.";
    }

    /// <summary>
    /// Gets an error message for plugin load timeout.
    /// </summary>
    public static string PluginLoadTimeout (string pluginName, int timeoutSeconds)
    {
        return $"Plugin '{pluginName}' took too long to load (>{timeoutSeconds} seconds).\n\n" +
               "The plugin may be:\n" +
               "• Performing complex initialization\n" +
               "• Stuck in an infinite loop\n" +
               "• Waiting for network resources\n\n" +
               "The plugin has been skipped. Contact the plugin developer if this continues.";
    }

    /// <summary>
    /// Gets an error message for plugin instantiation failure.
    /// </summary>
    public static string InstantiationFailed (string pluginName, string typeName)
    {
        return $"Failed to create an instance of plugin '{pluginName}'.\n\n" +
               $"Type: {typeName}\n\n" +
               "The plugin class may:\n" +
               "• Be missing a parameterless constructor\n" +
               "• Have constructor code that throws exceptions\n" +
               "• Require initialization parameters\n\n" +
               "Contact the plugin developer for assistance.";
    }

    #endregion

    #region Version Compatibility Errors

    /// <summary>
    /// Gets an error message for version incompatibility.
    /// </summary>
    public static string VersionIncompatible (string pluginName, string pluginVersion, string requiredVersion, string currentVersion)
    {
        return $"Plugin '{pluginName}' v{pluginVersion} is not compatible with this version of LogExpert.\n\n" +
               $"Plugin requires: LogExpert {requiredVersion}\n" +
               $"You have: LogExpert {currentVersion}\n\n" +
               "Options:\n" +
               "• Update LogExpert to the required version\n" +
               "• Find a compatible version of the plugin\n" +
               "• Contact the plugin developer";
    }

    /// <summary>
    /// Gets an error message for .NET version incompatibility.
    /// </summary>
    public static string DotNetVersionIncompatible (string pluginName, string requiredVersion)
    {
        return $"Plugin '{pluginName}' requires .NET {requiredVersion}.\n\n" +
               "Your system may not have the required .NET runtime installed.\n\n" +
               "Download and install the required .NET version from:\n" +
               "https://dotnet.microsoft.com/download";
    }

    #endregion

    #region Configuration Errors

    /// <summary>
    /// Gets an error message for configuration load failure.
    /// </summary>
    public static string ConfigLoadFailed (string pluginName)
    {
        return $"Failed to load configuration for plugin '{pluginName}'.\n\n" +
               "The plugin will use default settings.\n\n" +
               "If this is a new installation, this is normal. " +
               "Configuration will be created when you save settings.";
    }

    /// <summary>
    /// Gets an error message for configuration save failure.
    /// </summary>
    public static string ConfigSaveFailed (string pluginName, string reason)
    {
        return $"Failed to save configuration for plugin '{pluginName}':\n\n{reason}\n\n" +
               "Check that:\n" +
               "• You have write permissions to the config folder\n" +
               "• Disk is not full\n" +
               "• Config file is not locked by another application";
    }

    /// <summary>
    /// Gets an error message for trust configuration errors.
    /// </summary>
    public static string TrustConfigError (string reason)
    {
        return $"Failed to load or save plugin trust configuration:\n\n{reason}\n\n" +
               "Using default configuration. " +
               "Only built-in plugins will be trusted until this is resolved.";
    }

    #endregion

    #region Permission Errors

    /// <summary>
    /// Gets an error message for insufficient permissions.
    /// </summary>
    public static string InsufficientPermissions (string pluginName, string requiredPermission)
    {
        return $"Plugin '{pluginName}' requires permission '{requiredPermission}' which is not granted.\n\n" +
               "The plugin cannot function without this permission.\n\n" +
               "To grant this permission:\n" +
               "1. Check the plugin manifest for required permissions\n" +
               "2. Update the plugin permissions configuration\n" +
               "3. Restart LogExpert";
    }

    /// <summary>
    /// Gets an error message for denied user-added plugins.
    /// </summary>
    public static string UserPluginsNotAllowed ()
    {
        return "User-added trusted plugins are not allowed by policy.\n\n" +
               "Your system administrator has restricted plugin installation.\n\n" +
               "Contact your IT department if you need to use additional plugins.";
    }

    #endregion

    #region Summary Messages

    /// <summary>
    /// Gets a summary message for plugin loading results.
    /// </summary>
    public static string LoadingSummary (int loaded, int skipped, int failed, int total)
    {
        return $"Plugin Loading Complete:\n\n" +
               $"• Total plugins found: {total}\n" +
               $"• Successfully loaded: {loaded}\n" +
               $"• Skipped: {skipped}\n" +
               $"• Failed: {failed}\n\n" +
               (failed > 0
                   ? "Check the log file for details about failed plugins."
                   : "All plugins loaded successfully!");
    }

    /// <summary>
    /// Gets a warning message for no plugins loaded.
    /// </summary>
    public static string NoPluginsLoaded ()
    {
        return "No plugins were loaded.\n\n" +
               "This could mean:\n" +
               "• No plugin files in the plugins folder\n" +
               "• All plugins failed security validation\n" +
               "• Plugins directory not found\n\n" +
               "LogExpert will continue with built-in functionality only.";
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets a generic error message with exception details.
    /// </summary>
    public static string GenericError (string operation, string pluginName, Exception ex)
    {
        return $"An error occurred during {operation} for plugin '{pluginName}':\n\n" +
               $"{ex.GetType().Name}: {ex.Message}\n\n" +
               "See the log file for technical details.";
    }

    /// <summary>
    /// Formats an exception for display to users.
    /// </summary>
    public static string FormatException (Exception ex)
    {
        if (ex is AggregateException aggEx)
        {
            var innerMessages = aggEx.InnerExceptions
                .Select(e => $"• {e.GetType().Name}: {e.Message}")
                .ToList();
            return string.Join("\n", innerMessages);
        }

        return $"{ex.GetType().Name}: {ex.Message}";
    }

    #endregion
}
