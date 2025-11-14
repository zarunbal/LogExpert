using System.Text;

namespace LogExpert.Core.Classes.IPC;

public class LoadPayload
{
    public List<string> Files { get; set; } = [];

    public override string? ToString ()
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder.Append(string.Join(", ", Files));
        return stringBuilder.ToString();
    }
}
