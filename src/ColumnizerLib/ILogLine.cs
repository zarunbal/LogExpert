using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
    public interface ILogLine : ITextValue
    {
        #region Properties

        string FullLine { get; }

        int LineNumber { get; }

        #endregion
    }
}