using System.Reflection;

using NUnit.Framework;

namespace LogExpert.PluginRegistry.Tests;

/// <summary>
/// Regression tests for issue #658: trusted-plugins.json must only ever be written to the
/// directory the validator was configured with. The trust configuration must be loaded
/// lazily (at Initialize or first use), never eagerly at class-load time — an eager load
/// wrote the default file to %APPDATA%/LogExpert before Initialize could point the
/// validator at the portable configuration directory.
///
/// Limitation: these tests protect the lazy-load mechanism (removing it fails them with a
/// NullReferenceException), but they cannot detect an eager class-load write directly —
/// the type is already loaded by the time any in-process test runs, and an eager load
/// targets the real %APPDATA% path. Keep PluginValidator free of a static constructor and
/// of I/O in field initializers.
/// </summary>
[TestFixture]
public class PluginValidatorConfigPersistenceTests
{
    private string _testDataPath = null!;
    private string _configDir = null!;

    private string _originalConfigDirectory = null!;
    private string _originalConfigPath = null!;
    private object? _originalConfig;

    private static FieldInfo GetStaticField (string name)
    {
        var field = typeof(PluginValidator).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(field, Is.Not.Null, $"Expected private static field '{name}' on PluginValidator");
        return field!;
    }

    [SetUp]
    public void SetUp ()
    {
        _testDataPath = Path.Join(Path.GetTempPath(), "LogExpertConfigPersistenceTests", Guid.NewGuid().ToString());
        _configDir = Path.Join(_testDataPath, "activeConfig");
        _ = Directory.CreateDirectory(_configDir);

        _originalConfigDirectory = (string)GetStaticField("_configDirectory").GetValue(null)!;
        _originalConfigPath = (string)GetStaticField("_configPath").GetValue(null)!;
        _originalConfig = GetStaticField("_trustedPluginConfig").GetValue(null);
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Test")]
    public void TearDown ()
    {
        GetStaticField("_configDirectory").SetValue(null, _originalConfigDirectory);
        GetStaticField("_configPath").SetValue(null, _originalConfigPath);
        GetStaticField("_trustedPluginConfig").SetValue(null, _originalConfig);

        if (Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Puts the validator into the state it is in at process start before Initialize has
    /// run: no configuration loaded, config path pointing at the (here: sandboxed) default.
    /// </summary>
    private void SimulatePreInitializeState ()
    {
        GetStaticField("_configDirectory").SetValue(null, _configDir);
        GetStaticField("_configPath").SetValue(null, Path.Join(_configDir, "trusted-plugins.json"));
        GetStaticField("_trustedPluginConfig").SetValue(null, null);
    }

    [Test]
    [Description("Issue #658: config must be loaded lazily on first use, not eagerly at class load")]
    public void ValidatePlugin_WhenConfigNotYetLoaded_LoadsLazilyFromConfiguredDirectory ()
    {
        SimulatePreInitializeState();

        var pluginPath = Path.Join(_testDataPath, "SomePlugin.dll");
        File.WriteAllText(pluginPath, "not a real assembly");

        Assert.DoesNotThrow(() => PluginValidator.ValidatePlugin(pluginPath),
            "ValidatePlugin must work without an eagerly loaded configuration");

        Assert.That(File.Exists(Path.Join(_configDir, "trusted-plugins.json")), Is.True,
            "Lazy load should create the default configuration in the configured directory");
    }

    [Test]
    [Description("Issue #658: AddTrustedPlugin must lazily load and persist to the configured directory")]
    public void AddTrustedPlugin_WhenConfigNotYetLoaded_PersistsToConfiguredDirectory ()
    {
        SimulatePreInitializeState();

        var pluginPath = Path.Join(_testDataPath, "UserPlugin.dll");
        File.WriteAllText(pluginPath, "user plugin content");

        var result = PluginValidator.AddTrustedPlugin(pluginPath, out var errorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True, $"AddTrustedPlugin should succeed, but failed with: {errorMessage}");
            Assert.That(File.Exists(Path.Join(_configDir, "trusted-plugins.json")), Is.True,
                "Trust decision must be persisted to the configured directory");
        });
    }

    [Test]
    [Description("Normal mode is unchanged: Initialize creates the default config file in the given directory")]
    public void Initialize_WithEmptyDirectory_CreatesDefaultConfigFileThere ()
    {
        SimulatePreInitializeState();

        var freshDir = Path.Join(_testDataPath, "freshConfig");
        _ = Directory.CreateDirectory(freshDir);

        PluginValidator.Initialize(freshDir);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Join(freshDir, "trusted-plugins.json")), Is.True,
                "Initialize should create the default configuration in its directory");
            Assert.That(File.Exists(Path.Join(_configDir, "trusted-plugins.json")), Is.False,
                "Nothing may be written to the pre-Initialize default directory");
        });
    }
}
