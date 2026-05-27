using ColumnizerLib;

using LogExpert.Core.Config;

namespace LogExpert.Core.Classes.Columnizer;

/// <summary>
/// Pure precedence-chain resolver that picks a columnizer for a given file from up to four sources:
/// per-file persistence, columnizer history, columnizer-mask list, and AutoPick.
/// </summary>
/// <remarks>
/// The exact order of consultation is controlled by <see cref="ColumnizerSelectionPriority"/>.
/// AutoPick fires only when every other source produced <see langword="null"/>; it never outranks
/// an explicit Mask, History, or Persistence hit.
/// <para>
/// Stale Mask entries — those whose <see cref="ColumnizerMaskEntry.ColumnizerName"/> is not registered —
/// are skipped (an optional callback is invoked once per skipped entry) and resolution continues with the
/// next entry in the list.
/// </para>
/// </remarks>
public static class ColumnizerResolver
{
    /// <summary>
    /// Inputs to <see cref="Resolve"/>. Designed so the module is exercisable from unit tests with no
    /// dependency on settings storage, plugin registry singletons, or UI.
    /// </summary>
    public sealed class ResolveInputs
    {
        public ColumnizerSelectionPriority Priority { get; init; }

        /// <summary>The full path or identifier of the file being opened.</summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>The short (filename-only) form of <see cref="FileName"/>, used for mask matching.</summary>
        public string ShortFileName { get; init; } = string.Empty;

        public IReadOnlyList<ColumnizerMaskEntry> MaskList { get; init; } = [];

        /// <summary>Lookup of a saved history columnizer name for <see cref="FileName"/>. May be <see langword="null"/>.</summary>
        public Func<string, string?>? HistoryLookup { get; init; }

        /// <summary>Columnizer name supplied by the per-file persistence (<c>.lxp</c>). May be <see langword="null"/>.</summary>
        public string? PersistenceColumnizerName { get; init; }

        /// <summary>AutoPick callback (e.g. content-based detection). May be <see langword="null"/>.</summary>
        public Func<ILogLineMemoryColumnizer?>? AutoPick { get; init; }

        public IList<ILogLineMemoryColumnizer> Registered { get; init; } = [];

        /// <summary>Invoked once for each Mask entry that matched but referenced a missing columnizer.</summary>
        public Action<ColumnizerMaskEntry>? OnStaleMaskEntry { get; init; }
    }

    /// <summary>
    /// Returns the winning columnizer for the inputs, or <see langword="null"/> if no source produced a match.
    /// </summary>
    public static ILogLineMemoryColumnizer? Resolve (ResolveInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        ILogLineMemoryColumnizer? Mask () => TryGetMaskColumnizer(inputs.MaskList, inputs.ShortFileName, inputs.Registered, inputs.OnStaleMaskEntry);
        ILogLineMemoryColumnizer? History () => TryLookupByName(inputs.HistoryLookup?.Invoke(inputs.FileName), inputs.Registered);
        ILogLineMemoryColumnizer? Persistence () => TryLookupByName(inputs.PersistenceColumnizerName, inputs.Registered);

        var winner = inputs.Priority switch
        {
            ColumnizerSelectionPriority.MaskThenHistory => Persistence() ?? Mask() ?? History(),
            ColumnizerSelectionPriority.MaskOverridesPersistence => Mask() ?? Persistence() ?? History(),
            _ => Persistence() ?? History() ?? Mask(),
        };

        return winner ?? inputs.AutoPick?.Invoke();
    }

    /// <summary>
    /// Iterates the mask list and returns the first non-stale match. Entries whose columnizer is not registered
    /// invoke <paramref name="onStale"/> and the iteration continues. Never throws.
    /// </summary>
    public static ILogLineMemoryColumnizer? TryGetMaskColumnizer (
        IReadOnlyList<ColumnizerMaskEntry> maskList,
        string shortFileName,
        IList<ILogLineMemoryColumnizer> registered,
        Action<ColumnizerMaskEntry>? onStale = null)
    {
        if (maskList == null || maskList.Count == 0 || string.IsNullOrEmpty(shortFileName))
        {
            return null;
        }

        foreach (var entry in maskList)
        {
            if (!ColumnizerMaskMatcher.Matches(entry, shortFileName))
            {
                continue;
            }

            var columnizer = TryLookupByName(entry.ColumnizerName, registered);
            if (columnizer != null)
            {
                return columnizer;
            }

            onStale?.Invoke(entry);
        }

        return null;
    }

    private static ILogLineMemoryColumnizer? TryLookupByName (string? name, IList<ILogLineMemoryColumnizer> registered)
    {
        if (string.IsNullOrEmpty(name) || registered == null)
        {
            return null;
        }

        foreach (var c in registered)
        {
            if (c.GetName().Equals(name, StringComparison.Ordinal))
            {
                return c;
            }
        }

        return null;
    }
}
