using System.Text;

namespace LogExpert
{
    public interface ILogStreamReader
    {
        #region Properties / Indexers

        Encoding Encoding { get; }

        bool IsBufferComplete { get; }

        long Position { get; set; }

        #endregion

        #region Public Methods

        int ReadChar();
        string ReadLine();

        #endregion
    }
}
