namespace LogExpert.Core.Enums;

/// <summary>
/// Defines the available stream reader implementations.
/// </summary>
public enum ReaderType
{
    /// <summary>
    /// Direct-read implementation: reads decoded chars directly into pooled blocks via
    /// StreamReader.Read(char[], offset, count), eliminating per-line string allocation.
    /// </summary>
    SystemDirect,

    /// <summary>
    /// Legacy reader implementation (original).
    /// </summary>
    Legacy,

    /// <summary>
    /// System.IO.StreamReader based implementation.
    /// </summary>
    System
}
