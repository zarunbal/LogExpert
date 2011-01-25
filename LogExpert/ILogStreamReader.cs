using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  interface ILogStreamReader
  {
    int ReadChar();
    string ReadLine();

    long Position
    {
      get; set;
    }

    bool IsBufferComplete
    {
      get;
    }

    Encoding Encoding
    {
      get;
    }
  }
}
