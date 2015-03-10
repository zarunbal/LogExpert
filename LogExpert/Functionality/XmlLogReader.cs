using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	class XmlLogReader : ILogStreamReader
	{
		private PositionAwareStreamReader _reader;
		private string _endTag = "</log4j:event>";
		//private const int MAX_BUFFER_LEN = 4096;
		//private char[] buffer = new char[MAX_BUFFER_LEN];
		//private int bufferPos = 0;
		private StringBuilder _buffer = new StringBuilder();

		public XmlLogReader(PositionAwareStreamReader reader)
		{
			StartTag = "<log4j:event";
			EndTag = "</log4j:event>";
			_reader = reader;
		}

		public long Position
		{
			get
			{
				return _reader.Position;
			}
			set
			{
				_reader.Position = value;
			}
		}

		public Encoding Encoding
		{
			get
			{
				return _reader.Encoding;
			}
		}

		public bool IsBufferComplete
		{
			get
			{
				return _reader.IsBufferComplete;
			}
		}

		public string StartTag { get; set; }

		public string EndTag { get; set; }

		public int ReadChar()
		{
			return _reader.ReadChar();
		}

		public string ReadLine()
		{
			short state = 0;
			int tagIndex = 0;
			bool blockComplete = false;
			bool eof = false;
			int tryCounter = 5;
			ResetBuffer();
			while (!eof && !blockComplete)
			{
				int readInt = ReadChar();
				if (readInt == -1)
				{
					// if eof before the block is complete, wait some msecs for the logger to flush the complete xml struct
					if (state != 0)
					{
						if (--tryCounter > 0)
						{
							Thread.Sleep(100);
							continue;
						}
						else
						{
							eof = true;
							break;
						}
					}
					else
					{
						eof = true;
						break;
					}
				}

				char readChar = (char)readInt;
				// state:
				// 0 = looking for tag start
				// 1 = reading into buffer as long as the read data matches the start tag
				// 2 = reading into buffer while waiting for the begin of the end tag
				// 3 = reading into buffer as long as data matches the end tag. stopping when tag complete
				switch (state)
				{
					case 0:
						if (readChar == StartTag[0])
						{
							//Logger.logInfo("state = 1");
							state = 1;
							tagIndex = 1;
							AddToBuffer(readChar);
						}
						//else
						//{
						//  Logger.logInfo("char: " + readChar);
						//}
						break;
					case 1:
						if (readChar == StartTag[tagIndex])
						{
							AddToBuffer(readChar);
							if (++tagIndex >= StartTag.Length)   
							{
								//Logger.logInfo("state = 2");
								state = 2;  // start Tag complete
								tagIndex = 0;
							}
						}
						else
						{
							// tag doesn't match anymore
							//Logger.logInfo("state = 0 [" + buffer.ToString() + readChar + "]");
							state = 0;
							ResetBuffer();
						}
						break;
					case 2:
						AddToBuffer(readChar);
						if (readChar == EndTag[0])
						{
							//Logger.logInfo("state = 3");
							state = 3;
							tagIndex = 1;
						}
						break;
					case 3:
						AddToBuffer(readChar);
						if (readChar == EndTag[tagIndex])
						{
							tagIndex++;
							if (tagIndex >= EndTag.Length)
							{
								blockComplete = true;
								break;
							}
						}
						else
						{
							//Logger.logInfo("state = 2");
							state = 2;
						}
						break;
				}
			}
			if (!blockComplete)
				return null;    // EOF
			//string result = new string(buffer, 0, bufferPos);
			string result = _buffer.ToString();
			return result;
		}

		private void AddToBuffer(char readChar)
		{
			//buffer[bufferPos++] = readChar;
			_buffer.Append(readChar);
		}

		private void ResetBuffer()
		{
			//bufferPos = 0;
			_buffer = new StringBuilder();
		}
	}
}