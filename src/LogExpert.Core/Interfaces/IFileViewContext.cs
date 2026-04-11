using LogExpert.Core.Entities;

namespace LogExpert.Core.Interfaces;

public interface IFileViewContext
{
    ILogView LogView { get; }
    ILogPaintContext LogPaintContext { get; }
}