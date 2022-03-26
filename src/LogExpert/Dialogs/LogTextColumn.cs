using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public class LogTextColumn : DataGridViewColumn
    {
        #region cTor

        public LogTextColumn() : base(new LogGridCell())
        {
        }

        #endregion
    }
}
