using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Entities;

public class FileViewContext(ILogPaintContext logPaintContext, ILogView logView) : IFileViewContext, ILogPaintContext
{
    #region Properties

    public ILogPaintContext LogPaintContext { get; } = logPaintContext;

    public ILogView LogView { get; } = logView;

    #endregion
}