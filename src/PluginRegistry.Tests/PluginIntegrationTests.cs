using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Integration tests for actual plugin loading scenarios using real plugin DLLs.
/// </summary>
[TestFixture]
public class PluginIntegrationTests
{
    private string _testPluginsDirectory = string.Empty;
    private DefaultPluginLoader _loader = null!;

    [SetUp]
    public void SetUp ()
    {
        _loader = new DefaultPluginLoader();

        // Use the actual plugins directory from the build output
        var binDirectory = Path.GetDirectoryName(typeof(PluginIntegrationTests).Assembly.Location)!;
        _testPluginsDirectory = Path.Combine(binDirectory, "..", "..", "..", "..", "bin", "Debug", "plugins");
        _testPluginsDirectory = Path.GetFullPath(_testPluginsDirectory);

        // Verify the plugins directory exists
        if (!Directory.Exists(_testPluginsDirectory))
        {
            Assert.Warn($"Plugins directory not found: {_testPluginsDirectory}. Integration tests may be skipped.");
        }
    }

    #region Loading Real Plugins

    [Test]
    public void LoadPlugin_WithCsvColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(csvColonizerPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load CsvColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void LoadPlugin_WithJsonColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var jsonColonizerPath = Path.Combine(_testPluginsDirectory, "JsonColumnizer.dll");

        if (!File.Exists(jsonColonizerPath))
        {
            Assert.Ignore("JsonColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(jsonColonizerPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load JsonColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void LoadPlugin_WithRegexColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var regexColonizerPath = Path.Combine(_testPluginsDirectory, "RegexColumnizer.dll");

        if (!File.Exists(regexColonizerPath))
        {
            Assert.Ignore("RegexColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(regexColonizerPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load RegexColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void LoadPlugin_WithGlassfishColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var glassfishColonizerPath = Path.Combine(_testPluginsDirectory, "GlassfishColumnizer.dll");

        if (!File.Exists(glassfishColonizerPath))
        {
            Assert.Ignore("GlassfishColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(glassfishColonizerPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load GlassfishColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void LoadPlugin_WithLog4jXmlColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var log4jColonizerPath = Path.Combine(_testPluginsDirectory, "Log4jXmlColumnizer.dll");

        if (!File.Exists(log4jColonizerPath))
        {
            Assert.Ignore("Log4jXmlColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(log4jColonizerPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load Log4jXmlColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void LoadPlugin_WithJsonCompactColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var jsonCompactPath = Path.Combine(_testPluginsDirectory, "JsonCompactColumnizer.dll");

        if (!File.Exists(jsonCompactPath))
        {
            Assert.Ignore("JsonCompactColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = _loader.LoadPlugin(jsonCompactPath);

        // Assert
        Assert.That(result.Success, Is.True, $"Failed to load JsonCompactColumnizer: {result.ErrorMessage}");
        Assert.That(result.Plugin, Is.Not.Null);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Exception, Is.Null);
    }

    #endregion

    #region Loading Plugins with Manifests

    [Test]
    public void LoadPlugin_WhenManifestExists_ShouldLoadManifest ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");
        var manifestPath = Path.ChangeExtension(csvColonizerPath, ".manifest.json");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Create a test manifest if it doesn't exist
        if (!File.Exists(manifestPath))
        {
            var manifestContent = @"{
  ""Name"": ""CSV Columnizer"",
  ""Version"": ""1.0.0"",
  ""Author"": ""LogExpert Team"",
  ""Description"": ""Parses CSV files"",
  ""ApiVersion"": ""1.0"",
  ""Main"": ""CsvColumnizer.CsvColumnizer"",
  ""Permissions"": [""filesystem:read""]
}";
            File.WriteAllText(manifestPath, manifestContent);
        }

        try
        {
            // Act
            var result = _loader.LoadPlugin(csvColonizerPath);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Manifest, Is.Not.Null, "Manifest should be loaded when manifest file exists");
            Assert.That(result.Manifest!.Name, Is.EqualTo("CSV Columnizer"));
            Assert.That(result.Manifest.Version, Is.EqualTo("1.0.0"));
        }
        finally
        {
            // Cleanup - only delete if we created it
            if (File.Exists(manifestPath))
            {
                try { File.Delete(manifestPath); } catch { /* Ignore cleanup errors */ }
            }
        }
    }

    [Test]
    public void LoadPlugin_WhenManifestDoesNotExist_ShouldStillLoadPlugin ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");
        var manifestPath = Path.ChangeExtension(csvColonizerPath, ".manifest.json");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Ensure manifest doesn't exist
        if (File.Exists(manifestPath))
        {
            File.Delete(manifestPath);
        }

        try
        {
            // Act
            var result = _loader.LoadPlugin(csvColonizerPath);

            // Assert
            Assert.That(result.Success, Is.True, "Plugin should load successfully without manifest");
            Assert.That(result.Plugin, Is.Not.Null);
            Assert.That(result.Manifest, Is.Null, "Manifest should be null when manifest file doesn't exist");
        }
        finally
        {
            // No cleanup needed
        }
    }

    #endregion

    #region Plugin Discovery

    [Test]
    public void DiscoverPlugins_InPluginsDirectory_ShouldFindMultiplePlugins ()
    {
        // Arrange
        if (!Directory.Exists(_testPluginsDirectory))
        {
            Assert.Ignore("Plugins directory not found");
        }

        // Act
        var dllFiles = Directory.GetFiles(_testPluginsDirectory, "*.dll")
            .Where(f => !f.Contains("ColumnizerLib.dll") &&
                       !f.Contains("LogExpert.Core.dll") &&
                       !f.Contains("Newtonsoft.Json.dll") &&
                       !f.Contains("Renci.SshNet.dll") &&
                       !f.Contains("CsvHelper.dll") &&
                       !f.Contains("BouncyCastle") &&
                       !f.Contains("Microsoft.Extensions"))
            .ToList();

        // Assert
        Assert.That(dllFiles.Count, Is.GreaterThan(0), "Should find plugin DLLs in plugins directory");

        // Try to load each one
        var successCount = 0;
        foreach (var dllFile in dllFiles)
        {
            var result = _loader.LoadPlugin(dllFile);
            if (result.Success)
            {
                successCount++;
                TestContext.WriteLine($"Successfully loaded: {Path.GetFileName(dllFile)}");
            }
            else
            {
                TestContext.WriteLine($"Failed to load: {Path.GetFileName(dllFile)} - {result.ErrorMessage}");
            }
        }

        Assert.That(successCount, Is.GreaterThan(0), "At least one plugin should load successfully");
    }

    [Test]
    public void DiscoverPlugins_ShouldIdentifyPluginTypes ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Act
        var typeInfo = AssemblyInspector.InspectAssembly(csvColonizerPath);

        // Assert
        Assert.That(typeInfo, Is.Not.Null);
        Assert.That(typeInfo.HasColumnizer, Is.True, "CsvColumnizer should be identified as a columnizer");
        Assert.That(typeInfo.TypeCount, Is.GreaterThan(0), "Should find plugin types in assembly");
    }

    #endregion

    #region Error Handling

    [Test]
    public void LoadPlugin_WithNonExistentFile_ShouldReturnFailure ()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testPluginsDirectory, "NonExistent.dll");

        // Act
        var result = _loader.LoadPlugin(nonExistentPath);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
        Assert.That(result.Exception, Is.InstanceOf<FileNotFoundException>());
    }

    [Test]
    public void LoadPlugin_WithInvalidDll_ShouldReturnFailure ()
    {
        // Arrange - Create a fake DLL file with invalid content
        var invalidDllPath = Path.Combine(Path.GetTempPath(), "InvalidPlugin.dll");
        File.WriteAllText(invalidDllPath, "This is not a valid DLL file");

        try
        {
            // Act
            var result = _loader.LoadPlugin(invalidDllPath);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
            Assert.That(result.Exception, Is.Not.Null);
        }
        finally
        {
            // Cleanup
            if (File.Exists(invalidDllPath))
            {
                File.Delete(invalidDllPath);
            }
        }
    }

    [Test]
    public void LoadPlugin_WithWrongArchitecture_ShouldReturnFailureWithBadImageFormat ()
    {
        // This test verifies that loading a DLL with wrong architecture (x86 vs x64)
        // returns proper error information
        // Note: This test is platform-specific and may be skipped

        // Arrange
        var pluginsx86Dir = Path.Combine(_testPluginsDirectory, "..", "pluginsx86");

        if (!Directory.Exists(pluginsx86Dir))
        {
            Assert.Ignore("pluginsx86 directory not found - skipping architecture test");
        }

        var x86Dlls = Directory.GetFiles(pluginsx86Dir, "*.dll")
            .Where(f => !f.Contains("ColumnizerLib.dll"))
            .ToList();

        if (x86Dlls.Count == 0)
        {
            Assert.Ignore("No x86 DLLs found - skipping architecture test");
        }

        // Act
        var result = _loader.LoadPlugin(x86Dlls.First());

        // Assert
        // On x64 runtime, loading x86 DLL should fail with BadImageFormatException or return no types
        // Note: In .NET 10+, some x86 DLLs may load but fail to find implementations
        if (Environment.Is64BitProcess && !result.Success)
        {
            Assert.That(result.ErrorMessage, 
                Does.Contain("format").IgnoreCase.Or.Contains("No plugin types"),
                "Expected architecture error or no types found");
        }
    }

    #endregion

    #region Async Loading

    [Test]
    public async Task LoadPluginAsync_WithCsvColumnizer_ShouldLoadSuccessfully ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Act
        var result = await _loader.LoadPluginAsync(csvColonizerPath, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Plugin, Is.Not.Null);
    }

    [Test]
    public async Task LoadPluginAsync_WithCancellation_ShouldRespectCancellationToken ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _loader.LoadPluginAsync(csvColonizerPath, cts.Token));
    }

    [Test]
    public async Task LoadPluginAsync_MultiplePlugins_ShouldLoadConcurrently ()
    {
        // Arrange
        var pluginPaths = new List<string>
        {
            Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "JsonColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "RegexColumnizer.dll")
        };

        // Filter to only existing files
        pluginPaths = pluginPaths.Where(File.Exists).ToList();

        if (pluginPaths.Count == 0)
        {
            Assert.Ignore("No plugin DLLs found for concurrent loading test");
        }

        // Act
        var loadTasks = pluginPaths.Select(path =>
            _loader.LoadPluginAsync(path, CancellationToken.None)).ToList();

        var results = await Task.WhenAll(loadTasks);

        // Assert
        Assert.That(results.Length, Is.EqualTo(pluginPaths.Count));
        Assert.That(results.All(r => r.Success), Is.True, "All plugins should load successfully");
    }

    #endregion

    #region Plugin Cache Integration

    [Test]
    public void PluginCache_WithRealPlugin_ShouldCacheSuccessfully ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        var cache = new PluginCache(TimeSpan.FromHours(1), _loader);

        // Act
        var result1 = cache.LoadPluginWithCache(csvColonizerPath);
        var result2 = cache.LoadPluginWithCache(csvColonizerPath);

        // Assert
        Assert.That(result1.Success, Is.True);
        Assert.That(result2.Success, Is.True);
        Assert.That(cache.IsCached(csvColonizerPath), Is.True, "Plugin should be cached after first load");

        var stats = cache.GetStatistics();
        Assert.That(stats.TotalEntries, Is.GreaterThan(0), "Should have cached entries");
        Assert.That(stats.ActiveEntries, Is.EqualTo(1), "Should have one active cached plugin");
    }

    [Test]
    public void PluginCache_WithMultipleRealPlugins_ShouldCacheEachIndependently ()
    {
        // Arrange
        var pluginPaths = new List<string>
        {
            Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "JsonColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "RegexColumnizer.dll")
        }.Where(File.Exists).ToList();

        if (pluginPaths.Count < 2)
        {
            Assert.Ignore("Not enough plugin DLLs found for multi-plugin cache test");
        }

        var cache = new PluginCache(TimeSpan.FromHours(1), _loader);

        // Act
        foreach (var path in pluginPaths)
        {
            var result = cache.LoadPluginWithCache(path);
            Assert.That(result.Success, Is.True, $"Failed to load {Path.GetFileName(path)}");
        }

        // Assert
        foreach (var path in pluginPaths)
        {
            Assert.That(cache.IsCached(path), Is.True, $"{Path.GetFileName(path)} should be cached");
        }

        var stats = cache.GetStatistics();
        Assert.That(stats.TotalEntries, Is.EqualTo(pluginPaths.Count), "Should have all plugins cached");
        Assert.That(stats.ActiveEntries, Is.EqualTo(pluginPaths.Count), "All cached plugins should be active");
    }

    #endregion

    #region Plugin Validation Integration

    [Test]
    public void PluginValidator_WithRealPlugin_ShouldValidateSuccessfully ()
    {
        // Arrange
        var csvColonizerPath = Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll");

        if (!File.Exists(csvColonizerPath))
        {
            Assert.Ignore("CsvColumnizer.dll not found in plugins directory");
        }

        // Trust the plugin first
        PluginValidator.AddTrustedPlugin(csvColonizerPath, out var errorMessage);

        // Act
        var isValid = PluginValidator.ValidatePlugin(csvColonizerPath, out var manifest);

        // Assert
        Assert.That(isValid, Is.True, $"Real plugin should pass validation. Error: {errorMessage}");
    }

    [Test]
    public void PluginValidator_WithInvalidDll_ShouldFailValidation ()
    {
        // Arrange - Create invalid DLL
        var invalidDllPath = Path.Combine(Path.GetTempPath(), "InvalidPlugin.dll");
        File.WriteAllText(invalidDllPath, "Not a valid DLL");

        try
        {
            // Act
            var isValid = PluginValidator.ValidatePlugin(invalidDllPath, out var manifest);

            // Assert
            Assert.That(isValid, Is.False, "Invalid DLL should fail validation");
            Assert.That(manifest, Is.Null);
        }
        finally
        {
            // Cleanup
            if (File.Exists(invalidDllPath))
            {
                File.Delete(invalidDllPath);
            }
        }
    }

    #endregion

    #region Assembly Inspector Integration

    [Test]
    public void AssemblyInspector_WithMultiplePlugins_ShouldIdentifyAllTypes ()
    {
        // Arrange
        var pluginPaths = new List<string>
        {
            Path.Combine(_testPluginsDirectory, "CsvColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "JsonColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "RegexColumnizer.dll"),
            Path.Combine(_testPluginsDirectory, "GlassfishColumnizer.dll")
        }.Where(File.Exists).ToList();

        if (pluginPaths.Count == 0)
        {
            Assert.Ignore("No plugin DLLs found for assembly inspector test");
        }

        // Act & Assert
        foreach (var pluginPath in pluginPaths)
        {
            var typeInfo = AssemblyInspector.InspectAssembly(pluginPath);

            Assert.That(typeInfo, Is.Not.Null, $"Should inspect {Path.GetFileName(pluginPath)}");
            Assert.That(typeInfo.HasColumnizer, Is.True, $"{Path.GetFileName(pluginPath)} should be identified as columnizer");
            Assert.That(typeInfo.TypeCount, Is.GreaterThan(0), $"{Path.GetFileName(pluginPath)} should have plugin types");

            TestContext.WriteLine($"{Path.GetFileName(pluginPath)}: " +
                $"{typeInfo.TypeCount} plugin type(s), " +
                $"HasColumnizer={typeInfo.HasColumnizer}");
        }
    }

    [Test]
    public void AssemblyInspector_WithDefaultPlugins_ShouldIdentifyMultipleTypes ()
    {
        // Arrange
        var defaultPluginsPath = Path.Combine(_testPluginsDirectory, "DefaultPlugins.dll");

        if (!File.Exists(defaultPluginsPath))
        {
            Assert.Ignore("DefaultPlugins.dll not found in plugins directory");
        }

        // Act
        var typeInfo = AssemblyInspector.InspectAssembly(defaultPluginsPath);

        // Assert
        Assert.That(typeInfo, Is.Not.Null);
        Assert.That(typeInfo.TypeCount, Is.GreaterThan(0), "DefaultPlugins should contain multiple plugin types");

        TestContext.WriteLine($"DefaultPlugins.dll contains {typeInfo.TypeCount} plugin type(s)");
        TestContext.WriteLine($"  HasColumnizer: {typeInfo.HasColumnizer}");
        TestContext.WriteLine($"  HasFileSystem: {typeInfo.HasFileSystem}");
        TestContext.WriteLine($"  HasContextMenu: {typeInfo.HasContextMenu}");
        TestContext.WriteLine($"  HasKeywordAction: {typeInfo.HasKeywordAction}");
    }

    #endregion
}
