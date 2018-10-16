using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LogExpert
{
    internal class XmlBlockSplitter : ILogStreamReader
    {
        #region Private Fields

        private readonly XmlParserContext context;
        private readonly IList<string> lineList = new List<string>();
        private readonly char[] newLineChar = {'\n'};
        private readonly XmlLogReader reader;
        private readonly XmlReaderSettings settings;
        private readonly string[] splitStrings = {"\r\n", "\n", "\r"};
        private string stylesheet;
        private XslCompiledTransform xslt = new XslCompiledTransform();

        #endregion

        #region Ctor

        public XmlBlockSplitter(XmlLogReader reader, IXmlLogConfiguration xmlLogConfig)
        {
            this.reader = reader;
            this.reader.StartTag = xmlLogConfig.XmlStartTag;
            this.reader.EndTag = xmlLogConfig.XmlEndTag;
            Stylesheet = xmlLogConfig.Stylesheet;

            // Create the XmlNamespaceManager.
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            if (xmlLogConfig.Namespace != null)
            {
                nsmgr.AddNamespace(xmlLogConfig.Namespace[0], xmlLogConfig.Namespace[1]);
            }

            // Create the XmlParserContext.
            context = new XmlParserContext(nt, nsmgr, null, XmlSpace.None);
            settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
        }

        #endregion

        #region Interface ILogStreamReader

        public Encoding Encoding => reader.Encoding;

        public bool IsBufferComplete => lineList.Count == 0;

        public long Position
        {
            get => reader.Position;
            set => reader.Position = value;
        }

        public int ReadChar()
        {
            return reader.ReadChar();
        }

        public string ReadLine()
        {
            if (lineList.Count == 0)
            {
                string block = reader.ReadLine();
                if (block == null)
                {
                    return null;
                }

                try
                {
                    ParseXmlBlock(block);
                }
                catch (XmlException)
                {
                    lineList.Add("[XML Parser error] " + block);
                }
            }

            string line = lineList[0];
            lineList.RemoveAt(0);
            return line;
        }

        #endregion

        #region Properties / Indexers

        public string Stylesheet
        {
            get => stylesheet;
            set
            {
                stylesheet = value;
                if (stylesheet != null)
                {
                    XmlReader stylesheetReader = XmlReader.Create(new StringReader(Stylesheet));
                    xslt = new XslCompiledTransform();
                    xslt.Load(stylesheetReader);
                }
                else
                {
                    xslt = null;
                }
            }
        }

        #endregion

        #region Private Methods

        private void ParseXmlBlock(string block)
        {
            if (stylesheet != null)
            {
                XmlReader xmlReader = XmlReader.Create(new StringReader(block), settings, context);

                xmlReader.Read();
                xmlReader.MoveToContent();


// xmlReader.MoveToContent();
                StringWriter textWriter = new StringWriter();

                xslt.Transform(xmlReader, null, textWriter);
                string message = textWriter.ToString();
                SplitToLinesList(message);
            }
            else
            {
                SplitToLinesList(block);


// this.lineList.Add(block);   // TODO: make configurable, if block has to be splitted
            }
        }


        private void SplitToLinesList(string message)
        {
            const int MAX_LEN = 3000;
            string[] lines = message.Split(splitStrings, StringSplitOptions.None);
            foreach (string theLine in lines)
            {
                string line = theLine.Trim(newLineChar);
                while (line.Length > MAX_LEN)
                {
                    string part = line.Substring(0, MAX_LEN);
                    line = line.Substring(MAX_LEN);
                    lineList.Add(part);
                }

                lineList.Add(line);
            }
        }

        #endregion
    }
}
