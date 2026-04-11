namespace LogExpert.Core.Entities;

[Serializable]
public class SearchParams
{
    public int CurrentLine { get; set; }

    public List<string> HistoryList { get; set; } = [];

    public bool IsCaseSensitive { get; set; }

    public bool IsFindNext { get; set; }

    public bool IsForward { get; set; } = true;

    public bool IsFromTop { get; set; }

    public bool IsRegex { get; set; }

    public string SearchText { get; set; } = string.Empty;

    [field: NonSerialized]
    public bool IsShiftF3Pressed { get; set; }

    public void CopyFrom (SearchParams other)
    {
        ArgumentNullException.ThrowIfNull(other);

        CurrentLine = other.CurrentLine;
        HistoryList = other.HistoryList;
        IsCaseSensitive = other.IsCaseSensitive;
        IsFindNext = other.IsFindNext;
        IsForward = other.IsForward;
        IsFromTop = other.IsFromTop;
        IsRegex = other.IsRegex;
        SearchText = other.SearchText;
        IsShiftF3Pressed = other.IsShiftF3Pressed;
    }
}