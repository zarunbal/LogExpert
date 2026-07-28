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
}
