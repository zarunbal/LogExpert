using System.Text;

namespace LogExpert
{
    public interface IEncodingOptions
    {
        /// <summary>
        /// Sets or gets the Encoding which shall be used when reading a file. A value of null means 'please autodetect' via BOM.
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// The Encoding to be used when autodetect cannot be applied (missing BOM). Only used when Encoding is set to null.
        /// </summary>
        Encoding DefaultEncoding { get; set; }
    }
}