using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using ColumnizerLib;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Services.LogWindowCoordinatorService;

/// <summary>
/// Coordinates workspace-level operations for LogWindow instances.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class LogWindowCoordinator (IConfigManager configManager, IPluginRegistry pluginRegistry, Controls.LogTabWindow.LogTabWindow logTabWindow) : ILogWindowCoordinator
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigManager _configManager = configManager;
    private readonly IPluginRegistry _pluginRegistry = pluginRegistry;
    private readonly Controls.LogTabWindow.LogTabWindow _logTabWindow = logTabWindow;
    private readonly Lock _highlightGroupLock = new();

    public event EventHandler HighlightSettingsChanged;

    private List<HighlightGroup> HighlightGroups => _configManager.Settings.Preferences.HighlightGroupList;

    public SearchParams SearchParams { get; } = new SearchParams();

    /// <summary>
    /// Raises the HighlightSettingsChanged event.
    /// </summary>
    public void OnHighlightSettingsChanged ()
    {
        HighlightSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public HighlightGroup ResolveHighlightGroup (string? groupName, string? fileName)
    {
        lock (_highlightGroupLock)
        {
            // Tier 1: File-mask regex match (if fileName is provided)
            if (fileName != null)
            {
                var maskMatch = FindHighlightGroupByFileMask(fileName);
                if (maskMatch != null)
                {
                    return maskMatch;
                }
            }

            // Tier 2: Name match (if groupName is provided)
            if (groupName != null)
            {
                var nameMatch = FindHighlightGroupByName(groupName);
                if (nameMatch != null)
                {
                    return nameMatch;
                }
            }

            // Tier 3: First group in the list
            if (HighlightGroups.Count > 0)
            {
                return HighlightGroups[0];
            }

            // Tier 4: New empty group (never returns null)
            return new HighlightGroup();
        }
    }

    private HighlightGroup? FindHighlightGroupByName (string groupName)
    {
        foreach (var group in HighlightGroups)
        {
            if (group.GroupName.Equals(groupName, StringComparison.Ordinal))
            {
                return group;
            }
        }

        return null;
    }

    private HighlightGroup? FindHighlightGroupByFileMask (string fileName)
    {
        foreach (var entry in _configManager.Settings.Preferences.HighlightMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        return FindHighlightGroupByName(entry.HighlightGroupName);
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error($"RegEx-error while matching highlight mask: {e}");
                }
            }
        }

        return null;
    }

    public ILogLineMemoryColumnizer? ResolveColumnizer (string fileName)
    {
        var preferences = _configManager.Settings.Preferences;
        var shortName = Util.GetNameFromPath(fileName);

        return preferences.MaskPrio
            ? FindColumnizerByFileMask(shortName) ?? GetColumnizerHistoryEntry(fileName)
            : GetColumnizerHistoryEntry(fileName) ?? FindColumnizerByFileMask(shortName);
    }

    private ILogLineMemoryColumnizer? FindColumnizerByFileMask (string fileName)
    {
        foreach (var entry in _configManager.Settings.Preferences.ColumnizerMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        return ColumnizerPicker.FindMemorColumnizerByName(
                            entry.ColumnizerName,
                            _pluginRegistry.RegisteredColumnizers);
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error($"RegEx-error while finding columnizer: {e}");
                }
            }
        }

        return null;
    }

    private ILogLineMemoryColumnizer? GetColumnizerHistoryEntry (string fileName)
    {
        var historyEntry = FindColumnizerHistoryEntry(fileName);
        if (historyEntry != null)
        {
            foreach (var columnizer in _pluginRegistry.RegisteredColumnizers)
            {
                if (columnizer.GetName().Equals(historyEntry.ColumnizerName, StringComparison.Ordinal))
                {
                    return columnizer;
                }
            }

            // Stale entry — columnizer name no longer registered. Remove it.
            _ = _configManager.Settings.ColumnizerHistoryList.Remove(historyEntry);
        }

        return null;
    }

    public ColumnizerHistoryEntry? FindColumnizerHistoryEntry (string fileName)
    {
        foreach (var entry in _configManager.Settings.ColumnizerHistoryList)
        {
            if (entry.FileName.Equals(fileName, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    public LogWindow AddFilterTab (FilterPipe pipe, string title, ILogLineMemoryColumnizer? preProcessColumnizer)
    {
        return _logTabWindow.AddFilterTab(pipe, title, preProcessColumnizer);
    }

    public LogWindow AddTempFileTab (string fileName, string title)
    {
        return _logTabWindow.AddTempFileTab(fileName, title);
    }
}
