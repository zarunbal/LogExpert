using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LogExpert
{
  class LogTabControl : TabControl
  {
    BufferedGraphics myBuffer;

    public LogTabControl()
      : base()
    {
      //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      //SetStyle(ControlStyles.UserPaint, true);
      //SetStyle(ControlStyles.DoubleBuffer, true);
    }


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
  }
}
