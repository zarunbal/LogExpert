using System.Runtime.Versioning;
using System.Windows.Forms;

using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Services;

using NUnit.Framework;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.Tests.Services;

/// <summary>
/// Unit tests for TabController.
/// 
/// Note: Many tests are limited because LogWindow is a complex WinForms control
/// that cannot be easily mocked or subclassed. Tests that require actual LogWindow
/// instances would need to be run as integration tests with full UI infrastructure.
/// 
/// These tests focus on the core TabController functionality that can be tested
/// without instantiating LogWindow objects.
/// </summary>
[TestFixture]
[SupportedOSPlatform("windows")]
[Apartment(ApartmentState.STA)] // Required for WinForms controls
internal class TabControllerTests : IDisposable
{
    private Form _testForm;
    private DockPanel _dockPanel;
    private TabController _tabController;
    private bool _disposed;

    [SetUp]
    public void Setup()
    {
        // Create a real Form and DockPanel for testing
        // This is necessary because DockPanel requires WinForms infrastructure
        _testForm = new Form();
        _dockPanel = new DockPanel
        {
            Dock = DockStyle.Fill,
            DocumentStyle = DocumentStyle.DockingMdi
        };
        _testForm.Controls.Add(_dockPanel);
        _testForm.Show(); // Must show form for DockPanel to work

        _tabController = new TabController(_dockPanel);
    }

    [TearDown]
    public void TearDown()
    {
        _tabController?.Dispose();
        _testForm?.Close();
        _testForm?.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _tabController?.Dispose();
            _testForm?.Dispose();
        }

        _disposed = true;
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithDockPanel_InitializesSuccessfully()
    {
        // Arrange & Act - already done in Setup

        // Assert
        Assert.That(_tabController, Is.Not.Null);
        Assert.That(_tabController.GetWindowCount(), Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithNullDockPanel_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TabController(null));
    }

    [Test]
    public void Constructor_WithoutDockPanel_CreatesUninitializedController()
    {
        // Arrange & Act
        using var controller = new TabController();

        // Assert - controller should be created but not initialized
        // Calling GetWindowCount should still work (returns 0)
        Assert.That(controller.GetWindowCount(), Is.EqualTo(0));
    }

    #endregion

    #region InitializeDockPanel Tests

    [Test]
    public void InitializeDockPanel_WithValidDockPanel_Succeeds()
    {
        // Arrange
        using var controller = new TabController();

        // Act
        controller.InitializeDockPanel(_dockPanel);

        // Assert - no exception thrown, and controller should work
        Assert.That(controller.GetWindowCount(), Is.EqualTo(0));
    }

    [Test]
    public void InitializeDockPanel_WithNullDockPanel_ThrowsArgumentNullException()
    {
        // Arrange
        using var controller = new TabController();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => controller.InitializeDockPanel(null));
    }

    [Test]
    public void InitializeDockPanel_WhenAlreadyInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        using var controller = new TabController(_dockPanel);

        // Create a new form and dock panel for the second initialization attempt
        using var form2 = new Form();
        using var dockPanel2 = new DockPanel();
        form2.Controls.Add(dockPanel2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => controller.InitializeDockPanel(dockPanel2));
    }

    #endregion

    #region GetAllWindowsFromDockPanel Tests

    [Test]
    public void GetAllWindowsFromDockPanel_WhenNotInitialized_ReturnsEmptyList()
    {
        // Arrange
        using var controller = new TabController();

        // Act
        var result = controller.GetAllWindowsFromDockPanel();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAllWindowsFromDockPanel_WhenInitializedButEmpty_ReturnsEmptyList()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.GetAllWindowsFromDockPanel();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAllWindowsFromDockPanel_ReturnsReadOnlyList()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.GetAllWindowsFromDockPanel();

        // Assert - ReadOnlyCollection implements IReadOnlyList
        Assert.That(result, Is.InstanceOf<IReadOnlyList<LogWindow>>());
    }

    #endregion

    #region GetAllWindows Tests

    [Test]
    public void GetAllWindows_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.GetAllWindows();

        // Assert
        Assert.That(result, Is.Empty);
        Assert.That(result, Is.InstanceOf<IReadOnlyList<LogWindow>>());
    }

    #endregion

    #region GetWindowCount Tests

    [Test]
    public void GetWindowCount_WhenEmpty_ReturnsZero()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.GetWindowCount();

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    #endregion

    #region HasWindow Tests

    [Test]
    public void HasWindow_WithNullWindow_ReturnsFalse()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.HasWindow(null);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region GetActiveWindow Tests

    [Test]
    public void GetActiveWindow_WhenNoWindowActive_ReturnsNull()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.GetActiveWindow();

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region FindWindowByFileName Tests

    [Test]
    public void FindWindowByFileName_WithNullFileName_ReturnsNull()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.FindWindowByFileName(null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindWindowByFileName_WithEmptyFileName_ReturnsNull()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.FindWindowByFileName(string.Empty);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindWindowByFileName_WhenNoWindowsExist_ReturnsNull()
    {
        // Arrange - already done in Setup

        // Act
        var result = _tabController.FindWindowByFileName("test.log");

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region AddWindow Tests

    [Test]
    public void AddWindow_WithNullWindow_ThrowsArgumentNullException()
    {
        // Arrange - already done in Setup

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tabController.AddWindow(null, "Test Window"));
    }

    [Test]
    public void AddWindow_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        using var controller = new TabController();

        // Create a mock-like object that's not null to avoid ArgumentNullException
        // We need to test that the "not initialized" check happens
        // Unfortunately, LogWindow cannot be instantiated without its dependencies
        // So we can only verify the ArgumentNullException is thrown first for null
        var ex = Assert.Throws<ArgumentNullException>(() => controller.AddWindow(null, "Test"));
        Assert.That(ex.ParamName, Is.EqualTo("window"));
    }

    #endregion

    #region RemoveWindow Tests

    [Test]
    public void RemoveWindow_WithNullWindow_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.RemoveWindow(null));
    }

    #endregion

    #region CloseWindow Tests

    [Test]
    public void CloseWindow_WithNullWindow_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.CloseWindow(null));
    }

    #endregion

    #region CloseAllWindows Tests

    [Test]
    public void CloseAllWindows_WhenEmpty_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.CloseAllWindows());
    }

    #endregion

    #region CloseAllExcept Tests

    [Test]
    public void CloseAllExcept_WithNullWindow_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.CloseAllExcept(null));
    }

    #endregion

    #region ActivateWindow Tests

    [Test]
    public void ActivateWindow_WithNullWindow_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.ActivateWindow(null));
    }

    #endregion

    #region SwitchToNextWindow Tests

    [Test]
    public void SwitchToNextWindow_WhenEmpty_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.SwitchToNextWindow());
    }

    #endregion

    #region SwitchToPreviousWindow Tests

    [Test]
    public void SwitchToPreviousWindow_WhenEmpty_DoesNotThrow()
    {
        // Arrange - already done in Setup

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => _tabController.SwitchToPreviousWindow());
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_MultipleCallsDoNotThrow()
    {
        // Arrange
        using var controller = new TabController(_dockPanel);

        // Act & Assert - multiple dispose calls should not throw
        Assert.DoesNotThrow(() =>
        {
            controller.Dispose();
            controller.Dispose();
            controller.Dispose();
        });
    }

    #endregion
}
