using LogExpert.Classes.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LogExpert
{
    internal class XmlBlockSplitter : LogStreamReaderBase
    {
        #region Fields

        private static readonly string[] splitStrings = new string[] { "\r\n", "\n", "\r" };
        private static readonly char[] newLineChar = new char[] { '\n' };

        private readonly XmlLogReader reader;

        private readonly XmlParserContext context;
        private readonly XmlReaderSettings settings;

        private readonly Queue<string> lineList = new Queue<string>();
        
        private string stylesheet;
        private XslCompiledTransform xslt;

        #endregion

        #region cTor

        public XmlBlockSplitter(XmlLogReader reader, IXmlLogConfiguration xmlLogConfig)
        {
            this.reader = reader;
            this.reader.StartTag = xmlLogConfig.XmlStartTag;
            this.reader.EndTag = xmlLogConfig.XmlEndTag;

            this.Stylesheet = xmlLogConfig.Stylesheet;

            // Create the XmlNamespaceManager.
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            if (xmlLogConfig.Namespace != null)
            {
                nsmgr.AddNamespace(xmlLogConfig.Namespace[0], xmlLogConfig.Namespace[1]);
            }
            // Create the XmlParserContext.
            this.context = new XmlParserContext(nt, nsmgr, null, XmlSpace.None);
            this.settings = new XmlReaderSettings();
            this.settings.ConformanceLevel = ConformanceLevel.Fragment;
        }

        #endregion

        #region Properties

        public override long Position
        {
            get => this.reader.Position;
            set => this.reader.Position = value;
        }

        public override Encoding Encoding => this.reader.Encoding;

        public override bool IsBufferComplete => this.lineList.Count == 0;

        public string Stylesheet
        {
            get { return this.stylesheet; }
            set
            {
                this.stylesheet = value;
                if (this.stylesheet != null)
                {
                    XmlReader stylesheetReader = XmlReader.Create(new StringReader(this.stylesheet));

                    this.xslt = new XslCompiledTransform();
                    this.xslt.Load(stylesheetReader);
                }
                else
                {
                    this.xslt = null;
                }
            }
        }

        #endregion

        #region Private Methods

        private void ParseXmlBlock(string block)
        {
            if (this.stylesheet != null)
            {
                XmlReader xmlReader = XmlReader.Create(new StringReader(block), this.settings, this.context);

                xmlReader.Read();
                xmlReader.MoveToContent();
                //xmlReader.MoveToContent();
                StringWriter textWriter = new StringWriter();

                this.xslt.Transform(xmlReader, null, textWriter);
                string message = textWriter.ToString();
                this.SplitToLinesList(message);
            }
            else
            {
                this.SplitToLinesList(block);
                //this.lineList.Add(block);   // TODO: make configurable, if block has to be splitted
            }
        }

        private void SplitToLinesList(string message)
        {
            const int MAX_LEN = 3000;
            string[] lines = message.Split(XmlBlockSplitter.splitStrings, StringSplitOptions.None);
            foreach (string theLine in lines)
            {
                string line = theLine.Trim(newLineChar);
                while (line.Length > MAX_LEN)
                {
                    string part = line.Substring(0, MAX_LEN);
                    line = line.Substring(MAX_LEN);
                    this.lineList.Enqueue(part);
                }
                this.lineList.Enqueue(line);
            }
        }

        #endregion

        #region Public Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.reader.Dispose();
            }
        }

        public override int ReadChar()
        {
            return this.reader.ReadChar();
        }

        public override string ReadLine()
        {
            if (this.lineList.Count == 0)
            {
                string block = this.reader.ReadLine();
                if (block == null)
                {
                    return null;
                }

                try
                {
                    this.ParseXmlBlock(block);
                }
                catch (XmlException)
                {
                    this.lineList.Enqueue("[XML Parser error] " + block);
                }
            }
            return this.lineList.Dequeue();
        }

        #endregion
    }
}