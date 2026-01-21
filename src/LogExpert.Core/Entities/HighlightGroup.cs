using LogExpert.Core.Classes.Highlight;

namespace LogExpert.Core.Entities;

[Serializable]
public class HighlightGroup : ICloneable
{
    #region Properties

    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// List of highlight entries defining text patterns and their formatting.
    /// </summary>
    /// <remarks>
    /// Supports legacy property name "hilightEntryList" (with typo) for backward compatibility.
    /// Old settings files using the incorrect spelling will be automatically imported.
    /// </remarks>
    [Newtonsoft.Json.JsonProperty("HighlightEntryList")]
    [System.Text.Json.Serialization.JsonPropertyName("HighlightEntryList")]
    public List<HighlightEntry> HighlightEntryList { get; set; } = [];

    /// <summary>
    /// Legacy property for backward compatibility with old settings files that used the typo "hilightEntryList".
    /// This setter redirects data to the correct <see cref="HighlightEntryList"/> property.
    /// Will be removed in a future version once migration period is complete.
    /// </summary>
    [Obsolete("This property exists only for backward compatibility with old settings files. Use HighlightEntryList instead.")]
    [Newtonsoft.Json.JsonProperty("hilightEntryList")]
    [System.Text.Json.Serialization.JsonPropertyName("hilightEntryList")]
    public List<HighlightEntry> HilightEntryList
    {
        get => HighlightEntryList;
        set => HighlightEntryList = value ?? [];
    }

    public object Clone ()
    {
        HighlightGroup clone = new()
        {
            GroupName = GroupName
        };

        foreach (var entry in HighlightEntryList)
        {
            clone.HighlightEntryList.Add((HighlightEntry)entry.Clone());
        }

        return clone;
    }

    #endregion
}