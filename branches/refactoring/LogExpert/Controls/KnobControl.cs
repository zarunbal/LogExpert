using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
	public partial class KnobControl : UserControl
	{
		int startMouseY = 0;

		int oldValue = 0;

		public int MinValue { get; set; }

		public int MaxValue { get; set; }

		int value;

		public int Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
				Refresh();
			}
		}

		private int dragSensitivity = 3;

		StringFormat stringFormat = new StringFormat();
		bool isShiftPressed = false;

		public KnobControl()
		{
			InitializeComponent();
			stringFormat.LineAlignment = StringAlignment.Far;
			stringFormat.Alignment = StringAlignment.Center;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Color foregroundColor = this.Enabled ? Color.Black : Color.Gray;

			Pen blackPen = new Pen(foregroundColor, 1);
			Pen greyPen = new Pen(Color.Gray, 1);

			Rectangle rect = this.ClientRectangle;
			int height = this.Font.Height + 3;
			if (height > rect.Height)
				height = rect.Height + 3;
			rect.Inflate(-1, -height / 2);
			rect.Offset(0, -height / 2);
			e.Graphics.DrawEllipse(greyPen, rect);

			//rect = this.ClientRectangle;
			rect.Inflate(-2, -2);

			float startAngle = 135.0F + 270F * ((float)this.value / (float)this.Range);
			float sweepAngle = 0.1F;
			e.Graphics.DrawPie(blackPen, rect, startAngle, sweepAngle);

			Brush brush = new SolidBrush(foregroundColor);
			RectangleF rectF = new RectangleF(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
			e.Graphics.DrawString("" + this.value, this.Font, brush, rectF, this.stringFormat);

			blackPen.Dispose();
			greyPen.Dispose();
			brush.Dispose();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				Capture = true;
				startMouseY = e.Y;
				this.oldValue = this.Value;
			}
			if (e.Button == MouseButtons.Right)
			{
				Capture = false;
				this.Value = this.oldValue;
				this.Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			Capture = false;
			this.oldValue = this.Value;
			OnValueChanged(new EventArgs());
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!Capture)
				return;

			int sense = this.isShiftPressed ? this.DragSensitivity * 2 : this.DragSensitivity;

			int diff = this.startMouseY - e.Y;
			Logger.logDebug("KnobDiff: " + diff);
			int range = this.MaxValue - this.MinValue;
			this.value = this.oldValue + diff / sense;
			if (this.value < this.MinValue)
				this.value = this.MinValue;
			if (this.value > this.MaxValue)
				this.value = this.MaxValue;
			this.Invalidate();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			this.isShiftPressed = e.Shift;
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			this.isShiftPressed = e.Shift;
			base.OnKeyUp(e);
		}

		public int Range
		{
			get
			{
				return this.MaxValue - this.MinValue;
			}
		}

		public int DragSensitivity
		{
			get
			{
				return this.dragSensitivity;
			}
			set
			{
				this.dragSensitivity = value;
			}
		}

		public delegate void ValueChangedEventHandler(object sender, EventArgs e);

		public event ValueChangedEventHandler ValueChanged;

		protected void OnValueChanged(EventArgs e)
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, e);
			}
		}
	}
}