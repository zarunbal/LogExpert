using System.Windows.Forms;

namespace LogExpert.Extensions.Forms
{
    public class ToolStripRendererExtension : ToolStripSystemRenderer
    {
        public ToolStripRendererExtension() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // Commented on purpose to avoid drawing the border that appears visible in Dark Mode (not visible in Bright mode)
            //base.OnRenderToolStripBorder(e);
        }
    }
}
