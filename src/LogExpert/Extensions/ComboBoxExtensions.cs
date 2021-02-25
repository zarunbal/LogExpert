using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public static class ComboBoxExtensions
    {
        /// <see cref="https://stackoverflow.com/a/4842576/1987788"/>
        public static int GetMaxTextWidth(this ComboBox comboBox)
        {
            var maxTextWidth = comboBox.Width;

            foreach (var item in comboBox.Items)
            {
                var textWidthInPixels = TextRenderer.MeasureText(item.ToString(), comboBox.Font).Width;

                if (textWidthInPixels > maxTextWidth)
                {
                    maxTextWidth = textWidthInPixels;
                }
            }

            return maxTextWidth;
        }
    }
}
