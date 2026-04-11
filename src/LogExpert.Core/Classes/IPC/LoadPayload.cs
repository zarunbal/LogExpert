namespace LogExpert.Core.Classes.IPC;

public class LoadPayload
{
    public List<string> Files { get; set; } = [];

    public override string? ToString ()
    {
        return string.Join(", ", Files);
    }
}
