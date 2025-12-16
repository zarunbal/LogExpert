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

    /// <summary>
    /// Gets or sets the full file path to the project file.
    /// </summary>
    public string ProjectFilePath { get; set; }

    #endregion
}