using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;

namespace LogExpert
{
	internal class XmlBlockSplitter : ILogStreamReader
	{
		private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

		private string _stylesheet;
		private XslCompiledTransform _xslt = new XslCompiledTransform();
		private XmlLogReader _reader;
		private XmlParserContext _context;
		private XmlReaderSettings _settings;
		private IList<string> _lineList = new List<string>();
		private char[] _newLineChar = new char[] { '\n' };
		private string[] _splitStrings = new string[] { "\r\n", "\n", "\r" };

		public XmlBlockSplitter(XmlLogReader reader, IXmlLogConfiguration xmlLogConfig)
		{
			_reader = reader;
			_reader.StartTag = xmlLogConfig.XmlStartTag;
			_reader.EndTag = xmlLogConfig.XmlEndTag;
			Stylesheet = xmlLogConfig.Stylesheet;

			// Create the XmlNamespaceManager.
			NameTable nt = new NameTable();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
			if (xmlLogConfig.Namespace != null)
			{
				nsmgr.AddNamespace(xmlLogConfig.Namespace[0], xmlLogConfig.Namespace[1]);
			}
			// Create the XmlParserContext.
			_context = new XmlParserContext(nt, nsmgr, null, XmlSpace.None);
			_settings = new XmlReaderSettings();
			_settings.ConformanceLevel = ConformanceLevel.Fragment;
		}

		#region ILogStreamReader Member

		public int ReadChar()
		{
			return _reader.ReadChar();
		}

		public string ReadLine()
		{
			if (_lineList.Count == 0)
			{
				string block = _reader.ReadLine();
				if (block == null)
				{
					return null;
				}
				try
				{
					ParseXmlBlock(block);
				}
				catch (XmlException ex)
				{
					_logger.Error(ex);

					_lineList.Add("[XML Parser error] " + block);
				}
			}
			string line = _lineList[0];
			_lineList.RemoveAt(0);
			return line;
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
				return _lineList.Count == 0;
			}
		}

		public string Stylesheet
		{
			get
			{
				return _stylesheet;
			}
			set
			{
				_stylesheet = value;
				if (_stylesheet != null)
				{
					XmlReader stylesheetReader = XmlReader.Create(new StringReader(Stylesheet));
					_xslt = new XslCompiledTransform();
					_xslt.Load(stylesheetReader);
				}
				else
				{
					_xslt = null;
				}
			}
		}

		#endregion ILogStreamReader Member

		private void ParseXmlBlock(string block)
		{
			if (_stylesheet != null)
			{
				XmlReader xmlReader = XmlReader.Create(new StringReader(block), _settings, _context);

				xmlReader.Read();
				xmlReader.MoveToContent();
				//xmlReader.MoveToContent();
				StringWriter textWriter = new StringWriter();

				_xslt.Transform(xmlReader, null, textWriter);
				string message = textWriter.ToString();
				SplitToLinesList(message);
			}
			else
			{
				SplitToLinesList(block);
				//lineList.Add(block);   // TODO: make configurable, if block has to be splitted
			}
		}

		private void SplitToLinesList(string message)
		{
			const int MAX_LEN = 3000;
			string[] lines = message.Split(_splitStrings, StringSplitOptions.None);
			foreach (string theLine in lines)
			{
				string line = theLine.Trim(_newLineChar);
				while (line.Length > MAX_LEN)
				{
					string part = line.Substring(0, MAX_LEN);
					line = line.Substring(MAX_LEN);
					_lineList.Add(part);
				}
				_lineList.Add(line);
			}
		}
	}
}