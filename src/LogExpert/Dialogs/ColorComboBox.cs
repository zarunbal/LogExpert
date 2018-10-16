using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// using System.Linq;
namespace LogExpert.Dialogs
{
    public class ColorComboBox : ComboBox
    {
        #region Private Fields

        private Color customColor = Color.FromKnownColor(KnownColor.Black);

        #endregion

        #region Ctor

        public ColorComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += ColorComboBox_DrawItem;

// add color presets
            if (!DesignMode)
            {
                Items.Add(customColor);
                Items.Add(Color.Black);
                Items.Add(Color.White);
                Items.Add(Color.Gray);
                Items.Add(Color.DarkGray);
                Items.Add(Color.Blue);
                Items.Add(Color.LightBlue);
                Items.Add(Color.DarkBlue);
                Items.Add(Color.Green);
                Items.Add(Color.LightGreen);
                Items.Add(Color.DarkGreen);
                Items.Add(Color.Olive);
                Items.Add(Color.Red);
                Items.Add(Color.Pink);
                Items.Add(Color.Purple);
                Items.Add(Color.IndianRed);
                Items.Add(Color.DarkCyan);
                Items.Add(Color.Yellow);
            }
        }

        #endregion

        #region Properties / Indexers

        public Color CustomColor
        {
            get => customColor;
            set
            {
                customColor = value;
                Items.RemoveAt(0);
                Items.Insert(0, customColor);
            }
        }

        public Color SelectedColor => (Color)(SelectedIndex != -1 ? Items[SelectedIndex] : null);

        #endregion

        #region Private Methods

        private void ColorComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                Rectangle rectangle = new Rectangle(4, e.Bounds.Top + 2, 30, e.Bounds.Height - 4);
                Color rectColor = (Color)Items[e.Index];
                e.Graphics.FillRectangle(new SolidBrush(rectColor), rectangle);
                e.Graphics.DrawRectangle(Pens.Black, rectangle);
                if (e.Index == 0)
                {
                    e.Graphics.DrawString("Custom", e.Font, Brushes.Black,
                        new PointF(42, e.Bounds.Top + 2));
                }
                else
                {
                    e.Graphics.DrawString(((Color)Items[e.Index]).Name, e.Font, Brushes.Black,
                        new PointF(42, e.Bounds.Top + 2));
                }

                if (!Enabled)
                {
                    HatchBrush brush = new HatchBrush(HatchStyle.Percent50, Color.LightGray,
                        Color.FromArgb(10, Color.LightGray));
                    rectangle.Inflate(1, 1);
                    e.Graphics.FillRectangle(brush, rectangle);
                    brush.Dispose();
                }

                e.DrawFocusRectangle();
            }
        }

        #endregion
    }
}
