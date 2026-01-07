using ColumnizerLib;

namespace LogExpert.Core.EventArguments;

public class ContextMenuPluginEventArgs (IContextMenuEntry entry, IList<int> logLines, ILogLineMemoryColumnizer columnizer,
    ILogExpertCallbackMemory callback) : EventArgs
{

    #region Properties

    public IContextMenuEntry Entry { get; } = entry;

    public IList<int> LogLines { get; } = logLines;

    public ILogLineMemoryColumnizer Columnizer { get; } = columnizer;

    public ILogExpertCallbackMemory Callback { get; } = callback;

    #endregion
}