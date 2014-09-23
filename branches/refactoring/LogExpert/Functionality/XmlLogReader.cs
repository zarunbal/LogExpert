using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	class XmlLogReader : ILogStreamReader
	{
		private PositionAwareStreamReader reader;
		private string startTag = "<log4j:event";
		private string endTag = "</log4j:event>";
		//private const int MAX_BUFFER_LEN = 4096;
		//private char[] buffer = new char[MAX_BUFFER_LEN];
		//private int bufferPos = 0;
		private StringBuilder buffer = new StringBuilder();

		public XmlLogReader(PositionAwareStreamReader reader)
		{
			this.reader = reader;
		}

		public long Position
		{
			get
			{
				return this.reader.Position;
			}
			set
			{
				this.reader.Position = value;
			}
		}

		public Encoding Encoding
		{
			get
			{
				return this.reader.Encoding;
			}
		}

		public bool IsBufferComplete
		{
			get
			{
				return this.reader.IsBufferComplete;
			}
		}

		public string StartTag
		{
			get
			{
				return this.startTag;
			}
			set
			{
				this.startTag = value;
			}
		}

		public string EndTag
		{
			get
			{
				return this.endTag;
			}
			set
			{
				this.endTag = value;
			}
		}

		public int ReadChar()
		{
			return this.reader.ReadChar();
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
						if (readChar == this.StartTag[0])
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
						if (readChar == this.StartTag[tagIndex])
						{
							AddToBuffer(readChar);
							if (++tagIndex >= this.StartTag.Length)   
							{
								//Logger.logInfo("state = 2");
								state = 2;  // start Tag complete
								tagIndex = 0;
							}
						}
						else
						{
							// tag doesn't match anymore
							//Logger.logInfo("state = 0 [" + this.buffer.ToString() + readChar + "]");
							state = 0;
							ResetBuffer();
						}
						break;
					case 2:
						AddToBuffer(readChar);
						if (readChar == this.EndTag[0])
						{
							//Logger.logInfo("state = 3");
							state = 3;
							tagIndex = 1;
						}
						break;
					case 3:
						AddToBuffer(readChar);
						if (readChar == this.EndTag[tagIndex])
						{
							tagIndex++;
							if (tagIndex >= this.EndTag.Length)
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
			//string result = new string(this.buffer, 0, this.bufferPos);
			string result = this.buffer.ToString();
			return result;
		}

		private void AddToBuffer(char readChar)
		{
			//this.buffer[this.bufferPos++] = readChar;
			buffer.Append(readChar);
		}

		private void ResetBuffer()
		{
			//this.bufferPos = 0;
			buffer = new StringBuilder();
		}
	}
}