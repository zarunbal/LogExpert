using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace LogExpert
{
	/// <summary>
	/// This class is responsible for reading line from the log file. It also decodes characters with the appropriate charset encoding.
	/// PositionAwareStreamReader tries a BOM detection to determine correct file offsets when directly seeking into the file (on re-loading flushed buffers).
	/// UTF-8 handling is a bit slower, because after reading a character the byte length of the character must be determined.
	/// Lines are read char-by-char. StreamReader.ReadLine() is not used because StreamReader cannot tell a file position.
	/// </summary>
	public class PositionAwareStreamReader : ILogStreamReader
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
		private Encoding detectedEncoding;
		private bool useSystemReaderMethod;
		private int newLineSequenceLength;

		public PositionAwareStreamReader(Stream stream, EncodingOptions encodingOptions, bool useSystemReaderMethod)
		{
			this.useSystemReaderMethod = useSystemReaderMethod;
			this.stream = new BufferedStream(stream);
			this.preambleLength = DetectPreambleLengthAndEncoding();

			Encoding usedEncoding;
			if (this.detectedEncoding != null && encodingOptions.Encoding == null)
			{
				usedEncoding = this.detectedEncoding;
			}
			else if (encodingOptions.Encoding != null)
			{
				usedEncoding = encodingOptions.Encoding;
			}
			else
			{
				usedEncoding = encodingOptions.DefaultEncoding != null ? encodingOptions.DefaultEncoding : Encoding.Default;
			}

			if (usedEncoding is System.Text.UTF8Encoding)
				posInc_precomputed = 0;
			else if (usedEncoding is System.Text.UnicodeEncoding)
				posInc_precomputed = 2;
			else
				posInc_precomputed = 1;

			this.reader = new StreamReader(this.stream, usedEncoding, true);
			ResetReader();
			Position = 0;

			if (this.useSystemReaderMethod)
			{
				this.newLineSequenceLength = guessNewLineSequenceLength();
			}
		}

		private int guessNewLineSequenceLength()
		{
			long currentPos = Position;
			int len = 0;
			string line = this.reader.ReadLine();
			if (line != null)
			{
				this.stream.Seek(this.Encoding.GetByteCount(line) + this.preambleLength, SeekOrigin.Begin);
				ResetReader();
				int b = this.reader.Read();
				// int b = this.stream.ReadByte();
				if (b == 0x0d)
				{
					// b = this.stream.ReadByte();
					b = this.reader.Read();
					if (b == 0x0a)
					{
						len = 2;
					}
					else
					{
						len = 1;
					}
				}
				else
				{
					len = 1;
				}
				len *= this.Encoding.GetByteCount("\r");
			}
			Position = currentPos;
			return len;
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
			get
			{
				return this.pos;
			}
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
			return this.useSystemReaderMethod ? ReadLineNew() : ReadLineOld();
		}

		protected unsafe string ReadLineNew()
		{
			if (this.newLineSequenceLength == 0)
			{
				this.newLineSequenceLength = guessNewLineSequenceLength();
			}
			string line = this.reader.ReadLine();
			if (line != null)
			{
				this.pos += this.Encoding.GetByteCount(line);
				this.pos += this.newLineSequenceLength;
				if (line.Length > MAX_LINE_LEN)
				{
					line = line.Remove(MAX_LINE_LEN);
				}
			}
			return line;
		}

		protected unsafe string ReadLineOld()
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
		private int DetectPreambleLengthAndEncoding()
		{
			/*
			UTF-8:                                EF BB BF 
			UTF-16-Big-Endian-Bytereihenfolge:    FE FF 
			UTF-16-Little-Endian-Bytereihenfolge: FF FE 
			UTF-32-Big-Endian-Bytereihenfolge:    00 00 FE FF 
			UTF-32-Little-Endian-Bytereihenfolge: FF FE 00 00 
			*/
			detectedEncoding = null;
			byte[] readPreamble = new byte[4];
			int readLen = this.stream.Read(readPreamble, 0, 4);
			if (readLen < 2)
				return 0;
			Encoding[] encodings = new Encoding[]
			{
				Encoding.UTF8,
				Encoding.Unicode,
				Encoding.BigEndianUnicode,
				Encoding.UTF32
			};
			byte[][] preambles = new byte[4][]
			{
				Encoding.UTF8.GetPreamble(),
				Encoding.Unicode.GetPreamble(),
				Encoding.BigEndianUnicode.GetPreamble(),
				Encoding.UTF32.GetPreamble()
			};
			foreach (Encoding encoding in encodings)
			{
				byte[] preamble = encoding.GetPreamble();
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
					detectedEncoding = encoding;
					return preamble.Length;
				}
			}
			return 0;
		}

		public Encoding Encoding
		{
			get
			{
				return this.reader.CurrentEncoding;
			}
		}

		public bool IsBufferComplete
		{
			get
			{
				return true;
			}
		}
	}
}