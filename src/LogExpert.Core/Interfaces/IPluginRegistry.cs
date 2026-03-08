using ColumnizerLib;

namespace LogExpert.Core.Interfaces;

//TODO: Add documentation
public interface IPluginRegistry
{
    IList<ILogLineMemoryColumnizer> RegisteredColumnizers { get; }

    IFileSystemPlugin FindFileSystemForUri (string fileNameOrUri);
}