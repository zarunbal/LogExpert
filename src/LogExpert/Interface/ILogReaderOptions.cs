using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogExpert.Interface
{
    public interface ILogReaderOptions
    {
        bool UseSystemReader { get; set; }

        bool IsXmlReader { get; set; }

        IEncodingOptions EncodingOptions { get; set; }

        IXmlLogConfiguration XmlLogConfiguration { get; set; }

        int MaxBuffers { get; set; }

        int MaxLinerPerBuffer { get; set; }

        IMultifileOptions MultiFileOptions { get; set; }

        bool IsMultiFile { get; set; }

        string FileName { get; set; }

        string[] FileNames { get; set; }
    }
}
