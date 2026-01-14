using System.Runtime.Versioning;

using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Services;

using Moq;

using NUnit.Framework;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.Tests.Services;

[TestFixture]
[SupportedOSPlatform("windows")]
internal class TabControllerTests
{
    private Mock<DockPanel> _mockDockPanel;
    private TabController _TabController;

    [SetUp]
    public void Setup ()
    {
        _mockDockPanel = new Mock<DockPanel>();
        _TabController = new TabController(_mockDockPanel.Object);
    }

    [TearDown]
    public void TearDown ()
    {
        _TabController?.Dispose();
    }

    // Window Management Tests
    [Test]
    public void AddWindow_WithValidWindow_AddsToTracking ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");

        // Act
        _TabController.AddWindow(mockWindow, "Test Window");

        // Assert
        Assert.That(_TabController.GetWindowCount(), Is.EqualTo(1));
        Assert.That(_TabController.HasWindow(mockWindow), Is.True);
    }

    [Test]
    public void AddWindow_SameWindowTwice_ThrowsException ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        _TabController.AddWindow(mockWindow, "Test Window");

        // Act & Assert
        _ = Assert.Throws<InvalidOperationException>(() => _TabController.AddWindow(mockWindow, "Test Window"));
    }

    [Test]
    public void RemoveWindow_ExistingWindow_RemovesFromTracking ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        _TabController.AddWindow(mockWindow, "Test Window");

        // Act
        _TabController.RemoveWindow(mockWindow);

        // Assert
        Assert.That(_TabController.GetWindowCount(), Is.EqualTo(0));
        Assert.That(_TabController.HasWindow(mockWindow), Is.False);
    }

    // Event Tests
    [Test]
    public void AddWindow_RaisesWindowAddedEvent ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        bool eventRaised = false;
        LogWindow eventWindow = null;

        _TabController.WindowAdded += (s, e) =>
        {
            eventRaised = true;
            eventWindow = e.Window;
        };

        // Act
        _TabController.AddWindow(mockWindow, "Test Window");

        // Assert
        Assert.That(eventRaised, Is.True);
        Assert.That(eventWindow, Is.EqualTo(mockWindow));
    }

    // Window Finding Tests
    [Test]
    public void FindWindowByFileName_ExistingFile_ReturnsWindow ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        _TabController.AddWindow(mockWindow, "Test Window");

        // Act
        var found = _TabController.FindWindowByFileName("test.log");

        // Assert
        Assert.That(found, Is.EqualTo(mockWindow));
    }

    [Test]
    public void FindWindowByFileName_CaseInsensitive_ReturnsWindow ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        _TabController.AddWindow(mockWindow, "Test Window");

        // Act
        var found = _TabController.FindWindowByFileName("TEST.LOG");

        // Assert
        Assert.That(found, Is.EqualTo(mockWindow));
    }

    // Tab Switching Tests
    [Test]
    public void SwitchToNextWindow_MultipleWindows_ActivatesNextWindow ()
    {
        // Arrange
        var window1 = CreateMockWindow("test1.log");
        var window2 = CreateMockWindow("test2.log");
        _TabController.AddWindow(window1, "Window 1");
        _TabController.AddWindow(window2, "Window 2");
        window1.Activate();

        // Act
        _TabController.SwitchToNextWindow();

        // Assert
        // Verify window2.Activate() was called
        Mock.Get(window2).Verify(w => w.Activate(), Times.Once);
    }

    // Thread Safety Tests
    [Test]
    public void AddWindow_ConcurrentCalls_AllWindowsTracked ()
    {
        // Arrange
        var windows = Enumerable.Range(0, 100)
            .Select(i => CreateMockWindow($"test{i}.log"))
            .ToList();

        // Act
        _ = Parallel.ForEach(windows, (window, state, index) =>
        {
            _TabController.AddWindow(window, $"Window {index}");
        });

        // Assert
        Assert.That(_TabController.GetWindowCount(), Is.EqualTo(100));
    }

    // Disposal Tests
    [Test]
    public void Dispose_UnsubscribesFromAllEvents ()
    {
        // Arrange
        var mockWindow = CreateMockWindow("test.log");
        _TabController.AddWindow(mockWindow, "Test Window");

        // Act
        _TabController.Dispose();

        // Assert
        // Verify no event subscriptions remain
        Mock.Get(mockWindow).VerifyRemove(w => w.Disposed -= It.IsAny<EventHandler>());
    }

    private static LogWindow CreateMockWindow (string fileName)
    {
        var mock = new Mock<LogWindow>();
        _ = mock.Setup(w => w.FileName).Returns(fileName);
        return mock.Object;
    }
}
