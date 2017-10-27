using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
    public class ColumnizedLogLine : IColumnizedLogLine
    {
        #region Properties

        public ILogLine LogLine { get; set; }
        public IColumn[] ColumnValues { get; set; }

        #endregion
    }
}