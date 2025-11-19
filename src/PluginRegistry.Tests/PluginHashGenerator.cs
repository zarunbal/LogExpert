using System.Globalization;

using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.Tests;

/// <summary>
/// Utility class to generate SHA256 hashes for all built-in plugins.
/// Run this test to generate the GetBuiltInPluginHashes() method code.
/// </summary>
[TestFixture]
public class PluginHashGenerator
{
    private static readonly Dictionary<string, string> _builtInPlugins = new()
    {
        // Plugins in the main 'plugins' folder
        ["AutoColumnizer.dll"] = "plugins",
        ["CsvColumnizer.dll"] = "plugins",
        ["JsonColumnizer.dll"] = "plugins",
        ["JsonCompactColumnizer.dll"] = "plugins",
        ["RegexColumnizer.dll"] = "plugins",
        ["Log4jXmlColumnizer.dll"] = "plugins",
        ["GlassfishColumnizer.dll"] = "plugins",
        ["DefaultPlugins.dll"] = "plugins",
        ["FlashIconHighlighter.dll"] = "plugins",

        // SFTP plugin (x64) in plugins folder
        ["SftpFileSystem.dll"] = "plugins",

        // SFTP plugin (x86) in pluginsx86 folder - same DLL name, different folder
        ["SftpFileSystem.dll (x86)"] = "pluginsx86"
    };

