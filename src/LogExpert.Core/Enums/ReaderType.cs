namespace LogExpert.Core.Enums;

/// <summary>
/// Defines the available stream reader implementations.
/// </summary>
public enum ReaderType
{
    /// <summary>
    /// System.IO.Pipelines based reader implementation (high performance).
    /// </summary>
    Pipeline,

    /// <summary>
    /// Legacy reader implementation (original).
    /// </summary>
    Legacy,

    /// <summary>
    /// System.IO.StreamReader based implementation.
    /// </summary>
    System,

    /// <summary>
    /// Channel-based asynchronous reader implementation.
    /// </summary>
    Channel
}
