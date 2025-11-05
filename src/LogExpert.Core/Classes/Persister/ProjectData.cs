namespace LogExpert.Core.Classes.Persister;

[Serializable]
public class ProjectData
{
    #region Fields

    /// <summary>
    /// Gets or sets the list of members.
    /// </summary>
    public List<string> FileNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the XML representation of the tab layout configuration.
    /// </summary>
    public string TabLayoutXml { get; set; }

    #endregion
}