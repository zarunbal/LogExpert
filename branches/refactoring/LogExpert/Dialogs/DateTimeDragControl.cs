using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
	/// <summary>
	/// This control displays date and time and allows user to interact with the individual parts using the mouse
	/// to increment and decrement the values. The date format displayed is derived from the application UI locale.
	/// We currently support only three: US (mm/dd/yyyy), French (yyyy-mm-dd) and German (dd.mm.yyyy).
	/// The control raises events (ValueChanged, ValueDragged) when the date/time changes so that owner can react accordingly.
	/// </summary>
	public partial class DateTimeDragControl : UserControl
	{
		#region Data Members and Properties
		
		public enum DragOrientations
		{
			Horizontal,
			Vertical,
			InvertedVertical
		}
		
		private DateTime minDateTime = DateTime.MinValue;
		
		public DateTime MinDateTime
		{
			get
			{
				return minDateTime;
			}
			set
			{
				minDateTime = value;
			}
		}
		
		private DateTime maxDateTime = DateTime.MaxValue;
		
		public DateTime MaxDateTime
		{
			get
			{
				return maxDateTime;
			}
			set
			{
				maxDateTime = value;
			}
		}
		
		public DragOrientations dragOrientation = DragOrientations.Vertical;
		
		public DragOrientations DragOrientation
		{
			get
			{
				return dragOrientation;
			}
			set 
			{
				dragOrientation = value;
				UpdateContextMenu();
			}
		}
		
		public Color HoverColor { get; set; }
		
		private enum Components
		{
			Year,
			Month,
			Day,
			Hours,
			Minutes,
			Seconds
		};
		
		private string dateSeparator = ".";
		private Rectangle[] digitRects = new Rectangle[6];
		private Components[] rectContents = new Components[6]; // for now, only the first three components may change position
		private int yearIndex = 2;
		private int monthIndex = 1;
		private int dayIndex = 0;
		private StringFormat digitsFormat = new StringFormat();
		private ToolStripItem item1 = new ToolStripMenuItem();
		private ToolStripItem item2 = new ToolStripMenuItem();
		private ToolStripItem item3 = new ToolStripMenuItem();
		
		private int startMouseY = 0;
		private int startMouseX = 0;
		private int oldValue = 0;
		private int addedValue = 0;
		private const int NO_DIGIT_DRAGGED = -1;
		private int draggedDigit = NO_DIGIT_DRAGGED;
		
		private DateTime dateTime = new DateTime();
		
		public DateTime DateTime
		{
			get
			{
				return this.dateTime.Subtract(TimeSpan.FromMilliseconds(this.dateTime.Millisecond));
			}
			set
			{
				this.dateTime = value;
				if (this.dateTime < this.minDateTime)
					this.dateTime = minDateTime;
				if (this.dateTime > this.maxDateTime)
					this.dateTime = maxDateTime;
			}
		}
		
		#endregion
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public DateTimeDragControl()
		{
			InitializeComponent();
			
			this.digitsFormat.LineAlignment = StringAlignment.Center;
			this.digitsFormat.Alignment = StringAlignment.Near;
			this.digitsFormat.Trimming = StringTrimming.None;
			this.digitsFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
			
			this.draggedDigit = NO_DIGIT_DRAGGED;
		}
		
		private void DateTimeDragControl_Load(object sender, EventArgs e)
		{
			InitDigiRects();
			
			BuildContextualMenu();
		}
		
		#region Contextual Menu
		
		private void BuildContextualMenu()
		{
			this.ContextMenuStrip = new ContextMenuStrip();
			this.ContextMenuStrip.Name = "Timestamp selector";
			this.ContextMenuStrip.Items.Add(item1);
			this.ContextMenuStrip.Items.Add(item2);
			this.ContextMenuStrip.Items.Add(item3);
			item1.Click += new EventHandler(item1_Click);
			item1.Text = "Drag horizontal";
			item2.Click += new EventHandler(item2_Click);
			item2.Text = "Drag vertical";
			item3.Click += new EventHandler(item3_Click);
			item3.Text = "Drag vertical inverted";

			ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
		
			UpdateContextMenu();
		}
		
		private void UpdateContextMenu()
		{
			item1.Enabled = this.DragOrientation != DragOrientations.Horizontal;
			item2.Enabled = this.DragOrientation != DragOrientations.Vertical;
			item3.Enabled = this.DragOrientation != DragOrientations.InvertedVertical;
		}
		
		private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			if (this.Capture)
			{
				e.Cancel = true;
			}
		}
		
		private void item1_Click(object sender, EventArgs e)
		{
			this.DragOrientation = DragOrientations.Horizontal;
			item1.Enabled = false;
			item2.Enabled = true;
			item3.Enabled = true;
		}
		
		private void item2_Click(object sender, EventArgs e)
		{
			this.DragOrientation = DragOrientations.Vertical;
			item1.Enabled = true;
			item2.Enabled = false;
			item3.Enabled = true;
		}
		
		private void item3_Click(object sender, EventArgs e)
		{
			this.DragOrientation = DragOrientations.InvertedVertical;
			item1.Enabled = true;
			item2.Enabled = true;
			item3.Enabled = false;
		}

		#endregion
		
		#region Rendering
			
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			
			// Show what digit is dragged
			using (Brush hoverBrush = new SolidBrush(this.HoverColor))
			{
				if (this.draggedDigit != NO_DIGIT_DRAGGED)
					e.Graphics.FillRectangle(hoverBrush, this.digitRects[this.draggedDigit]);
			}
			
			// Display current value with user-defined date format and fixed time format ("HH:mm:ss")
			using (Brush brush = new SolidBrush(Color.Black))
			{
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[dayIndex], FormatDigitLeadingZeros(this.dateTime.Day, 2, dayIndex < 2 ? this.dateSeparator : ""));
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[monthIndex], FormatDigitLeadingZeros(this.dateTime.Month, 2, monthIndex < 2 ? this.dateSeparator : ""));
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[yearIndex], FormatDigitLeadingZeros(this.dateTime.Year, 4, yearIndex < 2 ? this.dateSeparator : ""));
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[3], FormatDigitLeadingZeros(this.dateTime.Hour, 2, ":"));
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[4], FormatDigitLeadingZeros(this.dateTime.Minute, 2, ":"));
				DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[5], FormatDigitLeadingZeros(this.dateTime.Second, 2, ""));
			}
		}
			
		private static string FormatDigitLeadingZeros(int number, int length, string postSeparator)
		{
			return number.ToString("D" + length.ToString()) + postSeparator;
		}
			
		private void DrawDigit(Graphics g, Brush brush, Rectangle clip, Rectangle r, string value)
		{
			g.DrawString(value, this.Font, brush, r, this.digitsFormat);
			//using (Pen pen = new Pen(brush))
			//  g.DrawRectangle(pen, r);
		}
			
		private void DateTimeDragControl_Resize(object sender, EventArgs e)
		{
			InitDigiRects();
		}
		
		#endregion
		
		#region Mouse callbacks

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
				
			if (e.Button == MouseButtons.Left)
			{
				this.draggedDigit = DetermineDraggedDigit(e);
				if (this.draggedDigit == NO_DIGIT_DRAGGED)
					return;
				Capture = true;
				this.startMouseY = e.Y;
				this.startMouseX = e.X;
				this.oldValue = GetDraggedValue();
				this.addedValue = 0;
			}
			else if (e.Button == MouseButtons.Right && this.Capture)
			{
				Capture = false;
				SetDraggedValue(0); //undo
			}
			this.Invalidate(); // repaint with the selected item (or none)
		}
				
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (!Capture)
				return;
			
			base.OnMouseUp(e);

			Capture = false;
			this.draggedDigit = NO_DIGIT_DRAGGED;
			this.Invalidate(); // repaint without the selected item
		
			OnValueChanged(new EventArgs());
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			
			if (!Capture)
				return;
			
			int diff;
			if (this.DragOrientation == DragOrientations.Vertical)
				diff = this.startMouseY - e.Y;
			else if (this.DragOrientation == DragOrientations.InvertedVertical)
				diff = this.startMouseY + e.Y;
			else
				diff = e.X - this.startMouseX;
			
			int delta = diff / 5 - this.addedValue; // one unit per 5 pixels move
					
			if (delta != 0) 
			{
				if (SetDraggedValue(delta))
					this.addedValue += delta;
				this.Invalidate();

				OnValueDragged(new EventArgs());
			}
		}
			
		private void DateTimeDragControl_MouseLeave(object sender, EventArgs e)
		{
			if (!this.Capture)
			{
				this.draggedDigit = NO_DIGIT_DRAGGED;
				Refresh();
			}
		}
		
		#endregion
		
		#region Private Methods
				
		// Returns the index of the rectangle (digitRects) under the mouse cursor
		private int DetermineDraggedDigit(MouseEventArgs e)
		{
			for (int i = 0; i < this.digitRects.Length; ++i)
				if (this.digitRects[i].Contains(e.Location))
					return i;
		
			return NO_DIGIT_DRAGGED;
		}
			
		// Return the value corresponding to current dragged digit
		private int GetDraggedValue()
		{
			switch (this.rectContents[this.draggedDigit])
			{
				case Components.Day:
					return this.dateTime.Day;
				case Components.Month:
					return this.dateTime.Month;
				case Components.Year:
					return this.dateTime.Year;
				case Components.Hours:
					return this.dateTime.Hour;
				case Components.Minutes:
					return this.dateTime.Minute;
				case Components.Seconds:
					return this.dateTime.Second;
				default:
					return NO_DIGIT_DRAGGED;
			}
		}
			
		private bool SetDraggedValue(int delta)
		{
			bool changed = true;
			try
			{
				switch (this.rectContents[this.draggedDigit])
				{
					case Components.Day:
						this.dateTime = this.dateTime.AddDays(delta);
						break;
					case Components.Month:
						this.dateTime = this.dateTime.AddMonths(delta);
						break;
					case Components.Year:
						this.dateTime = this.dateTime.AddYears(delta);
						break;
					case Components.Hours:
						this.dateTime = this.dateTime.AddHours(delta);
						break;
					case Components.Minutes:
						this.dateTime = this.dateTime.AddMinutes(delta);
						break;
					case Components.Seconds:
						this.dateTime = this.dateTime.AddSeconds(delta);
						break;
				}
			}
			catch (Exception)
			{ // invalid value dragged
			}
			
			if (this.dateTime > this.MaxDateTime)
			{
				this.dateTime = this.MaxDateTime;
				changed = false;
			}
			if (this.dateTime < this.MinDateTime)
			{
				this.dateTime = this.MinDateTime;
				changed = false;
			}
		
			return changed;
		}
					
		private int IndexOf(Components component)
		{
			for (int i = 0; i < this.rectContents.Length; i++)
				if (this.rectContents[i] == component)
					return i;
		
			return -1;
		}

		private void InitGermanRects()
		{
			this.dateSeparator = ".";
			this.rectContents = new Components[] { Components.Day, Components.Month, Components.Year, Components.Hours, Components.Minutes, Components.Seconds };
			
			Rectangle rect = this.ClientRectangle;
			int oneCharWidth = (rect.Width / 19);
			int step = (rect.Width - oneCharWidth) / 7; // separate the 19 characters into seven pieces: dd., MM., yyyy, " ", HH:, mm:, ss
			int left = rect.Left;
			for (int i = 0; i < this.digitRects.Length; ++i)
			{
				int s = step;
				if (i == 2) // the year is 4 chars instead of 3
					s = step + oneCharWidth;
				else if (i == 5) // seconds are 2 chars instead of 3
					s = step - oneCharWidth + 2;
				
				if (i == 3)
					left += step; // skip space

				this.digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
				left += s;
			}
		}

		private void InitFrenchRects()
		{
			this.dateSeparator = "-";
			this.rectContents = new Components[] { Components.Year, Components.Month, Components.Day, Components.Hours, Components.Minutes, Components.Seconds };
			
			Rectangle rect = this.ClientRectangle;
			int oneCharWidth = (rect.Width / 19);
			int step = (rect.Width - oneCharWidth) / 7; // separate the 19 characters into seven pieces: yyyy-, MM-, dd, " ", HH:, mm:, ss
			int left = rect.Left;
			for (int i = 0; i < this.digitRects.Length; ++i)
			{
				int s = step;
				if (i == 0) // the year is 5 chars instead of 3
					s = step + (2 * oneCharWidth);
				else if (i == 2 || i == 5) // day and seconds are 2 chars instead of 3
					s = step - oneCharWidth + 2;
				
				if (i == 3)
					left += step; // skip space

				this.digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
				left += s;
			}
		}

		private void InitAmericanRects()
		{
			this.dateSeparator = "/";
			this.rectContents = new Components[] { Components.Month, Components.Day, Components.Year, Components.Hours, Components.Minutes, Components.Seconds };
			
			Rectangle rect = this.ClientRectangle;
			int oneCharWidth = (rect.Width / 19);
			int step = (rect.Width - oneCharWidth) / 7; // separate the 19 characters into seven pieces: MM/, dd/, yyyy, " ", HH:, mm:, ss
			int left = rect.Left;
			for (int i = 0; i < this.digitRects.Length; ++i)
			{
				int s = step;
				if (i == 2) // the year is 4 chars instead of 3
					s = step + oneCharWidth;
				else if (i == 5) // seconds are 2 chars instead of 3
					s = step - oneCharWidth + 2;
				
				if (i == 3)
					left += step; // skip space

				this.digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
				left += s;
			}
		}

		private void InitDigiRects()
		{
			var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			var cultureName = (culture.Parent != null) ? culture.Parent.Name : "";
			
			if (cultureName == "fr")
				InitFrenchRects();
			else if (cultureName == "en")
				InitAmericanRects();
			else
				InitGermanRects(); // default

			this.yearIndex = IndexOf(Components.Year);
			this.monthIndex = IndexOf(Components.Month);
			this.dayIndex = IndexOf(Components.Day);
		}

		#endregion

		#region Public Events
		
		public delegate void ValueChangedEventHandler(object sender, EventArgs e);
				
		public event ValueChangedEventHandler ValueChanged;

		protected void OnValueChanged(EventArgs e)
		{
			if (ValueChanged != null)
				ValueChanged(this, e);
		}
		
		public delegate void ValueDraggedEventHandler(object sender, EventArgs e);
				
		public event ValueDraggedEventHandler ValueDragged;
		
		protected void OnValueDragged(EventArgs e)
		{
			if (ValueDragged != null)
				ValueDragged(this, e);
		}

		#endregion
	}
}