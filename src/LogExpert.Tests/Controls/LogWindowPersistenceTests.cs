using System.Reflection;
using System.Runtime.Versioning;

using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Config;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Interface;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests.Controls;

[TestFixture]
[Apartment(ApartmentState.STA)]
[SupportedOSPlatform("windows")]
public sealed class LogWindowPersistenceTests : IDisposable
{
    private Mock<ILogWindowCoordinator> _coordinatorMock = null!;
    private Mock<IConfigManager> _configManagerMock = null!;
    private Settings _settings = null!;
    private WindowsFormsSynchronizationContext? _syncContext;
    private LogWindow _logWindow = null!;
    private bool _disposed;

    [OneTimeSetUp]
    public void OneTimeSetUp ()
    {
        var dir = Path.GetDirectoryName(typeof(LogWindowPersistenceTests).Assembly.Location)!;
        _ = PluginRegistry.PluginRegistry.Create(dir, 500);
    }

    [SetUp]
    public void SetUp ()
    {
        if (SynchronizationContext.Current == null)
        {
            _syncContext = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_syncContext);
        }

        _coordinatorMock = new Mock<ILogWindowCoordinator>();
        _ = _coordinatorMock.Setup(c => c.ResolveHighlightGroup(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(new LogExpert.Core.Entities.HighlightGroup());
        _ = _coordinatorMock.Setup(c => c.SearchParams).Returns(new LogExpert.Core.Entities.SearchParams());

        _configManagerMock = new Mock<IConfigManager>();
        _settings = new Settings();
        _ = _configManagerMock.Setup(cm => cm.Settings).Returns(_settings);

        _settings.FilterList.Add(new FilterParams
        {
            SearchText = "SHARED_FILTER_LIST_ENTRY",
            IsCaseSensitive = false,
            IsRegex = false,
            IsFilterTail = true,
            FuzzyValue = 0,
            SpreadBefore = 0,
            SpreadBehind = 0
        });

        _logWindow = new LogWindow(
            _coordinatorMock.Object,
            "test.log",
            isTempFile: false,
            forcePersistenceLoading: false,
            configManager: _configManagerMock.Object);
    }

    [TearDown]
    public void TearDown ()
    {
        _logWindow?.Dispose();
        _syncContext?.Dispose();
        _syncContext = null;
    }

    public void Dispose ()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose (bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _logWindow?.Dispose();
            _syncContext?.Dispose();
        }

        _disposed = true;
    }

    [Test]
    public void GatherSessionSnapshot_SavesLiveFilterState_InsteadOfSharedFilterList ()
    {
        SetText("filterComboBox", "LIVE_FILTER_TEXT");
        SetText("filterRangeComboBox", "LIVE_RANGE_TEXT");
        SetChecked("filterCaseSensitiveCheckBox", true);
        SetChecked("filterRegexCheckBox", true);
        SetChecked("filterTailCheckBox", false);
        SetChecked("invertFilterCheckBox", true);
        SetChecked("rangeCheckBox", true);
        SetChecked("columnRestrictCheckBox", true);
        SetValue("knobControlFuzzy", 3);
        SetValue("knobControlFilterBackSpread", 4);
        SetValue("knobControlFilterForeSpread", 5);

        var snapshot = _logWindow.GatherSessionSnapshot();

        Assert.That(snapshot.FilterParamsList, Has.Count.EqualTo(1));

        var saved = snapshot.FilterParamsList[0];
        var shared = _settings.FilterList[0];

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.SameAs(shared));
            Assert.That(saved.SearchText, Is.EqualTo("LIVE_FILTER_TEXT"));
            Assert.That(saved.RangeSearchText, Is.EqualTo("LIVE_RANGE_TEXT"));
            Assert.That(saved.IsCaseSensitive, Is.True);
            Assert.That(saved.IsRegex, Is.True);
            Assert.That(saved.IsFilterTail, Is.False);
            Assert.That(saved.IsInvert, Is.True);
            Assert.That(saved.IsRangeSearch, Is.True);
            Assert.That(saved.ColumnRestrict, Is.True);
            Assert.That(saved.FuzzyValue, Is.EqualTo(3));
            Assert.That(saved.SpreadBefore, Is.EqualTo(4));
            Assert.That(saved.SpreadBehind, Is.EqualTo(5));
            Assert.That(saved.SearchText, Is.Not.EqualTo(shared.SearchText));
        });
    }

    private void SetText (string fieldName, string value)
    {
        var control = GetField<System.Windows.Forms.Control>(fieldName);
        control.Text = value;
    }

    private void SetChecked (string fieldName, bool value)
    {
        var control = GetField<object>(fieldName)!;
        control.GetType().GetProperty("Checked")!.SetValue(control, value);
    }

    private void SetValue (string fieldName, int value)
    {
        var control = GetField<object>(fieldName)!;
        control.GetType().GetProperty("Value")!.SetValue(control, value);
    }

    private T GetField<T> (string fieldName)
    {
        var field = _logWindow.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Could not find private field '{fieldName}' on LogWindow");
        return (T)field!.GetValue(_logWindow)!;
    }
}
