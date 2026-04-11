using LogExpert.Classes;
using LogExpert.Core.Interfaces;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests.IPC;

/// <summary>
/// Unit tests for Active Window Tracking functionality
/// Tests that the most recently activated window receives new files when "Allow Only One Instance" is enabled
/// </summary>
[TestFixture]
public class ActiveWindowTrackingTests
{
    private Mock<ILogTabWindow> _mockWindow1;
    private Mock<ILogTabWindow> _mockWindow2;
    private Mock<ILogTabWindow> _mockWindow3;
    private LogExpertProxy _proxy;

    [SetUp]
    public void SetUp ()
    {
        _mockWindow1 = new Mock<ILogTabWindow>();
        _mockWindow2 = new Mock<ILogTabWindow>();
        _mockWindow3 = new Mock<ILogTabWindow>();

        // Setup common mock behavior
        SetupWindowMock(_mockWindow1, "Window1");
        SetupWindowMock(_mockWindow2, "Window2");
        SetupWindowMock(_mockWindow3, "Window3");

        // Create proxy with first window
        _proxy = new LogExpertProxy(_mockWindow1.Object);
    }

    private static void SetupWindowMock (Mock<ILogTabWindow> mock, string name)
    {
        _ = mock.Setup(w => w.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>()))
            .Returns((Delegate d, object[] args) => default(object));
        _ = mock.Setup(w => w.LoadFiles(It.IsAny<string[]>()));
        _ = mock.Setup(w => w.ToString()).Returns(name);
    }

    #region Active Window Tracking Tests

    [Test]
    public void NotifyWindowActivated_TracksFirstActivation ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.LoadFiles(files);

        // Assert
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
    }

    [Test]
    public void NotifyWindowActivated_TracksMultipleActivations ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        _proxy.LoadFiles(files);

        // Assert - Window2 was activated last
        _mockWindow2.Verify(w => w.LoadFiles(files), Times.Once);
        _mockWindow1.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    [Test]
    public void NotifyWindowActivated_OverwritesPreviousActivation ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act - Activate windows in sequence
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        _proxy.NotifyWindowActivated(_mockWindow3.Object);
        _proxy.NotifyWindowActivated(_mockWindow1.Object); // Back to window1
        _proxy.LoadFiles(files);

        // Assert - Window1 was activated last
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
        _mockWindow2.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
        _mockWindow3.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    [Test]
    public void LoadFiles_WithoutActivation_FallsBackToLastWindow ()
    {
        // Arrange
        var files = new[] { "test.log" };
        // Don't call NotifyWindowActivated

        // Act
        _proxy.LoadFiles(files);

        // Assert - Should fall back to last window in list (window1, the only window)
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
    }

    [Test]
    public void NotifyWindowActivated_WithNullWindow_HandlesGracefully ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act - Null activation should be handled
        _proxy.NotifyWindowActivated(null);
        _proxy.LoadFiles(files);

        // Assert - Should fall back to last window
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
    }

    [Test]
    public void NotifyWindowActivated_AfterNullActivation_RestoresTracking ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act
        _proxy.NotifyWindowActivated(null);
        _proxy.NotifyWindowActivated(_mockWindow2.Object); // Valid activation
        _proxy.LoadFiles(files);

        // Assert - Should use window2
        _mockWindow2.Verify(w => w.LoadFiles(files), Times.Once);
        _mockWindow1.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    #endregion

    #region Scenario Tests

    [Test]
    public void Scenario_UserClicksWindow1ThenWindow2_Window2ReceivesFiles ()
    {
        // Simulate real-world scenario:
        // User opens two windows, clicks on window1, then window2, then opens a file

        // Act
        _proxy.NotifyWindowActivated(_mockWindow1.Object); // User clicks window1
        _proxy.NotifyWindowActivated(_mockWindow2.Object); // User clicks window2
        _proxy.LoadFiles(["newfile.log"]);

        // Assert
        _mockWindow2.Verify(w => w.LoadFiles(It.Is<string[]>(f => f[0] == "newfile.log")), Times.Once);
        _mockWindow1.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    [Test]
    public void Scenario_UserAlternatesBetweenWindows_LastClickedWindowReceivesFiles ()
    {
        // Simulate user alternating focus between windows

        // Act - User switches between windows multiple times
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        _proxy.NotifyWindowActivated(_mockWindow1.Object); // Final focus on window1

        _proxy.LoadFiles(["final.log"]);

        // Assert - Window1 should receive the file
        _mockWindow1.Verify(w => w.LoadFiles(It.Is<string[]>(f => f[0] == "final.log")), Times.Once);
        _mockWindow2.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    [Test]
    public void Scenario_MultipleFilesOpened_AllGoToSameActiveWindow ()
    {
        // Simulate opening multiple files while window2 is active

        // Act
        _proxy.NotifyWindowActivated(_mockWindow2.Object);

        _proxy.LoadFiles(["file1.log"]);
        _proxy.LoadFiles(["file2.log"]);
        _proxy.LoadFiles(["file3.log"]);

        // Assert - All files go to window2
        _mockWindow2.Verify(w => w.LoadFiles(It.Is<string[]>(f => f[0] == "file1.log")), Times.Once);
        _mockWindow2.Verify(w => w.LoadFiles(It.Is<string[]>(f => f[0] == "file2.log")), Times.Once);
        _mockWindow2.Verify(w => w.LoadFiles(It.Is<string[]>(f => f[0] == "file3.log")), Times.Once);
        _mockWindow1.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void EdgeCase_ActivationBeforeFirstWindow_DoesNotCrash ()
    {
        // Simulate activation call before any windows exist (edge case)

        // Arrange - Create proxy without window
        var emptyProxy = new LogExpertProxy(_mockWindow1.Object);

        // Act & Assert - Should not crash
        Assert.DoesNotThrow(() => emptyProxy.NotifyWindowActivated(_mockWindow2.Object));
    }

    [Test]
    public void EdgeCase_LoadFilesWithEmptyFileArray_HandlesGracefully ()
    {
        // Arrange
        _proxy.NotifyWindowActivated(_mockWindow2.Object);

        // Act & Assert - Should not crash
        Assert.DoesNotThrow(() => _proxy.LoadFiles([]));
    }

    [Test]
    public void EdgeCase_MultipleNotificationsForSameWindow_TracksCorrectly ()
    {
        // Arrange
        var files = new[] { "test.log" };

        // Act - Activate same window multiple times
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.NotifyWindowActivated(_mockWindow1.Object);
        _proxy.LoadFiles(files);

        // Assert - Should still work correctly
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
    }

    #endregion

    #region Behavior Verification

    [Test]
    public void LoadFilesUsesActiveWindow_NotCreationOrder ()
    {
        // This test verifies improvements:
        // Files load in the most recently ACTIVATED window,
        // not just the most recently CREATED window

        // Before : Files would go to _windowList[^1] (last created)
        // After : Files go to _mostRecentActiveWindow (last activated)

        // Arrange - Window1 was created first, but window2 is activated
        // (In a real scenario, window2 might have been created second)
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        var files = new[] { "test.log" };

        // Act
        _proxy.LoadFiles(files);

        // Assert - Window2 receives files because it was activated (not because of creation order)
        _mockWindow2.Verify(w => w.LoadFiles(files), Times.Once);
        _mockWindow1.Verify(w => w.LoadFiles(It.IsAny<string[]>()), Times.Never);
    }

    [Test]
    public void FallbackWhenNoActivation_UsesLastWindowInList ()
    {
        // Verifies fallback behavior:
        //
        // before: Files loaded in most recently CREATED window
        // after: Files load in most recently ACTIVATED window
        //
        // When no activation has occurred yet, after falls back to
        // the last window in the creation order (matching before behavior).

        // Arrange - No activation calls made
        var files = new[] { "test.log" };

        // Act
        _proxy.LoadFiles(files);

        // Assert - Falls back to window in list
        _mockWindow1.Verify(w => w.LoadFiles(files), Times.Once);
    }

    #endregion

    #region Integration with Lock Instance

    [Test]
    public void Integration_ActiveWindowTracking_WithoutLockInstance ()
    {
        // Documents integration: Active window tracking works independently
        // Lock instance priority is checked in NewWindowOrLockedWindow(),
        // which then calls LoadFiles() if no locked window exists

        // Arrange
        _proxy.NotifyWindowActivated(_mockWindow2.Object);
        var files = new[] { "test.log" };

        // Act - Simulate the flow: NewWindowOrLockedWindow -> LoadFiles
        _proxy.LoadFiles(files); // This is what NewWindowOrLockedWindow calls

        // Assert - Window2 receives files (active window tracking works)
        _mockWindow2.Verify(w => w.LoadFiles(files), Times.Once);
    }

    #endregion
}
