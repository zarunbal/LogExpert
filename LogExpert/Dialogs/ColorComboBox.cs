using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LogExpert.Dialogs
{
	public class ColorComboBox : ComboBox
	{
		System.Drawing.Color customColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Black);

		public ColorComboBox()
		{
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.DrawItem += new DrawItemEventHandler(ColorComboBox_DrawItem);
			// add color presets
			if (!this.DesignMode)
			{
				this.Items.Add(this.customColor);
				this.Items.Add(System.Drawing.Color.Black);
				this.Items.Add(System.Drawing.Color.White);
				this.Items.Add(System.Drawing.Color.Gray);
				this.Items.Add(System.Drawing.Color.DarkGray);
				this.Items.Add(System.Drawing.Color.Blue);
				this.Items.Add(System.Drawing.Color.LightBlue);
				this.Items.Add(System.Drawing.Color.DarkBlue);
				this.Items.Add(System.Drawing.Color.Green);
				this.Items.Add(System.Drawing.Color.LightGreen);
				this.Items.Add(System.Drawing.Color.DarkGreen);
				this.Items.Add(System.Drawing.Color.Olive);
				this.Items.Add(System.Drawing.Color.Red);
				this.Items.Add(System.Drawing.Color.Pink);
				this.Items.Add(System.Drawing.Color.Purple);
				this.Items.Add(System.Drawing.Color.IndianRed);
				this.Items.Add(System.Drawing.Color.DarkCyan);
				this.Items.Add(System.Drawing.Color.Yellow);
			}
		}

		void ColorComboBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index >= 0)
			{
				Rectangle rectangle = new Rectangle(4, e.Bounds.Top + 2, 30, e.Bounds.Height - 4);
				Color rectColor = (Color)Items[e.Index];
				e.Graphics.FillRectangle(new SolidBrush(rectColor), rectangle);
				e.Graphics.DrawRectangle(System.Drawing.Pens.Black, rectangle);
				if (e.Index == 0)
				{
					e.Graphics.DrawString("Custom", e.Font, System.Drawing.Brushes.Black,
						new PointF(42, e.Bounds.Top + 2));
				}
				else
				{
					e.Graphics.DrawString(((Color)Items[e.Index]).Name, e.Font, System.Drawing.Brushes.Black,
						new PointF(42, e.Bounds.Top + 2));
				}
				if (!Enabled)
				{
					HatchBrush brush = new HatchBrush(HatchStyle.Percent50, Color.LightGray, Color.FromArgb(10, Color.LightGray));
					rectangle.Inflate(1, 1);
					e.Graphics.FillRectangle(brush, rectangle);
					brush.Dispose();
				}
				e.DrawFocusRectangle();
			}
		}

		public Color CustomColor
		{
			get
			{
				return this.customColor;
			}
			set
			{
				this.customColor = value;
				this.Items.RemoveAt(0);
				this.Items.Insert(0, this.customColor);
			}
		}

		public Color SelectedColor
		{
			get
			{
				return (Color)(this.SelectedIndex != -1 ? this.Items[this.SelectedIndex] : null);
			}
		}
	}
}