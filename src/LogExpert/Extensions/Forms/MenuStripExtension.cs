using System.Windows.Forms;

namespace LogExpert.Extensions.Forms
{
    public class ExtendedMenuStripRenderer : ToolStripProfessionalRenderer
    {
        public ExtendedMenuStripRenderer() : base(new MenuSelectedColors()) { }
    }
}
