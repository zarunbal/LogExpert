namespace ColumnizerLib;

/// <summary>
/// Defines a contract for determining the priority level of a file based on log line memory samples.
/// </summary>
/// <remarks>Implementations use the provided file name and sample log lines to assess how suitable the columnizer
/// is for processing the file. Higher priority values indicate a better fit. This interface is typically used to select
/// the most appropriate columnizer when multiple options are available.</remarks>
public interface IColumnizerPriorityMemory : IColumnizerPriority
{
    /// <summary>
    /// Determines the priority level for the specified file based on the provided log line samples.
    /// </summary>
    /// <param name="fileName">The name of the file for which to determine the priority. Cannot be null or empty.</param>
    /// <param name="samples">A collection of log line memory samples used to assess the file's priority. Cannot be null.</param>
    /// <returns>A value of the Priority enumeration that represents the determined priority for the specified file.</returns>
    Priority GetPriority (string fileName, IEnumerable<ILogLineMemory> samples);
}