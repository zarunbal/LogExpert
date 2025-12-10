namespace LogExpert.Core.Classes.Persister;

public class ProjectValidationResult
{
    public List<string> ValidFiles { get; } = new();
    public List<string> MissingFiles { get; } = new();
    public Dictionary<string, List<string>> PossibleAlternatives { get; } = new();

    public bool HasMissingFiles => MissingFiles.Count > 0;
}
