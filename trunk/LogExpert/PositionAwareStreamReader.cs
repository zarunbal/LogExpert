using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace LogExpert
{
  class PositionAwareStreamReader : ILogStreamReader
  {
    const int MAX_LINE_LEN = 20000;
    Stream stream;
    StreamReader reader;
    //StringBuilder builder;
    int state;
    long pos;
    private int posInc_precomputed;
    private char[] charBuffer = new char[MAX_LINE_LEN];
    private int charBufferPos = 0;
    private int preambleLength = 0;


    public PositionAwareStreamReader(Stream stream, Encoding encoding)
    {
      this.stream = new BufferedStream(stream);
      this.reader = new StreamReader(this.stream, encoding, true);
      ResetReader();
      pos = 0;
      if (encoding is System.Text.UTF8Encoding)
        posInc_precomputed = 0;
      else if (encoding is System.Text.UnicodeEncoding)
        posInc_precomputed = 2;
      else
        posInc_precomputed = 1;
      Position = 0;
      this.preambleLength = DetectPreambleLength();
      Position = 0;
    }

    public void Close()
    {
      this.stream.Close();
    }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public long Position
    {
      get { return this.pos; }
      set
      {
        /*
         * 1: Irgendwann mal auskommentiert (+this.Encoding.GetPreamble().Length)
         * 2: Stand bei 1.1 3207
         * 3: Stand nach Fehlermeldung von Piet wegen Unicode-Bugs. 
         *    Keihne Ahnung, ob das jetzt endgültig OK ist.
         * 4: 27.07.09: Preamble-Length wird jetzt im CT ermittelt, da Encoding.GetPreamble().Length
         *    immer eine fixe Länge liefert (unabhängig vom echtem Dateiinhalt)
         */
        this.pos = value; //  +this.Encoding.GetPreamble().Length;      // 1
        //this.stream.Seek(this.pos, SeekOrigin.Begin);     // 2
        //this.stream.Seek(this.pos + this.Encoding.GetPreamble().Length, SeekOrigin.Begin);  // 3
        this.stream.Seek(this.pos + this.preambleLength, SeekOrigin.Begin);  // 4
        ResetReader();
      }
    }

    public unsafe int ReadChar()
    {
      int readInt;
      try
      {
        readInt = this.reader.Read();
        if (readInt != -1)
        {
          char readChar = (char)readInt;
          int posInc = posInc_precomputed != 0
                           ? posInc_precomputed
                           : this.reader.CurrentEncoding.GetByteCount(&readChar, 1);
          this.pos += posInc;
        }
      }
      catch (IOException)
      {
        readInt = -1;
      }
      return readInt;
    }


    public unsafe string ReadLine()
    {
      string result;
      int readInt;

      while (-1 != (readInt = ReadChar()))
      {
        char readChar = (char)readInt;

        // state: 0: looking for \r or \n, 
        //        1: looking for \n after \r
        //        2: looking for 
        switch (readChar)
        {
          case '\r':
            switch (state)
            {
              case 0:
                state = 1;
                break;
              case 1:
                result = GetLineAndResetBuilder();
                return result;
            }
            break;
          case '\n':
            switch (state)
            {
              case 0:
                // fall through
              case 1:
                result = GetLineAndResetBuilder();
                return result;
            }
            break;
          default:
            switch (state)
            {
              case 0:
                appendToBuilder(readChar);
                break;
              case 1:
                appendToBuilder(readChar);
                state = 0;
                break;
            }
            break;
        }
        //if (this.builder.Length > MAX_LINE_LEN)
        //  break;
      }
      //if (builder.Length == 0)
      //  return null;  // EOF
      result = GetLineAndResetBuilder();
      //if (result.Length == 0)
      //  return null;  // EOF
      if (readInt == -1 && result.Length == 0)
        return null;   // EOF
      return result;
    }


    //private string GetLineAndResetBuilder()
    //{
    //  string result;
    //  result = this.builder.ToString();
    //  this.state = 0;
    //  if (this.builder.Length > MAX_LINE_LEN)
    //    result = result.Substring(0, MAX_LINE_LEN);
    //  NewBuilder();
    //  return result;
    //}


    //private void appendToBuilder(char[] readChar)
    //{
    //  this.builder.Append(Char.ToString(readChar[0]));
    //}


    //private void NewBuilder()
    //{
    //  this.builder = new StringBuilder(400);
    //}

    private unsafe string GetLineAndResetBuilder()
    {
      string result = new string(this.charBuffer, 0, this.charBufferPos);
      NewBuilder();
      this.state = 0;
      return result;
    }


    private void appendToBuilder(char readChar)
    {
      if (this.charBufferPos >= MAX_LINE_LEN)
        return;
      this.charBuffer[this.charBufferPos++] = readChar;
    }


    private void NewBuilder()
    {
      this.charBufferPos = 0;
    }


    private void ResetReader()
    {
      state = 0;
      NewBuilder();
      this.reader.DiscardBufferedData();
    }

    /// <summary>
    /// Determines the actual number of preamble bytes in the file.
    /// </summary>
    /// <returns></returns>
    private int DetectPreambleLength()
    {
      /*
      UTF-8:                                EF BB BF 
      UTF-16-Big-Endian-Bytereihenfolge:    FE FF 
      UTF-16-Little-Endian-Bytereihenfolge: FF FE 
      UTF-32-Big-Endian-Bytereihenfolge:    00 00 FE FF 
      UTF-32-Little-Endian-Bytereihenfolge: FF FE 00 00 
      */

      byte [] readPreamble = new byte[4];
      int readLen = this.stream.Read(readPreamble, 0, 4);
      if (readLen < 2)
        return 0;
      byte[][] preambles = new byte[4][] { UTF8Encoding.UTF8.GetPreamble(),
                                         UTF8Encoding.Unicode.GetPreamble(),
                                         UTF8Encoding.BigEndianUnicode.GetPreamble(),
                                         UTF8Encoding.UTF32.GetPreamble()
                                       };
      foreach (byte [] preamble in preambles)
      {
        bool fail = false;
        for (int i = 0; i < readLen && i < preamble.Length; ++i)
        {
          if (readPreamble[i] != preamble[i])
          {
            fail = true;
            break;
          }
        }
        if (!fail)
        {
          return preamble.Length;
        }
      }
      return 0;
    }

    public Encoding Encoding
    {
      get { return this.reader.CurrentEncoding; }
    }

    public bool IsBufferComplete
    {
      get { return true; } 
    }
  }
}
