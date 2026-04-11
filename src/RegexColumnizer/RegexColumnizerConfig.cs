namespace RegexColumnizer;

public class RegexColumnizerConfig
{
    #region Properties

    public string Expression { get; set; } = "(?<text>.*)";

    public string Name { get; set; }

    public string CustomName { get; set; }

    #endregion
}