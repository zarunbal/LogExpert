using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;

namespace LogExpert
{
  class XmlBlockSplitter : ILogStreamReader
  {
    private string stylesheet;
    XslCompiledTransform xslt = new XslCompiledTransform();
    private XmlLogReader reader;
    XmlParserContext context;
    XmlReaderSettings settings;
    private IList<string> lineList = new List<string>();
    private char[] newLineChar = new char[] {'\n'};
    private string[] splitStrings = new string[] { "\r\n", "\n", "\r" };


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



    #region ILogStreamReader Member

    public int ReadChar()
    {
      return this.reader.ReadChar();
    }

    public string ReadLine()
    {
      if (this.lineList.Count == 0)
      {
        string block = this.reader.ReadLine();
        if (block == null)
          return null;
        try
        {
          ParseXmlBlock(block);
        }
        catch (XmlException)
        {
          this.lineList.Add("[XML Parser error] " + block);
        }
      }
      string line = this.lineList[0];
      this.lineList.RemoveAt(0);
      return line;
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
      get { return this.reader.Encoding; }
    }

    public bool IsBufferComplete
    {
      get { return this.lineList.Count == 0; }
    }

    public string Stylesheet
    {
      get { return this.stylesheet; }
      set
      {
        this.stylesheet = value;
        if (this.stylesheet != null)
        {
          XmlReader stylesheetReader = XmlReader.Create(new StringReader(Stylesheet));
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

    private void ParseXmlBlock(string block)
    {
      if (this.stylesheet != null)
      {
        XmlReader xmlReader = XmlReader.Create(new StringReader(block), settings, context);

        xmlReader.Read();
        xmlReader.MoveToContent();
        //xmlReader.MoveToContent();
        StringWriter textWriter = new StringWriter();

        this.xslt.Transform(xmlReader, null, textWriter);
        string message = textWriter.ToString();
        SplitToLinesList(message);
      }
      else
      {
        SplitToLinesList(block);
        //this.lineList.Add(block);   // TODO: make configurable, if block has to be splitted
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
          this.lineList.Add(part);
        }
        this.lineList.Add(line);
      }
    }
  }
}
