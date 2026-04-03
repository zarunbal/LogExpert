using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Services.LogWindowCoordinatorService;

/// <summary>
/// Coordinates workspace-level operations for LogWindow instances.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class LogWindowCoordinator (IConfigManager configManager) : ILogWindowCoordinator
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigManager _configManager = configManager;
    private readonly Lock _highlightGroupLock = new();

    /// <summary>
    /// The current list of highlight groups. This is owned by Preferences and
    /// updated via <see cref="UpdateHighlightGroups"/>.
    /// </summary>
    private List<HighlightGroup> HighlightGroupList { get; set; } = [];

    public event EventHandler HighlightSettingsChanged;

    /// <summary>
    /// Updates the highlight group list (called after settings change).
    /// </summary>
    public void UpdateHighlightGroups (List<HighlightGroup> groups)
    {
        lock (_highlightGroupLock)
        {
            HighlightGroupList = groups;
        }
    }

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
            if (HighlightGroupList.Count > 0)
            {
                return HighlightGroupList[0];
            }

            // Tier 4: New empty group (never returns null)
            return new HighlightGroup();
        }
    }

    private HighlightGroup? FindHighlightGroupByName (string groupName)
    {
        foreach (var group in HighlightGroupList)
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
}
