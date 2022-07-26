using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogExpert.Config
{
    public class ExtendedToolStripSeparator : ToolStripSeparator
    {
        public ExtendedToolStripSeparator()
        {
            this.Paint += ExtendedToolStripSeparator_Paint;
        }

        private void ExtendedToolStripSeparator_Paint(object sender, PaintEventArgs e)
        {
            // Get the separator's width and height.
            ToolStripSeparator toolStripSeparator = (ToolStripSeparator)sender;
            int width = toolStripSeparator.Width;
            int height = toolStripSeparator.Height;

            // Choose the colors for drawing.
            // I've used Color.White as the foreColor.
            Color foreColor = Color.FromName(Config.ColorMode.ForeColor.Name);
            // Color.Teal as the backColor.
            Color backColor = Color.FromName(Config.ColorMode.BackgroundColor.Name);

            // Fill the background.
            e.Graphics.FillRectangle(new SolidBrush(backColor), 0, 0, width, height);

            // Draw the line.
            e.Graphics.DrawLine(new Pen(foreColor), 4, height / 2, width - 4, height / 2);
        }
    }

    
    public class ExtendedMenuItemRenderer : ToolStripProfessionalRenderer
    {
        public ExtendedMenuItemRenderer() : base(new MenuSelectedColors()) { }
    }

    public class MenuSelectedColors : ProfessionalColorTable
    {
        public override Color MenuItemPressedGradientBegin
        {
            get { return LogExpert.Config.ColorMode.MenuBackgroundColor; }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get { return LogExpert.Config.ColorMode.MenuBackgroundColor; }
        }
    }
  
}