    [Test]
    [Explicit("Run manually to generate plugin hashes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit test")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit test")]
    public void GenerateBuiltInPluginHashes ()
    {
        // Try multiple possible base directories
        var possibleBaseDirectories = new[]
        {
            AppDomain.CurrentDomain.BaseDirectory,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."),
        };

        string baseDirectory = null;
        foreach (var dir in possibleBaseDirectories)
        {
            try
            {
                var fullPath = Path.GetFullPath(dir);
                var pluginsPath = Path.Combine(fullPath, "bin", "Debug", "plugins");
                var pluginsx86Path = Path.Combine(fullPath, "bin", "Debug", "pluginsx86");

                if (Directory.Exists(pluginsPath) || Directory.Exists(pluginsx86Path))
                {
                    baseDirectory = fullPath;
                    break;
                }

                // Try Release build
                pluginsPath = Path.Combine(fullPath, "bin", "Release", "plugins");
                pluginsx86Path = Path.Combine(fullPath, "bin", "Release", "pluginsx86");

                if (Directory.Exists(pluginsPath) || Directory.Exists(pluginsx86Path))
                {
                    baseDirectory = fullPath;
                    break;
                }
            }
            catch
            {
                // Ignore invalid paths
            }
        }

        Assert.That(baseDirectory, Is.Not.Null,
            "Could not find base directory with plugins folders. Please build LogExpert first.");

        // Determine which build configuration exists (Debug or Release)
        var configuration = "Debug";
        var pluginsDir = Path.Combine(baseDirectory, "bin", configuration, "plugins");
        if (!Directory.Exists(pluginsDir))
        {
            configuration = "Release";
            pluginsDir = Path.Combine(baseDirectory, "bin", configuration, "plugins");
        }

        Console.WriteLine($"Base Directory: {baseDirectory}");
        Console.WriteLine($"Configuration: {configuration}");
        Console.WriteLine($"Plugins Directory: {pluginsDir}");
        Console.WriteLine($"PluginsX86 Directory: {Path.Combine(baseDirectory, "bin", configuration, "pluginsx86")}");
        Console.WriteLine("");

        var hashes = new Dictionary<string, string>();
        var foundCount = 0;
        var missingCount = 0;

        foreach (var plugin in _builtInPlugins)
        {
            var pluginKey = plugin.Key;
            var pluginSubfolder = plugin.Value;

            // Extract actual filename (remove platform suffix if present)
            var pluginName = pluginKey.Contains(" (x86)", StringComparison.OrdinalIgnoreCase) ? pluginKey.Replace(" (x86)", "", StringComparison.OrdinalIgnoreCase) : pluginKey;
            var pluginPath = Path.Combine(baseDirectory, "bin", configuration, pluginSubfolder, pluginName);

            if (File.Exists(pluginPath))
            {
                try
                {
                    var hash = PluginHashCalculator.CalculateHash(pluginPath);
                    hashes[pluginKey] = hash;
                    Console.WriteLine($"✓ {pluginKey} ({pluginSubfolder})");
                    Console.WriteLine($"  {hash[..32]}...");
                    foundCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ {pluginKey}: ERROR - {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"⚠ {pluginKey} ({pluginSubfolder}): FILE NOT FOUND at {pluginPath}");
                missingCount++;
            }
        }

        Console.WriteLine("");
        Console.WriteLine($"Summary: {foundCount} found, {missingCount} missing");
        Console.WriteLine("");

        // Generate C# code
        if (hashes.Count > 0)
        {
            Console.WriteLine("// ===== Copy this method into PluginValidator.cs =====");
            Console.WriteLine("");
            Console.WriteLine("/// <summary>");
            Console.WriteLine("/// Gets pre-calculated SHA256 hashes for built-in plugins.");
            Console.WriteLine("/// Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo));
            Console.WriteLine("/// Note: SftpFileSystem.dll exists in both 'plugins' (x64) and 'pluginsx86' (x86) folders");
            Console.WriteLine("/// </summary>");
            Console.WriteLine("private static Dictionary<string, string> GetBuiltInPluginHashes()");
            Console.WriteLine("{");
            Console.WriteLine("    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)");
            Console.WriteLine("    {");

            foreach (var entry in hashes)
            {
                Console.WriteLine($"        [\"{entry.Key}\"] = \"{entry.Value}\",");
            }

            Console.WriteLine("    };");
            Console.WriteLine("}");
            Console.WriteLine("");
            Console.WriteLine("// ===== End of generated code =====");
        }

        Assert.That(foundCount, Is.EqualTo(_builtInPlugins.Count), $"Expected {_builtInPlugins.Count} plugins, found {foundCount}");
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit test")]
    public void VerifyAllPluginsHaveHashes ()
    {
        // Arrange
        var builtInHashes = PluginValidator.GetBuiltInPluginHashes();

        // Act & Assert - Verify that GetBuiltInPluginHashes() returns data
        Assert.That(builtInHashes, Is.Not.Null, "GetBuiltInPluginHashes() should not return null");
        Assert.That(builtInHashes.Count, Is.GreaterThan(0), "GetBuiltInPluginHashes() should return at least one hash");

        // Verify all built-in plugins have hashes
        var missingHashes = new List<string>();
        var foundHashes = new List<string>();

        foreach (var pluginKey in _builtInPlugins.Keys)
        {
            if (builtInHashes.TryGetValue(pluginKey, out string? hash))
            {
                foundHashes.Add(pluginKey);
                Assert.That(hash, Is.Not.Null.And.Not.Empty, $"Hash for {pluginKey} should not be null or empty");

                // Verify hash looks like a valid SHA256 (64 hex characters)
                Assert.That(hash, Has.Length.EqualTo(64), $"Hash for {pluginKey} should be 64 characters (SHA256)");
                Assert.That(hash, Does.Match("^[A-Fa-f0-9]{64}$"), $"Hash for {pluginKey} should be valid hexadecimal");
            }
            else
            {
                missingHashes.Add(pluginKey);
            }
        }

        // Report findings
        Console.WriteLine($"  Verification Results:");
        Console.WriteLine($"  Total plugins: {_builtInPlugins.Count}");
        Console.WriteLine($"  Plugins with hashes: {foundHashes.Count}");
        Console.WriteLine($"  Missing hashes: {missingHashes.Count}");
        Console.WriteLine();

        if (foundHashes.Count > 0)
        {
            Console.WriteLine("✓ Plugins with hashes:");
            foreach (var plugin in foundHashes)
            {
                var hash = builtInHashes[plugin];
                Console.WriteLine($"  - {plugin}: {hash[..16]}...");
            }
            Console.WriteLine();
        }

        if (missingHashes.Count > 0)
        {
            Console.WriteLine("✗ Plugins missing hashes:");
            foreach (var plugin in missingHashes)
            {
                Console.WriteLine($"  - {plugin}");
            }

            Console.WriteLine();
            Console.WriteLine("Run GenerateBuiltInPluginHashes() test to generate missing hashes.");
        }

        // Final assertion
        Assert.That(missingHashes, Is.Empty, $"All {_builtInPlugins.Count} built-in plugins should have hashes. Missing: {string.Join(", ", missingHashes)}");
    }
}
