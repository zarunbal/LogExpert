namespace ColumnizerLib;

/// <summary>
/// Defines a method that determines the priority of a columnizer for a given file and sample log lines.
/// </summary>
/// <remarks>Implementations use the provided file name and sample log lines to assess how suitable the columnizer
/// is for processing the file. Higher priority values indicate a better fit. This interface is typically used to select
/// the most appropriate columnizer when multiple options are available.</remarks>
[Obsolete("This interface is deprecated. Use IColumnizerPriorityMemory instead for a memory based implementation.")]
public interface IColumnizerPriority
{
    /// <summary>
    /// Get the priority for this columnizer so the up layer can decide which columnizer is the best fitted one.
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Priority GetPriority (string fileName, IEnumerable<ILogLine> samples);
}