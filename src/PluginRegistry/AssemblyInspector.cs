using System.Reflection;

using NLog;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Inspects assemblies to determine which plugin types they contain without fully loading them.
/// This enables intelligent decisions about lazy loading vs. immediate loading.
/// </summary>
public static class AssemblyInspector
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Inspects an assembly to determine which plugin types it contains.
    /// </summary>
    /// <param name="dllPath">Path to the DLL to inspect</param>
    /// <returns>Information about plugin types in the assembly</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally Catch all")]
    public static PluginTypeInfo InspectAssembly (string dllPath)
    {
        var info = new PluginTypeInfo();

        if (string.IsNullOrWhiteSpace(dllPath))
        {
            _logger.Warn("Cannot inspect assembly: path is null or empty");
            return info;
        }

        if (!File.Exists(dllPath))
        {
            _logger.Warn("Cannot inspect assembly: file not found at {Path}", dllPath);
            return info;
        }

        try
        {
            _logger.Debug("Inspecting assembly: {FileName}", Path.GetFileName(dllPath));

            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Skip abstract classes and interfaces - they can't be instantiated
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                var interfaces = type.GetInterfaces();

                // Check for each plugin interface type
                if (interfaces.Any(i => i.FullName == typeof(ILogLineColumnizer).FullName))
                {
                    info.HasColumnizer = true;
                    _logger.Debug("  Found ILogLineColumnizer: {TypeName}", type.Name);
                }

                if (interfaces.Any(i => i.FullName == typeof(IFileSystemPlugin).FullName))
                {
                    info.HasFileSystem = true;
                    _logger.Debug("  Found IFileSystemPlugin: {TypeName}", type.Name);
                }

                if (interfaces.Any(i => i.FullName == typeof(IContextMenuEntry).FullName))
                {
                    info.HasContextMenu = true;
                    _logger.Debug("  Found IContextMenuEntry: {TypeName}", type.Name);
                }

                if (interfaces.Any(i => i.FullName == typeof(IKeywordAction).FullName))
                {
                    info.HasKeywordAction = true;
                    _logger.Debug("  Found IKeywordAction: {TypeName}", type.Name);
                }
            }

            _logger.Info("Inspected {FileName}: Columnizer={Col}, FileSystem={FS}, ContextMenu={CM}, KeywordAction={KA}, TypeCount={Count}",
                Path.GetFileName(dllPath),
                info.HasColumnizer,
                info.HasFileSystem,
                info.HasContextMenu,
                info.HasKeywordAction,
                info.TypeCount);

            return info;
        }
        catch (BadImageFormatException ex)
        {
            _logger.Debug(ex, "Assembly {FileName} is not a valid .NET assembly", Path.GetFileName(dllPath));
            return new PluginTypeInfo(); // Empty info = not a plugin assembly
        }
        catch (ReflectionTypeLoadException ex)
        {
            _logger.Warn(ex, "Failed to load types from assembly {FileName}. Some types may be missing dependencies.", Path.GetFileName(dllPath));

            // Try to get type info from successfully loaded types
            if (ex.Types != null)
            {
                foreach (var type in ex.Types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    try
                    {
                        var interfaces = type.GetInterfaces();
                        if (interfaces.Any(i => i.FullName == typeof(ILogLineColumnizer).FullName))
                        {
                            info.HasColumnizer = true;
                        }

                        if (interfaces.Any(i => i.FullName == typeof(IFileSystemPlugin).FullName))
                        {
                            info.HasFileSystem = true;
                        }

                        if (interfaces.Any(i => i.FullName == typeof(IContextMenuEntry).FullName))
                        {
                            info.HasContextMenu = true;
                        }

                        if (interfaces.Any(i => i.FullName == typeof(IKeywordAction).FullName))
                        {
                            info.HasKeywordAction = true;
                        }
                    }
                    catch
                    {
                        // Skip types that fail to load
                    }
                }
            }

            return info;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to inspect assembly: {FileName}", Path.GetFileName(dllPath));
            // Return empty info - will trigger direct loading as fallback
            return new PluginTypeInfo();
        }
    }

    /// <summary>
    /// Checks if an assembly is likely a plugin assembly based on file name patterns.
    /// This is a quick heuristic check before full inspection.
    /// </summary>
    /// <param name="dllPath">Path to the DLL</param>
    /// <returns>True if the assembly might be a plugin</returns>
    public static bool IsLikelyPluginAssembly (string dllPath)
    {
        if (string.IsNullOrWhiteSpace(dllPath))
        {
            return false;
        }

        var fileName = Path.GetFileNameWithoutExtension(dllPath);

        // Common plugin naming patterns
        var pluginPatterns = new[]
        {
            "Columnizer",
            "Plugin",
            "FileSystem",
            "Highlighter"
        };

        return pluginPatterns.Any(pattern => fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
