using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogExpert
{
    public interface IAutoColumnizer
    {
        ILogLineColumnizer FindColumnizer(string fileName, IAutoLogLineColumnizerCallback logFileReader);
    }
}