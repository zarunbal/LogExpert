using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogExpert.Extensions
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
