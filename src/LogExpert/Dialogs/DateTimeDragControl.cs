using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    /// <summary>
    ///     This control displays date and time and allows user to interact with the individual parts using the mouse
    ///     to increment and decrement the values. The date format displayed is derived from the application UI locale.
    ///     We currently support only three: US (mm/dd/yyyy), French (yyyy-mm-dd) and German (dd.mm.yyyy).
    ///     The control raises events (ValueChanged, ValueDragged) when the date/time changes so that owner can react
    ///     accordingly.
    /// </summary>
    public partial class DateTimeDragControl : UserControl
    {
        #region Delegates

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        public delegate void ValueDraggedEventHandler(object sender, EventArgs e);

        #endregion

        #region Public Enums

        public enum DragOrientations
        {
            Horizontal,
            Vertical,
            InvertedVertical
        }

        #endregion

        #region Static/Constants

        private const int NO_DIGIT_DRAGGED = -1;

        #endregion

        #region Private Fields

        private readonly Rectangle[] digitRects = new Rectangle[6];
        private readonly StringFormat digitsFormat = new StringFormat();

        private readonly ToolStripItem item1 = new ToolStripMenuItem();
        private readonly ToolStripItem item2 = new ToolStripMenuItem();
        private readonly ToolStripItem item3 = new ToolStripMenuItem();
        private int addedValue;

        private string dateSeparator = ".";

        private DateTime dateTime;
        private int dayIndex;
        private int draggedDigit = NO_DIGIT_DRAGGED;

        public DragOrientations dragOrientation = DragOrientations.Vertical;

        private int monthIndex = 1;
        private int oldValue;

        private Components[] rectContents = new Components[6]; // for now, only the first three components may change position

        private int startMouseX;

        private int startMouseY;
        private int yearIndex = 2;

        #endregion

        #region Public Events

        public event ValueChangedEventHandler ValueChanged;
        public event ValueDraggedEventHandler ValueDragged;

        #endregion

        #region Ctor

        /// <summary>
        ///     Default Constructor
        /// </summary>
        public DateTimeDragControl()
        {
            InitializeComponent();

            digitsFormat.LineAlignment = StringAlignment.Center;
            digitsFormat.Alignment = StringAlignment.Near;
            digitsFormat.Trimming = StringTrimming.None;
            digitsFormat.FormatFlags =
                StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

            draggedDigit = NO_DIGIT_DRAGGED;
        }

        #endregion

        #region Properties / Indexers

        public DateTime DateTime
        {
            get => dateTime.Subtract(TimeSpan.FromMilliseconds(dateTime.Millisecond));
            set
            {
                dateTime = value;
                if (dateTime < MinDateTime)
                {
                    dateTime = MinDateTime;
                }

                if (dateTime > MaxDateTime)
                {
                    dateTime = MaxDateTime;
                }
            }
        }

        public DragOrientations DragOrientation
        {
            get => dragOrientation;
            set
            {
                dragOrientation = value;
                UpdateContextMenu();
            }
        }

        public Color HoverColor { get; set; }

        public DateTime MaxDateTime { get; set; } = DateTime.MaxValue;

        public DateTime MinDateTime { get; set; } = DateTime.MinValue;

        #endregion

        #region Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                draggedDigit = DetermineDraggedDigit(e);
                if (draggedDigit == NO_DIGIT_DRAGGED)
                {
                    return;
                }

                Capture = true;
                startMouseY = e.Y;
                startMouseX = e.X;
                oldValue = GetDraggedValue();
                addedValue = 0;
            }
            else if (e.Button == MouseButtons.Right && Capture)
            {
                Capture = false;
                SetDraggedValue(0); // undo
            }

            Invalidate(); // repaint with the selected item (or none)
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!Capture)
            {
                return;
            }

            int diff;
            if (DragOrientation == DragOrientations.Vertical)
            {
                diff = startMouseY - e.Y;
            }
            else if (DragOrientation == DragOrientations.InvertedVertical)
            {
                diff = startMouseY + e.Y;
            }
            else
            {
                diff = e.X - startMouseX;
            }

            int delta = diff / 5 - addedValue; // one unit per 5 pixels move

            if (delta != 0)
            {
                if (SetDraggedValue(delta))
                {
                    addedValue += delta;
                }

                Invalidate();

                OnValueDragged(new EventArgs());
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!Capture)
            {
                return;
            }

            base.OnMouseUp(e);

            Capture = false;
            draggedDigit = NO_DIGIT_DRAGGED;
            Invalidate(); // repaint without the selected item

            OnValueChanged(new EventArgs());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Show what digit is dragged
            using (Brush hoverBrush = new SolidBrush(HoverColor))
            {
                if (draggedDigit != NO_DIGIT_DRAGGED)
                {
                    e.Graphics.FillRectangle(hoverBrush, digitRects[draggedDigit]);
                }
            }

            // Display current value with user-defined date format and fixed time format ("HH:mm:ss")
            using (Brush brush = new SolidBrush(Color.Black))
            {
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[dayIndex],
                    FormatDigitLeadingZeros(dateTime.Day, 2, dayIndex < 2 ? dateSeparator : string.Empty));
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[monthIndex],
                    FormatDigitLeadingZeros(dateTime.Month, 2, monthIndex < 2 ? dateSeparator : string.Empty));
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[yearIndex],
                    FormatDigitLeadingZeros(dateTime.Year, 4, yearIndex < 2 ? dateSeparator : string.Empty));
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[3],
                    FormatDigitLeadingZeros(dateTime.Hour, 2, ":"));
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[4],
                    FormatDigitLeadingZeros(dateTime.Minute, 2, ":"));
                DrawDigit(e.Graphics, brush, e.ClipRectangle, digitRects[5],
                    FormatDigitLeadingZeros(dateTime.Second, 2, string.Empty));
            }
        }

        #endregion

        #region Event handling Methods

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

        #endregion

        #region Private Methods

        private void BuildContextualMenu()
        {
            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Name = "Timestamp selector";
            ContextMenuStrip.Items.Add(item1);
            ContextMenuStrip.Items.Add(item2);
            ContextMenuStrip.Items.Add(item3);
            item1.Click += item1_Click;
            item1.Text = "Drag horizontal";
            item2.Click += item2_Click;
            item2.Text = "Drag vertical";
            item3.Click += item3_Click;
            item3.Text = "Drag vertical inverted";

            ContextMenuStrip.Opening += ContextMenuStrip_Opening;

            UpdateContextMenu();
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (Capture)
            {
                e.Cancel = true;
            }
        }

        private void DateTimeDragControl_Load(object sender, EventArgs e)
        {
            InitDigiRects();

            BuildContextualMenu();
        }

        private void DateTimeDragControl_MouseLeave(object sender, EventArgs e)
        {
            if (!Capture)
            {
                draggedDigit = NO_DIGIT_DRAGGED;
                Refresh();
            }
        }

        private void DateTimeDragControl_Resize(object sender, EventArgs e)
        {
            InitDigiRects();
        }

        // Returns the index of the rectangle (digitRects) under the mouse cursor
        private int DetermineDraggedDigit(MouseEventArgs e)
        {
            for (int i = 0; i < digitRects.Length; ++i)
            {
                if (digitRects[i].Contains(e.Location))
                {
                    return i;
                }
            }

            return NO_DIGIT_DRAGGED;
        }

        private void DrawDigit(Graphics g, Brush brush, Rectangle clip, Rectangle r, string value)
        {
            g.DrawString(value, Font, brush, r, digitsFormat);

            // using (Pen pen = new Pen(brush))
            // g.DrawRectangle(pen, r);
        }

        private static string FormatDigitLeadingZeros(int number, int length, string postSeparator)
        {
            return number.ToString("D" + length) + postSeparator;
        }

        // Return the value corresponding to current dragged digit
        private int GetDraggedValue()
        {
            switch (rectContents[draggedDigit])
            {
                case Components.Day: return dateTime.Day;
                case Components.Month: return dateTime.Month;
                case Components.Year: return dateTime.Year;
                case Components.Hours: return dateTime.Hour;
                case Components.Minutes: return dateTime.Minute;
                case Components.Seconds: return dateTime.Second;
                default:
                    return NO_DIGIT_DRAGGED;
            }
        }

        private int IndexOf(Components component)
        {
            for (int i = 0; i < rectContents.Length; i++)
            {
                if (rectContents[i] == component)
                {
                    return i;
                }
            }

            return -1;
        }

        private void InitAmericanRects()
        {
            dateSeparator = "/";
            rectContents = new[]
            {
                Components.Month, Components.Day, Components.Year, Components.Hours, Components.Minutes,
                Components.Seconds
            };

            Rectangle rect = ClientRectangle;
            int oneCharWidth = rect.Width / 19;
            int step = (rect.Width - oneCharWidth) /
                       7; // separate the 19 characters into seven pieces: MM/, dd/, yyyy, " ", HH:, mm:, ss
            int left = rect.Left;
            for (int i = 0; i < digitRects.Length; ++i)
            {
                int s = step;
                if (i == 2)
                {
// the year is 4 chars instead of 3
                    s = step + oneCharWidth;
                }
                else if (i == 5)
                {
// seconds are 2 chars instead of 3
                    s = step - oneCharWidth + 2;
                }

                if (i == 3)
                {
                    left += step; // skip space
                }

                digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
                left += s;
            }
        }

        private void InitDigiRects()
        {
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
            string cultureName = culture.Parent != null ? culture.Parent.Name : string.Empty;

            if (cultureName == "fr")
            {
                InitFrenchRects();
            }
            else if (cultureName == "en")
            {
                InitAmericanRects();
            }
            else
            {
                InitGermanRects(); // default
            }

            yearIndex = IndexOf(Components.Year);
            monthIndex = IndexOf(Components.Month);
            dayIndex = IndexOf(Components.Day);
        }

        private void InitFrenchRects()
        {
            dateSeparator = "-";
            rectContents = new[]
            {
                Components.Year, Components.Month, Components.Day, Components.Hours, Components.Minutes,
                Components.Seconds
            };

            Rectangle rect = ClientRectangle;
            int oneCharWidth = rect.Width / 19;
            int step = (rect.Width - oneCharWidth) /
                       7; // separate the 19 characters into seven pieces: yyyy-, MM-, dd, " ", HH:, mm:, ss
            int left = rect.Left;
            for (int i = 0; i < digitRects.Length; ++i)
            {
                int s = step;
                if (i == 0)
                {
// the year is 5 chars instead of 3
                    s = step + 2 * oneCharWidth;
                }
                else if (i == 2 || i == 5)
                {
// day and seconds are 2 chars instead of 3
                    s = step - oneCharWidth + 2;
                }

                if (i == 3)
                {
                    left += step; // skip space
                }

                digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
                left += s;
            }
        }

        private void InitGermanRects()
        {
            dateSeparator = ".";
            rectContents = new[]
            {
                Components.Day, Components.Month, Components.Year, Components.Hours, Components.Minutes,
                Components.Seconds
            };

            Rectangle rect = ClientRectangle;
            int oneCharWidth = rect.Width / 19;
            int step = (rect.Width - oneCharWidth) /
                       7; // separate the 19 characters into seven pieces: dd., MM., yyyy, " ", HH:, mm:, ss
            int left = rect.Left;
            for (int i = 0; i < digitRects.Length; ++i)
            {
                int s = step;
                if (i == 2)
                {
// the year is 4 chars instead of 3
                    s = step + oneCharWidth;
                }
                else if (i == 5)
                {
// seconds are 2 chars instead of 3
                    s = step - oneCharWidth + 2;
                }

                if (i == 3)
                {
                    left += step; // skip space
                }

                digitRects[i] = new Rectangle(left, rect.Top, s, rect.Height);
                left += s;
            }
        }

        private void item1_Click(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.Horizontal;
            item1.Enabled = false;
            item2.Enabled = true;
            item3.Enabled = true;
        }

        private void item2_Click(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.Vertical;
            item1.Enabled = true;
            item2.Enabled = false;
            item3.Enabled = true;
        }

        private void item3_Click(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.InvertedVertical;
            item1.Enabled = true;
            item2.Enabled = true;
            item3.Enabled = false;
        }

        private bool SetDraggedValue(int delta)
        {
            bool changed = true;
            try
            {
                switch (rectContents[draggedDigit])
                {
                    case Components.Day:
                        dateTime = dateTime.AddDays(delta);
                        break;
                    case Components.Month:
                        dateTime = dateTime.AddMonths(delta);
                        break;
                    case Components.Year:
                        dateTime = dateTime.AddYears(delta);
                        break;

                    case Components.Hours:
                        dateTime = dateTime.AddHours(delta);
                        break;
                    case Components.Minutes:
                        dateTime = dateTime.AddMinutes(delta);
                        break;
                    case Components.Seconds:
                        dateTime = dateTime.AddSeconds(delta);
                        break;
                }
            }
            catch (Exception)
            {
                // invalid value dragged
            }

            if (dateTime > MaxDateTime)
            {
                dateTime = MaxDateTime;
                changed = false;
            }

            if (dateTime < MinDateTime)
            {
                dateTime = MinDateTime;
                changed = false;
            }

            return changed;
        }

        private void UpdateContextMenu()
        {
            item1.Enabled = DragOrientation != DragOrientations.Horizontal;
            item2.Enabled = DragOrientation != DragOrientations.Vertical;
            item3.Enabled = DragOrientation != DragOrientations.InvertedVertical;
        }

        #endregion

        #region Nested type: Components

        private enum Components
        {
            Year,
            Month,
            Day,
            Hours,
            Minutes,
            Seconds
        }

        #endregion
    }
}
