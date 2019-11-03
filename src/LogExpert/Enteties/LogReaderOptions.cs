using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogExpert.Interface;

namespace LogExpert.Enteties
{
    public class LogReaderOptions : ILogReaderOptions
    {
        public bool UseSystemReader { get; set; }
        public bool IsXmlReader { get; set; }
        public IEncodingOptions EncodingOptions { get; set; }
        public IXmlLogConfiguration XmlLogConfiguration { get; set; }
        public int MaxBuffers { get; set; }
        public int MaxLinerPerBuffer { get; set; }
        public IMultifileOptions MultiFileOptions { get; set; }
        public bool IsMultiFile { get; set; }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
    }
}
