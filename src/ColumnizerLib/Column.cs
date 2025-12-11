namespace ColumnizerLib;

public class Column : IColumnMemory
{
    //TODO Memory Functions need implementation
    #region Fields

    private const string REPLACEMENT = "...";

    // Display-level maximum line length (separate from reader-level limit)
    // Can be configured via SetMaxDisplayLength()
    private static int _maxDisplayLength = 20_000;

    private static readonly List<Func<string, string>> _replacements = [
        //replace tab with 3 spaces, from old coding. Needed???
                input => input.Replace("\t", "  ", StringComparison.Ordinal),

                //shorten string if it exceeds maxLength
                input => input.Length > _maxDisplayLength
                        ? string.Concat(input.AsSpan(0, _maxDisplayLength), REPLACEMENT)
                        : input
    ];

    #endregion

    #region cTor

    static Column ()
    {
        if (Environment.Version >= Version.Parse("6.2"))
        {
            //Win8 or newer support full UTF8 chars with the preinstalled fonts.
            //Replace null char with UTF8 Symbol U+2400 (␀)
            _replacements.Add(input => input.Replace("\0", "␀", StringComparison.Ordinal));
        }
        else
        {
            //Everything below Win8 the installed fonts seems to not to support reliabel
            //Replace null char with space
            //.net 10 does no longer support windows lower then windows 10
            //TODO: remove if with one of the next releases
            //https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md
            _replacements.Add(input => input.Replace("\0", " ", StringComparison.Ordinal));
        }

        EmptyColumn = new Column { FullValue = string.Empty };
    }

    #endregion

    #region Properties

    public static IColumnMemory EmptyColumn { get; }

    public IColumnizedLogLineMemory Parent { get; set; }

    public string FullValue
    {
        get;
        set
        {
            field = value;

            var temp = FullValue;

            foreach (var replacement in _replacements)
            {
                temp = replacement(temp);
            }

            DisplayValue = temp;
        }
    }

    public string DisplayValue { get; private set; }

    public string Text => DisplayValue;

    public IColumnizedLogLineMemory ParentMemory { get; }

    public ReadOnlyMemory<char> FullValueMemory
    {
        get;
        set; //implement
    }

    public ReadOnlyMemory<char> DisplayValueMemory { get; }

    public ReadOnlyMemory<char> TextMemory { get; }
    IColumnizedLogLine IColumn.Parent { get; }

    #endregion

    #region Public methods

    /// <summary>
    /// Configures the maximum display length for all Column instances.
    /// This is separate from the reader-level MaxLineLength.
    /// </summary>
    /// <param name="maxLength">Maximum length for displayed content. Must be at least 1000.</param>
    public static void SetMaxDisplayLength (int maxLength)
    {
        if (maxLength < 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), Resources.Column_Error_Messages_MaximumDisplayLengthMustBeAtLeast1000Characters);
        }

        _maxDisplayLength = maxLength;
    }

    /// <summary>
    /// Gets the current maximum display length setting.
    /// </summary>
    public static int GetMaxDisplayLength () => _maxDisplayLength;

    public static Column[] CreateColumns (int count, IColumnizedLogLineMemory parent)
    {
        return CreateColumns(count, parent, string.Empty);
    }

    public static Column[] CreateColumns (int count, IColumnizedLogLineMemory parent, string defaultValue)
    {
        var output = new Column[count];

        for (var i = 0; i < count; i++)
        {
            output[i] = new Column { FullValue = defaultValue, Parent = parent };
        }

        return output;
    }

    public override string ToString ()
    {
        return DisplayValue ?? string.Empty;
    }

    #endregion
}