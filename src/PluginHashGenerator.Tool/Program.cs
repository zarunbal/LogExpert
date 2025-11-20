using System.Globalization;
using System.Text;

using LogExpert.PluginRegistry;

namespace PluginHashGenerator.Tool;

/// <summary>
/// Console tool to generate plugin hashes and update the GetBuiltInPluginHashes() method.
/// Usage: PluginHashGenerator.Tool <output_path> <target_file> <configuration>
/// </summary>
internal class Program
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Tool for Hash Generation does not need localization")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally continue on error to process other plugins")]
    private static int Main (string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: PluginHashGenerator.Tool <output_path> <target_file> [configuration]");
                Console.Error.WriteLine("  output_path: Path to bin/{Configuration}/ directory");
                Console.Error.WriteLine("  target_file: Path to PluginHashGenerator.Generated.cs");
                Console.Error.WriteLine("  configuration: Build configuration (Debug/Release) - optional");
                return 1;
            }

            var outputPath = args[0];
            var targetFile = args[1];
            var configuration = args.Length > 2 ? args[2] : "Release";

            Console.WriteLine($"Generating plugin hashes from: {outputPath}");
            Console.WriteLine($"Target file: {targetFile}");
            Console.WriteLine($"Configuration: {configuration}");

            // Find all plugin DLLs
            var pluginPaths = new List<string>();

            // Check plugins folder
            var pluginsDir = Path.Join(outputPath, "plugins");
            if (Directory.Exists(pluginsDir))
            {
                pluginPaths.AddRange(Directory.GetFiles(pluginsDir, "*.dll"));
                Console.WriteLine($"Found {pluginPaths.Count} DLLs in plugins folder");
            }

            // Check pluginsx86 folder
            var pluginsx86Dir = Path.Join(outputPath, "pluginsx86");
            if (Directory.Exists(pluginsx86Dir))
            {
                var x86Plugins = Directory.GetFiles(pluginsx86Dir, "*.dll");
                Console.WriteLine($"Found {x86Plugins.Length} DLLs in pluginsx86 folder");
                pluginPaths.AddRange(x86Plugins);
            }

            if (pluginPaths.Count == 0)
            {
                Console.WriteLine("WARNING: No plugin DLLs found. Skipping hash generation.");
                return 0; // Not an error - plugins might not be built yet
            }

            // Filter to only actual plugins (exclude dependencies)
            var knownDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

            var pluginHashes = new Dictionary<string, string>();

            foreach (var pluginPath in pluginPaths.OrderBy(p => p))
            {
                var fileName = Path.GetFileName(pluginPath);

                // Skip dependencies
                if (knownDependencies.Contains(fileName))
                {
                    Console.WriteLine($"  Skipping dependency: {fileName}");
                    continue;
                }

                try
                {
                    var hash = PluginHashCalculator.CalculateHash(pluginPath);

                    // For x86 plugins, add suffix to distinguish them
                    var key = fileName;
                    if (pluginPath.Contains("pluginsx86", StringComparison.OrdinalIgnoreCase))
                    {
                        key = $"{Path.GetFileNameWithoutExtension(fileName)}.dll (x86)";
                    }

                    // Handle duplicate keys (same plugin in both folders)
                    if (!pluginHashes.ContainsKey(key))
                    {
                        pluginHashes[key] = hash;
                        Console.WriteLine($"  ✓ {key}: {hash[..16]}...");
                    }
                }
                catch (Exception ex)
                {
                    //Intentionally continue on error to process other plugins
                    Console.Error.WriteLine($"  ✗ ERROR calculating hash for {fileName}: {ex.Message}");
                }
            }

            // Generate the source code
            var sourceCode = GenerateSourceCode(pluginHashes, configuration);

            // Ensure target directory exists
            var targetDir = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                _ = Directory.CreateDirectory(targetDir);
            }

            // Write the file
            File.WriteAllText(targetFile, sourceCode);

            Console.WriteLine($"\n✓ Successfully generated plugin hashes ({pluginHashes.Count} plugins)");
            Console.WriteLine($"  File: {targetFile}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static string GenerateSourceCode (Dictionary<string, string> pluginHashes, string configuration)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        var sb = new StringBuilder();

        foreach (var kvp in pluginHashes.OrderBy(kvp => kvp.Key))
        {
            // Properly escape the key for C# string literal
            var escapedKey = kvp.Key.Replace("\\", "\\\\", StringComparison.OrdinalIgnoreCase).Replace("\"", "\\\"", StringComparison.OrdinalIgnoreCase);
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"            [\"{escapedKey}\"] = \"{kvp.Value}\",");
        }

        string sourceCode = $$"""
            // <auto-generated/>
            // This file is auto-generated during build. Do not edit manually.
            // To regenerate, rebuild the project or run the GeneratePluginHashes MSBuild target.

            using System.Collections.Generic;

            namespace LogExpert.PluginRegistry;

            public static partial class PluginValidator
            {
                /// <summary>
                /// Gets pre-calculated SHA256 hashes for built-in plugins.
                /// Generated: {{timestamp}} UTC
                /// Configuration: {{configuration}}
                /// Plugin count: {{pluginHashes.Count}}
                /// </summary>
                public static Dictionary<string, string> GetBuiltInPluginHashes()
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                         {{sb}}
                    };
                }
            }
            """;

        return sourceCode;
    }
}