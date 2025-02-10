using LogExpert.Classes.DateTimeParser;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
        #region Fields

        private const int NO_DIGIT_DRAGGED = -1;
        private int _addedValue;


        private DateTime _dateTime;
        private readonly IList<Rectangle> _digitRects = new List<Rectangle>();
        private readonly StringFormat _digitsFormat = new();
        private int _draggedDigit;

        public DragOrientations dragOrientation = DragOrientations.Vertical;

        private readonly ToolStripItem toolStripItemHorizontalDrag = new ToolStripMenuItem();
        private readonly ToolStripItem toolStripItemVerticalDrag = new ToolStripMenuItem();
        private readonly ToolStripItem toolStripItemVerticalInvertedDrag = new ToolStripMenuItem();

        private int _oldValue;

        private string[] _dateParts;

        private int _startMouseX;
        private int _startMouseY;

        #endregion

        #region cTor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DateTimeDragControl()
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            _digitsFormat.LineAlignment = StringAlignment.Center;
            _digitsFormat.Alignment = StringAlignment.Near;
            _digitsFormat.Trimming = StringTrimming.None;
            _digitsFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

            _draggedDigit = NO_DIGIT_DRAGGED;
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
            get => dragOrientation;
            set
            {
                dragOrientation = value;
                UpdateContextMenu();
            }
        }

        public Color HoverColor { get; set; }

        public DateTime DateTime
        {
            get => _dateTime.Subtract(TimeSpan.FromMilliseconds(_dateTime.Millisecond));
            set
            {
                _dateTime = value;

                if (_dateTime < MinDateTime)
                {
                    _dateTime = MinDateTime;
                }
                if (_dateTime > MaxDateTime)
                {
                    _dateTime = MaxDateTime;
                }
            }
        }

        #endregion

        #region Private Methods

        // Returns the index of the rectangle (digitRects) under the mouse cursor
        private int DetermineDraggedDigit(MouseEventArgs e)
        {
            for (int i = 0; i < _digitRects.Count; ++i)
            {
                if (_digitRects[i].Contains(e.Location) && Token.IsDatePart(_dateParts[i]))
                {
                    return i;
                }
            }

            return NO_DIGIT_DRAGGED;
        }

        // Return the value corresponding to current dragged digit
        private int GetDraggedValue()
        {
            string datePart = _dateParts[_draggedDigit];

            if (datePart.StartsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                return _dateTime.Year;
            }

            if (datePart.StartsWith("M"))
            {
                return _dateTime.Month;
            }

            if (datePart.StartsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                return _dateTime.Day;
            }

            if (datePart.StartsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                return _dateTime.Hour;
            }

            if (datePart.StartsWith("m"))
            {
                return _dateTime.Minute;
            }

            return datePart.StartsWith("s", StringComparison.OrdinalIgnoreCase) ? _dateTime.Second : NO_DIGIT_DRAGGED;
        }

        private bool SetDraggedValue(int delta)
        {
            if (_draggedDigit == NO_DIGIT_DRAGGED)
            {
                return false;
            }

            bool changed = true;
            try
            {
                string datePart = _dateParts[_draggedDigit];

                if (datePart.StartsWith("y", StringComparison.OrdinalIgnoreCase))
                {
                    _dateTime = _dateTime.AddYears(delta);
                }
                else if (datePart.StartsWith("M"))
                {
                    _dateTime = _dateTime.AddMonths(delta);
                }
                else if (datePart.StartsWith("d", StringComparison.OrdinalIgnoreCase))
                {
                    _dateTime = _dateTime.AddDays(delta);
                }
                else if (datePart.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                {
                    _dateTime = _dateTime.AddHours(delta);
                }
                else if (datePart.StartsWith("m"))
                {
                    _dateTime = _dateTime.AddMinutes(delta);
                }
                else if (datePart.StartsWith("s", StringComparison.OrdinalIgnoreCase))
                {
                    _dateTime = _dateTime.AddSeconds(delta);
                }
            }
            catch (Exception)
            {
                // invalid value dragged
            }

            if (_dateTime > MaxDateTime)
            {
                _dateTime = MaxDateTime;
                changed = false;
            }
            if (_dateTime < MinDateTime)
            {
                _dateTime = MinDateTime;
                changed = false;
            }

            return changed;
        }

        private void InitCustomRects(Section dateSection)
        {
            _dateParts = dateSection
                .GeneralTextDateDurationParts
                .Select(DateFormatPartAdjuster.AdjustDateTimeFormatPart)
                .ToArray();

            Rectangle rect = ClientRectangle;
            int oneCharWidth = rect.Width / _dateParts.Sum(s => s.Length);
            int left = rect.Left;

            _digitRects.Clear();

            foreach (string datePart in _dateParts)
            {
                int s = datePart.Length * oneCharWidth;
                _digitRects.Add(new Rectangle(left, rect.Top, s, rect.Height));
                left += s;
            }

        }

        private void InitDigitRects()
        {
            CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            string datePattern = string.Concat(culture.DateTimeFormat.ShortDatePattern, " ", culture.DateTimeFormat.LongTimePattern);

            List<Section> sections = Parser.ParseSections(datePattern, out _);
            Section dateSection = sections.FirstOrDefault();

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
            InitDigitRects();

            BuildContextualMenu();
        }

        #endregion

        protected void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected void OnValueDragged(EventArgs e)
        {
            ValueDragged?.Invoke(this, e);
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
            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Name = "Timestamp selector";
            ContextMenuStrip.Items.Add(toolStripItemHorizontalDrag);
            ContextMenuStrip.Items.Add(toolStripItemVerticalDrag);
            ContextMenuStrip.Items.Add(toolStripItemVerticalInvertedDrag);

            toolStripItemHorizontalDrag.Click += OnToolStripItemHorizontalDragClick;
            toolStripItemHorizontalDrag.Text = "Drag horizontal";

            toolStripItemVerticalDrag.Click += OnToolStripItemVerticalDragClick;
            toolStripItemVerticalDrag.Text = "Drag vertical";

            toolStripItemVerticalInvertedDrag.Click += OnToolStripItemVerticalInvertedDragClick;
            toolStripItemVerticalInvertedDrag.Text = "Drag vertical inverted";

            ContextMenuStrip.Opening += OnContextMenuStripOpening;

            UpdateContextMenu();
        }

        private void UpdateContextMenu()
        {
            toolStripItemHorizontalDrag.Enabled = DragOrientation != DragOrientations.Horizontal;
            toolStripItemVerticalDrag.Enabled = DragOrientation != DragOrientations.Vertical;
            toolStripItemVerticalInvertedDrag.Enabled = DragOrientation != DragOrientations.InvertedVertical;
        }

        private void OnContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            if (Capture)
            {
                e.Cancel = true;
            }
        }

        private void OnToolStripItemHorizontalDragClick(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.Horizontal;
            toolStripItemHorizontalDrag.Enabled = false;
            toolStripItemVerticalDrag.Enabled = true;
            toolStripItemVerticalInvertedDrag.Enabled = true;
        }

        private void OnToolStripItemVerticalDragClick(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.Vertical;
            toolStripItemHorizontalDrag.Enabled = true;
            toolStripItemVerticalDrag.Enabled = false;
            toolStripItemVerticalInvertedDrag.Enabled = true;
        }

        private void OnToolStripItemVerticalInvertedDragClick(object sender, EventArgs e)
        {
            DragOrientation = DragOrientations.InvertedVertical;
            toolStripItemHorizontalDrag.Enabled = true;
            toolStripItemVerticalDrag.Enabled = true;
            toolStripItemVerticalInvertedDrag.Enabled = false;
        }

        #endregion

        #region Rendering

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Show what digit is dragged
            using (Brush hoverBrush = new SolidBrush(HoverColor))
            {
                if (_draggedDigit != NO_DIGIT_DRAGGED)
                {
                    e.Graphics.FillRectangle(hoverBrush, _digitRects[_draggedDigit]);
                }
            }

            // Display current value with user-defined date format and fixed time format ("HH:mm:ss")
            using (Brush brush = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < _dateParts.Length; i++)
                {
                    string datePart = _dateParts[i];
                    Rectangle rect = _digitRects[i];
                    string value;

                    if (Token.IsDatePart(datePart))
                    {
                        try
                        {
                            value = _dateTime.ToString("-" + datePart + "-");
                            value = value.Substring(1, value.Length - 2);
                        }
                        catch
                        {
                            value = datePart;
                        }
                    }
                    else
                    {
                        value = datePart;
                    }

                    e.Graphics.DrawString(value, Font, brush, rect, _digitsFormat);
                }
            }
        }

        private void DateTimeDragControl_Resize(object sender, EventArgs e)
        {
            InitDigitRects();
        }

        #endregion

        #region Mouse callbacks

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _draggedDigit = DetermineDraggedDigit(e);
                if (_draggedDigit == NO_DIGIT_DRAGGED)
                {
                    return;
                }
                Capture = true;
                _startMouseY = e.Y;
                _startMouseX = e.X;
                _oldValue = GetDraggedValue();
                _addedValue = 0;
            }
            else if (e.Button == MouseButtons.Right && Capture)
            {
                Capture = false;
                SetDraggedValue(0); //undo
            }
            Invalidate(); // repaint with the selected item (or none)
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!Capture)
            {
                return;
            }

            base.OnMouseUp(e);

            Capture = false;
            _draggedDigit = NO_DIGIT_DRAGGED;
            Invalidate(); // repaint without the selected item

            OnValueChanged(EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!Capture)
            {
                return;
            }

            int diff;
            switch (DragOrientation)
            {
                case DragOrientations.Vertical:
                    {
                        diff = _startMouseY - e.Y;
                        break;
                    }
                case DragOrientations.InvertedVertical:
                    {
                        diff = _startMouseY + e.Y;
                        break;
                    }
                default:
                    {
                        diff = e.X - _startMouseX;
                        break;
                    }
            }

            int delta = diff / 5 - _addedValue; // one unit per 5 pixels move

            if (delta == 0)
            {
                return;
            }

            if (SetDraggedValue(delta))
            {
                _addedValue += delta;
            }

            Invalidate();

            OnValueDragged(EventArgs.Empty);
        }

        private void DateTimeDragControl_MouseLeave(object sender, EventArgs e)
        {
            if (Capture)
            {
                return;
            }

            _draggedDigit = NO_DIGIT_DRAGGED;
            Refresh();
        }

        #endregion
    }
}
