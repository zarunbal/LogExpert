using LogExpert.UI.Controls.LogWindow;

namespace LogExpert.UI.Entities;

internal class LogWindowMetadata
{
    public LogWindow Window { get; set; }

    public string Title { get; set; }

    public string FileName { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsTempFile { get; set; }

    public Color TabColor { get; set; }

    public object Tag { get; set; }
}
