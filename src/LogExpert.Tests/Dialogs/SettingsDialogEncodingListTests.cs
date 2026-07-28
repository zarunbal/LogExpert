using System.Text;

using LogExpert.Core.Helpers;
using LogExpert.Dialogs;

using NUnit.Framework;

namespace LogExpert.Tests.Dialogs;

/// <summary>
/// The Preferences encoding dropdown is the only way to set <c>Preferences.DefaultEncoding</c>, so the
/// list is asserted directly — building the dialog is not needed to know what it offers.
/// </summary>
[TestFixture]
public class SettingsDialogEncodingListTests
{
    [Test]
    [TestCase(1250, TestName = "GetAvailableEncodings_OffersWindows1250")]
    [TestCase(1252, TestName = "GetAvailableEncodings_OffersWindows1252")]
    public void GetAvailableEncodings_OffersLegacyCodePage (int codePage)
    {
        var encodings = SettingsDialog.GetAvailableEncodings();

        Assert.That(encodings.Select(encoding => encoding.CodePage), Does.Contain(codePage));
    }

    /// <summary>
    /// Every offered encoding is saved as its name and resolved from that name on the next start, so a
    /// name that cannot be resolved again would silently degrade to <see cref="Encoding.Default"/>.
    /// </summary>
    [Test]
    public void GetAvailableEncodings_EveryEntryResolvesByItsPersistedName ()
    {
        var encodings = SettingsDialog.GetAvailableEncodings();

        Assert.Multiple(() =>
        {
            foreach (var encoding in encodings)
            {
                Assert.That(
                    EncodingRegistry.TryGetEncoding(encoding.HeaderName, out _),
                    Is.True,
                    $"'{encoding.HeaderName}' cannot be resolved back from a saved preference");
            }
        });
    }

    /// <summary>
    /// The dropdown renders each entry as its <see cref="Encoding.HeaderName"/>, so two entries sharing a
    /// code page are two rows the user cannot tell apart. <c>Encoding.Default</c> and
    /// <see cref="Encoding.UTF8"/> are both code page 65001 on .NET and both read <c>utf-8</c>.
    /// </summary>
    [Test]
    public void GetAvailableEncodings_NoTwoEntriesShareACodePage ()
    {
        var codePages = SettingsDialog.GetAvailableEncodings().Select(encoding => encoding.CodePage);

        Assert.That(codePages, Is.Unique);
    }

    /// <summary>
    /// The selection is persisted by name and restored with
    /// <c>comboBoxEncoding.SelectedItem = EncodingRegistry.GetEncoding(name, …)</c>. WinForms locates that
    /// item with <see cref="object.Equals(object)"/>, so an entry whose name resolves back to an instance
    /// that is not equal to it can never be reselected — the row goes dead after the first restart.
    /// </summary>
    [Test]
    public void GetAvailableEncodings_EveryEntryIsReselectableAfterARoundTrip ()
    {
        var encodings = SettingsDialog.GetAvailableEncodings();

        Assert.Multiple(() =>
        {
            foreach (var encoding in encodings)
            {
                var restored = EncodingRegistry.GetEncoding(encoding.HeaderName, SettingsDialog.FallbackEncoding);

                Assert.That(
                    restored,
                    Is.EqualTo(encoding),
                    $"picking '{encoding.HeaderName}' saves a name that reselects a different entry");
            }
        });
    }

    /// <summary>
    /// <c>FillDialog</c> and <c>SavePreferences</c> fall back to this encoding when the persisted name is
    /// unusable, so it has to be an offered entry — otherwise the dropdown shows no selection and OK
    /// rewrites the preference to something that was never on the list.
    /// </summary>
    [Test]
    public void GetAvailableEncodings_OffersTheFallbackEncoding ()
    {
        var encodings = SettingsDialog.GetAvailableEncodings();

        Assert.That(encodings, Does.Contain(SettingsDialog.FallbackEncoding));
    }

    /// <summary>
    /// The same round trip against a real <see cref="ComboBox"/>, because the reselect goes through
    /// <c>Items.IndexOf</c> — WinForms, not NUnit, has the final say on whether a row is reachable.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)]
    public void ComboBox_EveryOfferedRow_IsSelectedAgainAfterASaveAndRestore ()
    {
        var encodings = SettingsDialog.GetAvailableEncodings();

        using var comboBox = new ComboBox { FormattingEnabled = true };
        comboBox.Items.AddRange([.. encodings]);

        Assert.Multiple(() =>
        {
            for (var row = 0; row < encodings.Count; row++)
            {
                comboBox.SelectedIndex = row;
                var saved = ((Encoding)comboBox.SelectedItem).HeaderName;

                comboBox.SelectedIndex = -1;
                comboBox.SelectedItem = EncodingRegistry.GetEncoding(saved, SettingsDialog.FallbackEncoding);

                Assert.That(comboBox.SelectedIndex, Is.EqualTo(row), $"row {row} ('{saved}') is unreachable after a restart");
            }
        });
    }
}
