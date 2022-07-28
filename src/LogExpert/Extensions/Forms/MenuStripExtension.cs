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
        public override Color ImageMarginGradientBegin => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color ImageMarginGradientMiddle => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color ImageMarginGradientEnd => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color ToolStripDropDownBackground => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color MenuBorder => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemBorder => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemSelected => LogExpert.Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemSelectedGradientBegin => LogExpert.Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemSelectedGradientEnd => LogExpert.Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemPressedGradientBegin => LogExpert.Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemPressedGradientEnd => LogExpert.Config.ColorMode.MenuBackgroundColor;
    }
}
