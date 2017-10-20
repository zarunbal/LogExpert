using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColumnizerLib
{
    public interface ILogLine
    {
        string FullLine { get; }

        int LineNumber { get; }

        string DisplayLine { get;  }
    }
}
