namespace ColumnizerLib;

public class Column : IColumnMemory
{
    //TODO Memory Functions need implementation
    #region Fields

    private const string REPLACEMENT = "...";

    // Display-level maximum line length (separate from reader-level limit)
    // Can be configured via SetMaxDisplayLength()
    private static int _maxDisplayLength = 20_000;

    private static readonly List<Func<ReadOnlyMemory<char>, ReadOnlyMemory<char>>> _replacementsMemory = [
        //replace tab with 3 spaces, from old coding. Needed???
        ReplaceTab,

        //shorten string if it exceeds maxLength
        input => input.Length > _maxDisplayLength
                ? ShortenMemory(input, _maxDisplayLength)
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
            _replacementsMemory.Add(input => ReplaceNullChar(input, '␀'));
        }
        else
        {
            //Everything below Win8 the installed fonts seems to not to support reliabel
            //Replace null char with space
            //.net 10 does no longer support windows lower then windows 10
            //TODO: remove if with one of the next releases
            //https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md
            _replacementsMemory.Add(input => ReplaceNullChar(input, ' '));
        }

        EmptyColumn = new Column { FullValue = ReadOnlyMemory<char>.Empty };
    }

    #endregion

    #region Properties

    public static IColumnMemory EmptyColumn { get; }

    [Obsolete]
    IColumnizedLogLine IColumn.Parent { get; }

    [Obsolete]
    string IColumn.FullValue
    {
        get;
        //set
        //{
        //    field = value;

        //    var temp = FullValue.ToString();

        //    foreach (var replacement in _replacements)
        //    {
        //        temp = replacement(temp);
        //    }

        //    DisplayValue = temp.AsMemory();
        //}
    }

    [Obsolete("Use the DisplayValue property of IColumnMemory")]
    string IColumn.DisplayValue { get; }

    [Obsolete("Use Text property of ITextValueMemory")]
    string ITextValue.Text => DisplayValue.ToString();

    public IColumnizedLogLineMemory Parent
    {
        get; set => field = value;
    }

    public ReadOnlyMemory<char> FullValue
    {
        get;
        set
        {
            field = value;

            var temp = value;

            foreach (var replacement in _replacementsMemory)
            {
                temp = replacement(temp);
            }

            DisplayValue = temp;
        }
    }

    public ReadOnlyMemory<char> DisplayValue { get; private set; }

    public ReadOnlyMemory<char> Text => DisplayValue;

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
        return CreateColumns(count, parent, ReadOnlyMemory<char>.Empty);
    }

    public static Column[] CreateColumns (int count, IColumnizedLogLineMemory parent, ReadOnlyMemory<char> defaultValue)
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
        return DisplayValue.ToString() ?? ReadOnlyMemory<char>.Empty.ToString();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Replaces tab characters with two spaces in the memory buffer.
    /// </summary>
    private static ReadOnlyMemory<char> ReplaceTab (ReadOnlyMemory<char> input)
    {
        var span = input.Span;
        var tabIndex = span.IndexOf('\t');

        if (tabIndex == -1)
        {
            return input;
        }

        // Count total tabs to calculate new length
        var tabCount = 0;
        foreach (var c in span)
        {
            if (c == '\t')
            {
                tabCount++;
            }
        }

        // Allocate new buffer: original length + (tabCount * 1) since we replace 1 char with 2
        var newLength = input.Length + tabCount;
        var buffer = new char[newLength];
        var bufferPos = 0;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == '\t')
            {
                buffer[bufferPos++] = ' ';
                buffer[bufferPos++] = ' ';
            }
            else
            {
                buffer[bufferPos++] = span[i];
            }
        }

        return buffer;
    }

    /// <summary>
    /// Shortens the memory buffer to the specified maximum length and appends "...".
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Non Localiced Parameter")]
    private static ReadOnlyMemory<char> ShortenMemory (ReadOnlyMemory<char> input, int maxLength)
    {
        var buffer = new char[maxLength + REPLACEMENT.Length];
        input.Span[..maxLength].CopyTo(buffer);
        REPLACEMENT.AsSpan().CopyTo(buffer.AsSpan(maxLength));
        return buffer;
    }

    /// <summary>
    /// Replaces null characters with the specified replacement character.
    /// </summary>
    private static ReadOnlyMemory<char> ReplaceNullChar (ReadOnlyMemory<char> input, char replacement)
    {
        var span = input.Span;
        var nullIndex = span.IndexOf('\0');

        if (nullIndex == -1)
        {
            return input;
        }

        // Need to create a new buffer since we're modifying content
        var buffer = new char[input.Length];
        span.CopyTo(buffer);

        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == '\0')
            {
                buffer[i] = replacement;
            }
        }

        return buffer;
    }

    #endregion
}