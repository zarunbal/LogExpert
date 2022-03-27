using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public class LogGridCell : DataGridViewTextBoxCell
    {
        #region Properties

        public override Type EditType => typeof(LogCellEditingControl);

        #endregion
    }
}
