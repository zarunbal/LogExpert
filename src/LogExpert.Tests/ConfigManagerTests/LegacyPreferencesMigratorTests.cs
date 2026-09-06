using LogExpert.Configuration;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;

using NUnit.Framework;

namespace LogExpert.Tests.ConfigManagerTests;

[TestFixture]
public class LegacyPreferencesMigratorTests
{
    [Test]
    public void V0Settings_WithMaskPrioTrue_BecomesMaskThenHistory ()
    {
        var settings = new Settings
        {
            SettingsVersion = 0,
        };
#pragma warning disable CS0618
        settings.Preferences.MaskPrio = true;
#pragma warning restore CS0618

        var changed = LegacyPreferencesMigrator.Migrate(settings);

        Assert.That(changed, Is.True);
        Assert.That(settings.Preferences.ColumnizerSelectionPriority, Is.EqualTo(ColumnizerSelectionPriority.MaskThenHistory));
        Assert.That(settings.SettingsVersion, Is.EqualTo(2));
    }

    [Test]
    public void V0Settings_WithMaskPrioFalse_KeepsDefaultPriority ()
    {
        var settings = new Settings { SettingsVersion = 0 };

        _ = LegacyPreferencesMigrator.Migrate(settings);

        Assert.That(settings.Preferences.ColumnizerSelectionPriority, Is.EqualTo(ColumnizerSelectionPriority.HistoryThenMask));
        Assert.That(settings.SettingsVersion, Is.EqualTo(2));
    }

    [Test]
    public void V0Settings_PreExistingMaskEntries_AreRewrittenToRegex ()
    {
        var settings = new Settings { SettingsVersion = 0 };
        settings.Preferences.ColumnizerMaskList.Add(new ColumnizerMaskEntry
        {
            Mask = @".+\.log$",
            ColumnizerName = "JsonColumnizer",
            // Default-loaded value is Glob (the new default).
        });

        _ = LegacyPreferencesMigrator.Migrate(settings);

        Assert.That(settings.Preferences.ColumnizerMaskList[0].Type, Is.EqualTo(MaskType.Regex));
    }

    [Test]
    public void V1Settings_WithLegacyMultiFilePattern_PreservesPreviousBehavior ()
    {
        var settings = new Settings { SettingsVersion = 1 };
        settings.Preferences.MultiFileOptions = new MultiFileOptions { FormatPattern = "*$J(.).log" };

        var changed = LegacyPreferencesMigrator.Migrate(settings);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.True);
            Assert.That(settings.SettingsVersion, Is.EqualTo(2));
            Assert.That(settings.Preferences.MultiFileOptions.FormatPattern, Is.EqualTo("*$J(.)"));
        });
    }

    [Test]
    public void CurrentSettings_IsNoOp ()
    {
        var settings = new Settings { SettingsVersion = LegacyPreferencesMigrator.CURRENT_SETTINGS_VERSION };
        settings.Preferences.MultiFileOptions = new MultiFileOptions { FormatPattern = "*$J(.).log" };
        settings.Preferences.ColumnizerMaskList.Add(new ColumnizerMaskEntry
        {
            Mask = "*.log",
            ColumnizerName = "C",
            Type = MaskType.Glob,
        });

        var changed = LegacyPreferencesMigrator.Migrate(settings);

        Assert.That(changed, Is.False);
        Assert.That(settings.Preferences.ColumnizerMaskList[0].Type, Is.EqualTo(MaskType.Glob));
        Assert.That(settings.Preferences.MultiFileOptions.FormatPattern, Is.EqualTo("*$J(.).log"));
    }

    [Test]
    public void NullSettings_Throws ()
    {
        _ = Assert.Throws<ArgumentNullException>(() => LegacyPreferencesMigrator.Migrate(null!));
    }
}
