using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LogExpert.Classes.DateTimeParser;

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
        #region Fields

        private const int NO_DIGIT_DRAGGED = -1;
        private int addedValue = 0;

        private DateTime dateTime = new DateTime();
        private readonly IList<Rectangle> digitRects = new List<Rectangle>();
        private readonly StringFormat digitsFormat = new StringFormat();
        private int draggedDigit = NO_DIGIT_DRAGGED;

        public DragOrientations dragOrientation = DragOrientations.Vertical;

        private readonly ToolStripItem item1 = new ToolStripMenuItem();
        private readonly ToolStripItem item2 = new ToolStripMenuItem();
        private readonly ToolStripItem item3 = new ToolStripMenuItem();

        private int oldValue = 0;

        private string[] dateParts;

        private int startMouseX = 0;
        private int startMouseY = 0;

        #endregion

        #region cTor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DateTimeDragControl()
        {
            InitializeComponent();

            this.digitsFormat.LineAlignment = StringAlignment.Center;
            this.digitsFormat.Alignment = StringAlignment.Near;
            this.digitsFormat.Trimming = StringTrimming.None;
            this.digitsFormat.FormatFlags =
                StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

            this.draggedDigit = NO_DIGIT_DRAGGED;
        }

        #endregion

        #region Delegates

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        public delegate void ValueDraggedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event ValueChangedEventHandler ValueChanged;
        public event ValueDraggedEventHandler ValueDragged;

        #endregion

        #region Properties

        public DateTime MinDateTime { get; set; } = DateTime.MinValue;

        public DateTime MaxDateTime { get; set; } = DateTime.MaxValue;

        public DragOrientations DragOrientation
        {
            get { return dragOrientation; }
            set
            {
                dragOrientation = value;
                UpdateContextMenu();
            }
        }

        public Color HoverColor { get; set; }

        public DateTime DateTime
        {
            get { return this.dateTime.Subtract(TimeSpan.FromMilliseconds(this.dateTime.Millisecond)); }
            set
            {
                this.dateTime = value;
                if (this.dateTime < this.MinDateTime)
                {
                    this.dateTime = MinDateTime;
                }
                if (this.dateTime > this.MaxDateTime)
                {
                    this.dateTime = MaxDateTime;
                }
            }
        }

        #endregion

        #region Private Methods

        // Returns the index of the rectangle (digitRects) under the mouse cursor
        private int DetermineDraggedDigit(MouseEventArgs e)
        {
            for (int i = 0; i < this.digitRects.Count; ++i)
            {
                if (this.digitRects[i].Contains(e.Location) && Token.IsDatePart(this.dateParts[i]))
                {
                    return i;
                }
            }

            return NO_DIGIT_DRAGGED;
        }

        // Return the value corresponding to current dragged digit
        private int GetDraggedValue()
        {
            var datePart = this.dateParts[this.draggedDigit];

            if (datePart.StartsWith("y", StringComparison.OrdinalIgnoreCase))
                return this.dateTime.Year;
            else if (datePart.StartsWith("M"))
                return this.dateTime.Month;
            else if (datePart.StartsWith("d", StringComparison.OrdinalIgnoreCase))
                return this.dateTime.Day;
            else if (datePart.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                return this.dateTime.Hour;
            else if (datePart.StartsWith("m"))
                return this.dateTime.Minute;
            else if (datePart.StartsWith("s", StringComparison.OrdinalIgnoreCase))
                return this.dateTime.Second;
            else
                return NO_DIGIT_DRAGGED;
        }

        private bool SetDraggedValue(int delta)
        {
            if (this.draggedDigit == NO_DIGIT_DRAGGED)
                return false;

            bool changed = true;
            try
            {
                var datePart = this.dateParts[this.draggedDigit];

                if (datePart.StartsWith("y", StringComparison.OrdinalIgnoreCase))
                    this.dateTime = this.dateTime.AddYears(delta);
                else if (datePart.StartsWith("M"))
                    this.dateTime = this.dateTime.AddMonths(delta);
                else if (datePart.StartsWith("d", StringComparison.OrdinalIgnoreCase))
                    this.dateTime = this.dateTime.AddDays(delta);
                else if (datePart.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                    this.dateTime = this.dateTime.AddHours(delta);
                else if (datePart.StartsWith("m"))
                    this.dateTime = this.dateTime.AddMinutes(delta);
                else if (datePart.StartsWith("s", StringComparison.OrdinalIgnoreCase))
                    this.dateTime = this.dateTime.AddSeconds(delta);
            }
            catch (Exception)
            {
                // invalid value dragged
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

        private void InitCustomRects(Section dateSection)
        {
            this.dateParts = dateSection
                .GeneralTextDateDurationParts
                .Select(p => DateFormatPartAdjuster.AdjustDateTimeFormatPart(p))
                .ToArray();

            Rectangle rect = this.ClientRectangle;
            int oneCharWidth = rect.Width / dateParts.Sum(s => s.Length);
            int left = rect.Left;

            this.digitRects.Clear();
            for (var i = 0; i < dateParts.Length; i++)
            {
                var datePart = dateParts[i];
                var s = datePart.Length * oneCharWidth;
                this.digitRects.Add(new Rectangle(left, rect.Top, s, rect.Height));
                left += s;
            }

        }

        private void InitDigiRects()
        {
            CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            var datePattern = string.Concat(
                culture.DateTimeFormat.ShortDatePattern,
                " ",
                culture.DateTimeFormat.LongTimePattern
            );

            var sections = Parser.ParseSections(datePattern, out bool syntaxError);
            var dateSection = sections.FirstOrDefault();

            if (dateSection == null)
            {
                sections = Parser.ParseSections("dd.MM.yyyy HH:mm:ss", out bool _);
                dateSection = sections.Single();
            }

            InitCustomRects(dateSection);
        }

        #endregion

        #region Events handler

        private void DateTimeDragControl_Load(object sender, EventArgs e)
        {
            InitDigiRects();

            BuildContextualMenu();
        }

        #endregion

        protected void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }

        protected void OnValueDragged(EventArgs e)
        {
            if (ValueDragged != null)
            {
                ValueDragged(this, e);
            }
        }

        public enum DragOrientations
        {
            Horizontal,
            Vertical,
            InvertedVertical
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
                {
                    e.Graphics.FillRectangle(hoverBrush, this.digitRects[this.draggedDigit]);
                }
            }

            // Display current value with user-defined date format and fixed time format ("HH:mm:ss")
            using (Brush brush = new SolidBrush(Color.Black))
            {
                for (var i = 0; i < dateParts.Length; i++)
                {
                    var datePart = dateParts[i];
                    var rect = digitRects[i];
                    string value;

                    if (Token.IsDatePart(datePart))
                    {
                        try
                        {
                            value = this.dateTime.ToString("-" + datePart + "-");
                            value = value.Substring(1, value.Length - 2);
                        }
                        catch
                        {
                            value = datePart;
                        }
                    }
                    else
                        value = datePart;

                    e.Graphics.DrawString(value, this.Font, brush, rect, this.digitsFormat);
                }
            }
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
                {
                    return;
                }
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
            {
                return;
            }

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
            {
                return;
            }

            int diff;
            if (this.DragOrientation == DragOrientations.Vertical)
            {
                diff = this.startMouseY - e.Y;
            }
            else if (this.DragOrientation == DragOrientations.InvertedVertical)
            {
                diff = this.startMouseY + e.Y;
            }
            else
            {
                diff = e.X - this.startMouseX;
            }

            int delta = diff / 5 - this.addedValue; // one unit per 5 pixels move

            if (delta != 0)
            {
                if (SetDraggedValue(delta))
                {
                    this.addedValue += delta;
                }
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
    }
}