namespace LogExpert.PluginRegistry;

/// <summary>
/// Information about plugin types contained in an assembly.
/// Helps determine the appropriate loading strategy for each plugin.
/// </summary>
public class PluginTypeInfo
{
    /// <summary>
    /// Gets or sets a value indicating whether the assembly contains ILogLineColumnizer implementations.
    /// </summary>
    public bool HasColumnizer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assembly contains IFileSystemPlugin implementations.
    /// </summary>
    public bool HasFileSystem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assembly contains IContextMenuEntry implementations.
    /// </summary>
    public bool HasContextMenu { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assembly contains IKeywordAction implementations.
    /// </summary>
    public bool HasKeywordAction { get; set; }

    /// <summary>
    /// Returns true if no plugin types were found in the assembly.
    /// </summary>
    public bool IsEmpty => !HasColumnizer && !HasFileSystem && 
                          !HasContextMenu && !HasKeywordAction;

    /// <summary>
    /// Returns true if exactly one plugin type was found in the assembly.
    /// Single-type assemblies are candidates for lazy loading.
    /// </summary>
    public bool IsSingleType => 
        (HasColumnizer ? 1 : 0) + 
        (HasFileSystem ? 1 : 0) + 
        (HasContextMenu ? 1 : 0) + 
        (HasKeywordAction ? 1 : 0) == 1;

    /// <summary>
    /// Returns true if only columnizer plugins were found (no other types).
    /// </summary>
    public bool IsColumnizerOnly => HasColumnizer && !HasFileSystem && 
                                   !HasContextMenu && !HasKeywordAction;

    /// <summary>
    /// Returns true if the assembly contains multiple plugin types.
    /// Mixed assemblies should be loaded immediately to ensure all types are available.
    /// </summary>
    public bool IsMultiType => !IsEmpty && !IsSingleType;

    /// <summary>
    /// Gets the count of plugin types found in the assembly.
    /// </summary>
    public int TypeCount => 
        (HasColumnizer ? 1 : 0) + 
        (HasFileSystem ? 1 : 0) + 
        (HasContextMenu ? 1 : 0) + 
        (HasKeywordAction ? 1 : 0);
}
