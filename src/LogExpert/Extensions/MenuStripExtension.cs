using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogExpert.Extensions
{
    public class ExtendedMenuStripRenderer : ToolStripProfessionalRenderer
    {
        public ExtendedMenuStripRenderer() : base(new MenuSelectedColors()) { }
    }

    public class MenuSelectedColors : ProfessionalColorTable
    {
        // Not doing anything?
        public override Color MenuBorder 
        {
            get { return LogExpert.Config.ColorMode.MenuBackgroundColor; }
        }

        public override Color MenuItemBorder
        {
            get { return LogExpert.Config.ColorMode.MenuBackgroundColor; }
        }

        public override Color MenuItemSelected
        {
            get { return LogExpert.Config.ColorMode.HoverMenuBackgroundColor; }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return LogExpert.Config.ColorMode.HoverMenuBackgroundColor; }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return LogExpert.Config.ColorMode.HoverMenuBackgroundColor; }
        }

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
