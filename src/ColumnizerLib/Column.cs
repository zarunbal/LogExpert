using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
    public class Column : IColumn
    {
        #region Fields

        private static readonly int _maxLength = 4678 - 3;

        private string _fullValue;

        #endregion

        #region Properties

        public static IColumn EmptyColumn { get; } = new Column {FullValue = string.Empty};

        public IColumnizedLogLine Parent { get; set; }

        public string FullValue
        {
            get { return _fullValue; }
            set
            {
                _fullValue = value;
                if (_fullValue.Length > _maxLength)
                {
                    DisplayValue = _fullValue.Replace("\t", "  ").Substring(0, _maxLength) + "...";
                }
                else
                {
                    DisplayValue = _fullValue.Replace("\t", "  ");
                }
            }
        }

        public string DisplayValue { get; private set; }

        string ITextValue.Text => FullValue;

        #endregion

        #region Public methods

        public static Column[] CreateColumns(int count, IColumnizedLogLine parent)
        {
            return CreateColumns(count, parent, string.Empty);
        }

        public static Column[] CreateColumns(int count, IColumnizedLogLine parent, string defaultValue)
        {
            Column[] output = new Column[count];

            for (int i = 0; i < count; i++)
            {
                output[i] = new Column {FullValue = defaultValue, Parent = parent};
            }

            return output;
        }

        public override string ToString()
        {
            return DisplayValue ?? "";
        }

        #endregion
    }
}