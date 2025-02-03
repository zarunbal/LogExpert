using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Extensions.Forms
{
    public class MenuToolStripSeparatorExtension : ToolStripSeparator
    {
        public MenuToolStripSeparatorExtension()
        {
            Paint += OnExtendedToolStripSeparatorPaint;
        }

        private void OnExtendedToolStripSeparatorPaint(object sender, PaintEventArgs e)
        {
            // Get the separator's width and height.
            ToolStripSeparator toolStripSeparator = (ToolStripSeparator)sender;
            int width = toolStripSeparator.Width;
            int height = toolStripSeparator.Height;

            // Choose the colors for drawing.
            // I've used Color.White as the foreColor.
            Color foreColor = Config.ColorMode.ForeColor;
            // Color.Teal as the backColor.
            Color backColor = Config.ColorMode.MenuBackgroundColor;

            // Fill the background.
            e.Graphics.FillRectangle(new SolidBrush(backColor), 0, 0, width, height);

            // Draw the line.
            e.Graphics.DrawLine(new Pen(foreColor), 4, height / 2, width - 4, height / 2);
        }
    }  
}
