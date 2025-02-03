using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Extensions.Forms
{
    public class MenuSelectedColors : ProfessionalColorTable
    {
        public override Color ImageMarginGradientBegin => Config.ColorMode.MenuBackgroundColor;

        public override Color ImageMarginGradientMiddle => Config.ColorMode.MenuBackgroundColor;

        public override Color ImageMarginGradientEnd => Config.ColorMode.MenuBackgroundColor;

        public override Color ToolStripDropDownBackground => Config.ColorMode.MenuBackgroundColor;

        public override Color MenuBorder => Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemBorder => Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemSelected => Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemSelectedGradientBegin => Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemSelectedGradientEnd => Config.ColorMode.HoverMenuBackgroundColor;

        public override Color MenuItemPressedGradientBegin => Config.ColorMode.MenuBackgroundColor;

        public override Color MenuItemPressedGradientEnd => Config.ColorMode.MenuBackgroundColor;
    }
}