using ColumnizerLib;

namespace JsonColumnizer;

public partial class JsonColumnizer
{
    public class ColumnWithName : Column
    {
        public string ColumnName { get; set; }
    }

    #endregion
}