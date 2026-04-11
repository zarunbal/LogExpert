using System.Runtime.Versioning;

using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.EventArguments;
using LogExpert.Core.Interfaces;
using LogExpert.Dialogs;
using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Interface;

using NLog;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Services.ToolWindowCoordinatorService;

[SupportedOSPlatform("windows")]
internal sealed class ToolWindowCoordinator (IConfigManager configManager) : IToolWindowCoordinator
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigManager _configManager = configManager;

    private BookmarkWindow? _bookmarkWindow;
    private LogWindow? _connectedLogWindow;
    private bool _firstBookmarkWindowShow = true;
    private bool _disposed;

    public void Initialize ()
    {
        _bookmarkWindow = new BookmarkWindow
        {
            HideOnClose = true,
            ShowHint = DockState.DockBottom
        };

        var prefs = _configManager.Settings.Preferences;
        _bookmarkWindow.PreferencesChanged(prefs.FontName, prefs.FontSize, prefs.SetLastColumnWidth, prefs.LastColumnWidth, SettingsFlags.All);
        _bookmarkWindow.VisibleChanged += OnBookmarkWindowVisibleChanged;
        _firstBookmarkWindowShow = true;
    }

    [SupportedOSPlatform("windows")]
    public void Destroy ()
    {
        if (_bookmarkWindow != null)
        {
            _bookmarkWindow.VisibleChanged -= OnBookmarkWindowVisibleChanged;
            _bookmarkWindow.HideOnClose = false;
            _bookmarkWindow.Close();
            _bookmarkWindow = null;
        }
    }

    public void Connect (LogWindow logWindow)
    {
        // Unsubscribe from previous window if still connected
        Disconnect();

        _connectedLogWindow = logWindow;

        // Subscribe to bookmark events for relay
        logWindow.BookmarkAdded += OnBookmarkAdded;
        logWindow.BookmarkRemoved += OnBookmarkRemoved;
        logWindow.BookmarkTextChanged += OnBookmarkTextChanged;
        logWindow.ColumnizerChanged += OnColumnizerChanged;

        // Set bookmark data and file context
        FileViewContext ctx = new(logWindow, logWindow);
        _bookmarkWindow?.SetBookmarkData(logWindow.BookmarkData);
        _bookmarkWindow?.SetCurrentFile(ctx);
    }

    public void Disconnect ()
    {
        if (_connectedLogWindow != null)
        {
            _connectedLogWindow.BookmarkAdded -= OnBookmarkAdded;
            _connectedLogWindow.BookmarkRemoved -= OnBookmarkRemoved;
            _connectedLogWindow.BookmarkTextChanged -= OnBookmarkTextChanged;
            _connectedLogWindow.ColumnizerChanged -= OnColumnizerChanged;
            _connectedLogWindow = null;
        }

        _bookmarkWindow?.SetBookmarkData(null);
        _bookmarkWindow?.SetCurrentFile(null);
    }

    [SupportedOSPlatform("windows")]
    public void ToggleBookmarkVisibility (DockPanel dockPanel)
    {
        if (_bookmarkWindow == null)
        {
            return;
        }

        if (_bookmarkWindow.Visible)
        {
            _bookmarkWindow.Hide();
        }
        else
        {
            // Strange: on very first Show() no bookmarks are displayed. After a hide it will work.
            if (_firstBookmarkWindowShow)
            {
                _bookmarkWindow.Show(dockPanel);
                _bookmarkWindow.Hide();
            }

            _bookmarkWindow.Show(dockPanel);
        }
    }

    public IDockContent? GetDockContent (string persistString)
    {
        return persistString.Equals(WindowTypes.BookmarkWindow.ToString(), StringComparison.Ordinal)
            ? _bookmarkWindow
            : (IDockContent?)null;
    }

    public void ApplyPreferences (string fontName, float fontSize, bool setLastColumnWidth, int lastColumnWidth, SettingsFlags flags)
    {
        _bookmarkWindow?.PreferencesChanged(fontName, fontSize, setLastColumnWidth, lastColumnWidth, flags);
    }

    public void SetLineColumnVisible (bool visible)
    {
        _ = (_bookmarkWindow?.LineColumnVisible = visible);
    }

    public void Dispose ()
    {
        if (!_disposed)
        {
            Disconnect();
            Destroy();
            _disposed = true;
        }
    }

    // Event relay handlers

    private void OnBookmarkAdded (object? sender, EventArgs e)
    {
        _bookmarkWindow?.UpdateView();
    }

    private void OnBookmarkRemoved (object? sender, EventArgs e)
    {
        _bookmarkWindow?.UpdateView();
    }

    private void OnBookmarkTextChanged (object? sender, BookmarkEventArgs e)
    {
        _bookmarkWindow?.BookmarkTextChanged(e.Bookmark);
    }

    private void OnColumnizerChanged (object? sender, ColumnizerEventArgs e)
    {
        _bookmarkWindow?.SetColumnizer(e.Columnizer);
    }

    private void OnBookmarkWindowVisibleChanged (object? sender, EventArgs e)
    {
        _firstBookmarkWindowShow = false;
    }
}