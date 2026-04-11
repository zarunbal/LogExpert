using ColumnizerLib;

namespace LogExpert.Core.EventArguments;

public class ColumnizerEventArgs(ILogLineMemoryColumnizer columnizer) : System.EventArgs
{
    #region Properties

    public ILogLineMemoryColumnizer Columnizer { get; } = columnizer;

    #endregion
}