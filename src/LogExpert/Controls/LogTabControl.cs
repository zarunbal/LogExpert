using System.Drawing;
using System.Windows.Forms;

namespace LogExpert
{
    internal class LogTabControl : TabControl
    {
        #region Private Fields

        private BufferedGraphics myBuffer;

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            BufferedGraphicsContext currentContext;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(CreateGraphics(), DisplayRectangle);

            PaintEventArgs args = new PaintEventArgs(myBuffer.Graphics, e.ClipRectangle);

            base.OnPaint(args);

            myBuffer.Render(e.Graphics);
            myBuffer.Dispose();
        }

        #endregion
    }
}
