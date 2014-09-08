using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  internal class LogFileException : ApplicationException
  {
    internal LogFileException(string msg)
      : base(msg)
    {
    }

    internal LogFileException(string msg, Exception inner)
      : base(msg, inner)
    {
    }

  }
}
