using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogExpert;
using NUnit.Framework;

namespace Logexpert.UnitTests
{
	public class PositionAwareStreamReaderTest
	{
		private static readonly int _lenght =  int.MaxValue / 8;
		private static readonly int _batch = 1000;

		private Stream _stream = null;

		public PositionAwareStreamReaderTest()
		{
			try
			{
				_stream = new MemoryStream();

				int last = 0;
				for (int i = 0; i <= _lenght; i = i + _batch)
				{
					StringBuilder builder = new StringBuilder();
					for (int b = last; b < i; b++)
					{
						builder.Append(string.Format("{0}\r\n", b));
					}
					last = i;

					byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());

					_stream.Write(buffer, 0, buffer.Length);
				}
				_stream.Flush();
				_stream.Position = 0;
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			_stream.Dispose();
		}

		[SetUp]
		public void ResetStreamPosition()
		{
			_stream.Position = 0;
		}

		[Test]
		public void Old_Reader_600000()
		{
			TestPositionAwareReader(false);
		}

		[Test]
		public void New_Reader_600000()
		{
			TestPositionAwareReader(false);
		}

		private void TestPositionAwareReader(bool useNewReader)
		{
				//FileInfo info = new FileInfo(@"e:\temp\txt.txt");

				//if (info.Exists)
				//{
				//	info.Delete();
				//}

				//stream.Position = 0;

				//using (FileStream file = info.OpenWrite())
				//{
				//	byte[] buffer = new byte[1024];

				//	for (long i = 0; i < stream.Length; i++)
				//	{
				//		int read = stream.Read(buffer, 0, buffer.Length);
				//		if (read !=0)
				//		{
				//			file.Write(buffer, 0, read);
				//		}
						
				//	}
				//}

				EncodingOptions encopts = new EncodingOptions
				{
					DefaultEncoding = Encoding.UTF8,
					Encoding = Encoding.UTF8
				};

				PositionAwareStreamReader reader = new PositionAwareStreamReader(_stream, encopts, useNewReader);

				for (int i = 0; i < _lenght; i++)
				{
					if (i == 599500)
					{
						if (Debugger.IsAttached)
						{
							Debugger.Break();
						}
					}
					string line = reader.ReadLine();

					Assert.AreEqual(i.ToString(), line);
				}

				reader.Close();
		}
	}

	//public class Test : Stream
	//{
		 

	//}
}