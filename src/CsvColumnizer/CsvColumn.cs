namespace CsvColumnizer
{
    internal class CsvColumn
    {
        #region cTor

        public CsvColumn(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; }

        #endregion
    }
}