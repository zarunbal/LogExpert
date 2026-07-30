using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

using LogExpert.Core.EventArguments;
using LogExpert.Core.Helpers;
using LogExpert.Dialogs;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Services.MenuToolbarService;

[SupportedOSPlatform("windows")]
internal sealed class MenuToolbarController : IMenuToolbarController
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private MenuStrip _mainMenu;
    private ToolStrip _buttonToolbar;
    private ToolStrip _externalToolsToolStrip;

    // Controls passed in from LogTabWindow (not owned by this controller)
    private DateTimeDragControl _dragControlDateTime;
    private CheckBox _checkBoxFollowTail;

    // Menu items (cached during initialization for performance)
    private ToolStripMenuItem _closeFileMenuItem;
    private ToolStripMenuItem _searchMenuItem;
    private ToolStripMenuItem _filterMenuItem;
    private ToolStripMenuItem _goToLineMenuItem;
    private ToolStripMenuItem _multiFileMenuItem;
    private ToolStripMenuItem _multiFileEnabledMenuItem;
    private ToolStripMenuItem _timeshiftMenuItem;
    private ToolStripTextBox _timeshiftTextBox;
    private ToolStripMenuItem _cellSelectMenuItem;
    private ToolStripMenuItem _columnFinderMenuItem;

    // Encoding menu items
    private ToolStripMenuItem _encodingMenuItem;
    private ToolStripMenuItem _encodingAsciiMenuItem;
    private ToolStripMenuItem _encodingUtf8MenuItem;
    private ToolStripMenuItem _encodingUtf16MenuItem;
    private ToolStripMenuItem _encodingIso88591MenuItem;
    private ToolStripMenuItem _encodingGb2312MenuItem;

    // Toolbar items
    private ToolStripButton _bubblesButton;

    // Highlight group combo
    private ToolStripComboBox _highlightGroupCombo;

    // History menu
    private ToolStripMenuItem _lastUsedMenuItem;

    private readonly SynchronizationContext _uiContext;
    private bool _disposed;
    private bool _suppressEvents;

    public event EventHandler<HistoryItemClickedEventArgs> HistoryItemClicked;
    public event EventHandler<HistoryItemClickedEventArgs> HistoryItemRemoveRequested;
    public event EventHandler<HighlightGroupSelectedEventArgs> HighlightGroupSelected;

    public MenuToolbarController ()
    {
        _uiContext = SynchronizationContext.Current
            ?? throw new InvalidOperationException("Must be created on UI thread");
    }

    public void InitializeMenus (MenuStrip mainMenu, ToolStrip buttonToolbar,
        ToolStrip externalToolsToolStrip, DateTimeDragControl dragControlDateTime,
        CheckBox checkBoxFollowTail)
    {
        ArgumentNullException.ThrowIfNull(mainMenu);
        ArgumentNullException.ThrowIfNull(buttonToolbar);
        ArgumentNullException.ThrowIfNull(dragControlDateTime);
        ArgumentNullException.ThrowIfNull(checkBoxFollowTail);

        _mainMenu = mainMenu;
        _buttonToolbar = buttonToolbar;
        _externalToolsToolStrip = externalToolsToolStrip;
        _dragControlDateTime = dragControlDateTime;
        _checkBoxFollowTail = checkBoxFollowTail;

        // Cache menu items by designer name (recursive search into dropdowns)
        _closeFileMenuItem = FindMenuItem("closeFileToolStripMenuItem");
        _searchMenuItem = FindMenuItem("searchToolStripMenuItem");
        _filterMenuItem = FindMenuItem("filterToolStripMenuItem");
        _goToLineMenuItem = FindMenuItem("goToLineToolStripMenuItem");
        _multiFileMenuItem = FindMenuItem("multiFileToolStripMenuItem");
        _multiFileEnabledMenuItem = FindMenuItem("multiFileEnabledStripMenuItem");
        _timeshiftMenuItem = FindMenuItem("timeshiftToolStripMenuItem");
        _timeshiftTextBox = FindToolStripItem<ToolStripTextBox>(_mainMenu, "timeshiftToolStripTextBox");
        _cellSelectMenuItem = FindMenuItem("cellSelectModeToolStripMenuItem");
        _columnFinderMenuItem = FindMenuItem("columnFinderToolStripMenuItem");

        // Encoding menu items
        _encodingMenuItem = FindMenuItem("encodingToolStripMenuItem");
        _encodingAsciiMenuItem = FindMenuItem("encodingASCIIToolStripMenuItem");
        _encodingUtf8MenuItem = FindMenuItem("encodingUTF8toolStripMenuItem");
        _encodingUtf16MenuItem = FindMenuItem("encodingUTF16toolStripMenuItem");
        _encodingIso88591MenuItem = FindMenuItem("encodingISO88591toolStripMenuItem");
        _encodingGb2312MenuItem = FindMenuItem("encodingGB2312toolStripMenuItem");

        // Toolbar items
        _bubblesButton = FindToolStripItem<ToolStripButton>(_buttonToolbar, "toolStripButtonBubbles");

        // Highlight group combo (may be on buttonToolbar or externalToolsToolStrip)
        _highlightGroupCombo = FindToolStripItem<ToolStripComboBox>(_buttonToolbar, "highlightGroupsToolStripComboBox") ?? FindToolStripItem<ToolStripComboBox>(_externalToolsToolStrip, "highlightGroupsToolStripComboBox");

        _highlightGroupCombo?.SelectedIndexChanged += OnHighlightGroupComboSelectedIndexChanged;

        // History menu
        _lastUsedMenuItem = FindMenuItem("lastUsedToolStripMenuItem");

        LogMissingItems();
    }

    public void UpdateGuiState (GuiStateEventArgs state, bool timestampControlEnabled)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (_uiContext != SynchronizationContext.Current)
        {
            _uiContext.Post(_ => UpdateGuiState(state, timestampControlEnabled), null);
            return;
        }

        _suppressEvents = true;

        try
        {
            _checkBoxFollowTail.Checked = state.FollowTail;
            _mainMenu.Enabled = state.MenuEnabled;

            // Timeshift
            if (_timeshiftMenuItem != null)
            {
                _timeshiftMenuItem.Enabled = state.TimeshiftPossible;
                _timeshiftMenuItem.Checked = state.TimeshiftEnabled;
            }

            if (_timeshiftTextBox != null)
            {
                _timeshiftTextBox.Text = state.TimeshiftText;
                _timeshiftTextBox.Enabled = state.TimeshiftEnabled;
            }

            // Multi-file
            if (_multiFileMenuItem != null)
            {
                _multiFileMenuItem.Enabled = state.MultiFileEnabled;
                _multiFileMenuItem.Checked = state.IsMultiFileActive;
            }

            _ = (_multiFileEnabledMenuItem?.Checked = state.IsMultiFileActive);

            // Cell select
            _ = (_cellSelectMenuItem?.Checked = state.CellSelectMode);

            // Encoding
            UpdateEncodingMenu(state.CurrentEncoding);

            // Timestamp drag control
            if (state.TimeshiftPossible && timestampControlEnabled)
            {
                _dragControlDateTime.MinDateTime = state.MinTimestamp;
                _dragControlDateTime.MaxDateTime = state.MaxTimestamp;
                _dragControlDateTime.DateTime = state.Timestamp;
                _dragControlDateTime.Visible = true;
                _dragControlDateTime.Enabled = true;
                _dragControlDateTime.Refresh();
            }
            else
            {
                _dragControlDateTime.Visible = false;
                _dragControlDateTime.Enabled = false;
            }

            // Toolbar
            _ = (_bubblesButton?.Checked = state.ShowBookmarkBubbles);

            // Highlight group
            _ = (_highlightGroupCombo?.Text = state.HighlightGroupName);

            // Column finder
            _ = (_columnFinderMenuItem?.Checked = state.ColumnFinderVisible);
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    public void UpdateEncodingMenu (Encoding currentEncoding)
    {
        if (_uiContext != SynchronizationContext.Current)
        {
            _uiContext.Post(_ => UpdateEncodingMenu(currentEncoding), null);
            return;
        }

        // Clear every row through the dropdown rather than row by row, so a row added to the menu
        // cannot be left checked by a clear list nobody remembered to extend.
        if (_encodingMenuItem != null)
        {
            foreach (var row in _encodingMenuItem.DropDownItems)
            {
                SetCheckedSafe(row as ToolStripMenuItem, false);
            }
        }

        SetCheckedSafe(MenuItemFor(currentEncoding), true);
    }

    /// <summary>
    /// The encoding menu row representing <paramref name="encoding"/>, or null when the file is read
    /// with an encoding the menu does not offer (a Preferences default such as windows-1250, say).
    /// </summary>
    /// <remarks>
    /// Matched by code page rather than by runtime type, because several instances stand for the same
    /// row: the menu applies UTF-8 without a BOM while the Preferences default is
    /// <see cref="Encoding.UTF8"/> with one, and <c>Encoding.Default</c> — what a file with neither a
    /// preamble nor a configured default is read with — is a third UTF-8 instance. Type and equality
    /// checks put those on different rows (or, before the "ANSI" row was dropped, on the row for a
    /// different encoding entirely).
    /// </remarks>
    private ToolStripMenuItem MenuItemFor (Encoding encoding)
    {
        if (encoding == null)
        {
            return null;
        }

        var codePage = encoding.CodePage;

        if (codePage == Encoding.ASCII.CodePage)
        {
            return _encodingAsciiMenuItem;
        }

        if (codePage == Encoding.UTF8.CodePage)
        {
            return _encodingUtf8MenuItem;
        }

        if (codePage == Encoding.Unicode.CodePage || codePage == Encoding.BigEndianUnicode.CodePage)
        {
            return _encodingUtf16MenuItem;
        }

        if (codePage == Encoding.Latin1.CodePage)
        {
            return _encodingIso88591MenuItem;
        }

        if (codePage == EncodingRegistry.CODE_PAGE_GB2312)
        {
            return _encodingGb2312MenuItem;
        }

        return null;
    }

    public void UpdateHighlightGroups (IEnumerable<string> groups, string selectedGroup)
    {
        if (_highlightGroupCombo == null)
        {
            return;
        }

        if (_uiContext != SynchronizationContext.Current)
        {
            _uiContext.Post(_ => UpdateHighlightGroups(groups, selectedGroup), null);
            return;
        }

        _suppressEvents = true;
        try
        {
            _highlightGroupCombo.Items.Clear();

            foreach (var group in groups)
            {
                _ = _highlightGroupCombo.Items.Add(group);

                if (group.Equals(selectedGroup, StringComparison.Ordinal))
                {
                    _highlightGroupCombo.Text = group;
                }
            }
        }
        finally
        {
            _suppressEvents = false;
        }
    }

    public void PopulateFileHistory (IEnumerable<string> fileHistory)
    {
        if (_lastUsedMenuItem == null)
        {
            return;
        }

        if (_uiContext != SynchronizationContext.Current)
        {
            _uiContext.Post(_ => PopulateFileHistory(fileHistory), null);
            return;
        }

        // Unsubscribe from previous dropdown events
        if (_lastUsedMenuItem.DropDown != null)
        {
            _lastUsedMenuItem.DropDown.ItemClicked -= OnHistoryMenuItemClicked;
            _lastUsedMenuItem.DropDown.MouseUp -= OnHistoryStripMouseUp;
        }

        var strip = new ToolStripDropDownMenu();

        foreach (var file in fileHistory)
        {
            _ = strip.Items.Add(new ToolStripMenuItem(file));
        }

        strip.ItemClicked += OnHistoryMenuItemClicked;
        strip.MouseUp += OnHistoryStripMouseUp;
        _lastUsedMenuItem.DropDown = strip;
    }

    #region Private Helpers

    private static void SetCheckedSafe (ToolStripMenuItem item, bool value)
    {
        _ = (item?.Checked = value);
    }

    private void OnHighlightGroupComboSelectedIndexChanged (object sender, EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        if (_highlightGroupCombo.SelectedItem is string groupName && !string.IsNullOrEmpty(groupName))
        {
            HighlightGroupSelected?.Invoke(this, new HighlightGroupSelectedEventArgs(groupName));
        }
    }

    private void OnHistoryMenuItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        var fileName = e.ClickedItem?.Text;
        if (!string.IsNullOrEmpty(fileName))
        {
            HistoryItemClicked?.Invoke(this, new HistoryItemClickedEventArgs(fileName));
        }
    }

    private void OnHistoryStripMouseUp (object sender, MouseEventArgs e)
    {
        // Right-click to remove from history (preserves existing LogTabWindow behavior)
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        if (sender is ToolStripDropDownMenu strip)
        {
            var item = strip.GetItemAt(e.Location);
            if (item != null && !string.IsNullOrEmpty(item.Text))
            {
                HistoryItemRemoveRequested?.Invoke(this, new HistoryItemClickedEventArgs(item.Text));
            }
        }
    }

    private ToolStripMenuItem FindMenuItem (string name)
    {
        return FindToolStripItem<ToolStripMenuItem>(_mainMenu, name);
    }

    private static T FindToolStripItem<T> (ToolStrip strip, string name) where T : ToolStripItem
    {
        if (strip == null)
        {
            return null;
        }

        foreach (ToolStripItem item in strip.Items)
        {
            if (item.Name == name && item is T typedItem)
            {
                return typedItem;
            }

            if (item is ToolStripDropDownItem dropDown)
            {
                var found = FindToolStripItemRecursive<T>(dropDown.DropDownItems, name);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static T FindToolStripItemRecursive<T> (ToolStripItemCollection items, string name) where T : ToolStripItem
    {
        foreach (ToolStripItem item in items)
        {
            if (item.Name == name && item is T typedItem)
            {
                return typedItem;
            }

            if (item is ToolStripDropDownItem dropDown)
            {
                var found = FindToolStripItemRecursive<T>(dropDown.DropDownItems, name);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private void LogMissingItems ()
    {
        // Log warnings for any menu items that couldn't be found during initialization
        LogIfNull(_closeFileMenuItem, "closeFileToolStripMenuItem");
        LogIfNull(_searchMenuItem, "searchToolStripMenuItem");
        LogIfNull(_filterMenuItem, "filterToolStripMenuItem");
        LogIfNull(_timeshiftMenuItem, "timeshiftToolStripMenuItem");
        LogIfNull(_encodingAsciiMenuItem, "encodingASCIIToolStripMenuItem");
        LogIfNull(_highlightGroupCombo, "highlightGroupsToolStripComboBox");
        LogIfNull(_lastUsedMenuItem, "lastUsedToolStripMenuItem");
    }

    private static void LogIfNull (object item, string name)
    {
        if (item == null)
        {
            _logger.Warn("MenuToolbarController: menu item '{0}' not found during initialization", name);
        }
    }

    #endregion

    public void Dispose ()
    {
        if (_disposed)
        {
            return;
        }

        _highlightGroupCombo?.SelectedIndexChanged -= OnHighlightGroupComboSelectedIndexChanged;

        if (_lastUsedMenuItem?.DropDown != null)
        {
            _lastUsedMenuItem.DropDown.ItemClicked -= OnHistoryMenuItemClicked;
            _lastUsedMenuItem.DropDown.MouseUp -= OnHistoryStripMouseUp;
        }

        _disposed = true;
    }
}
