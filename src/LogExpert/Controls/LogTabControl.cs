using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Controls
{
    internal class LogTabControl : TabControl
    {
        #region Fields

        private BufferedGraphics myBuffer;

        #endregion

        #region cTor

        public LogTabControl()
            : base()
        {
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.DoubleBuffer, true);
        }

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            BufferedGraphicsContext currentContext;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);

            PaintEventArgs args = new PaintEventArgs(myBuffer.Graphics, e.ClipRectangle);

            base.OnPaint(args);

            myBuffer.Render(e.Graphics);
            myBuffer.Dispose();
        }

        #endregion
    }
}