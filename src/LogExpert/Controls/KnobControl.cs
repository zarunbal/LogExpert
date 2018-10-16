using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;

namespace LogExpert
{
    public partial class KnobControl : UserControl
    {
        #region Delegates

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private readonly StringFormat stringFormat = new StringFormat();

        private bool isShiftPressed;

        private int oldValue;
        private int startMouseY;
        private int value;

        #endregion

        #region Public Events

        public event ValueChangedEventHandler ValueChanged;

        #endregion

        #region Ctor

        public KnobControl()
        {
            InitializeComponent();
            stringFormat.LineAlignment = StringAlignment.Far;
            stringFormat.Alignment = StringAlignment.Center;
        }

        #endregion

        #region Properties / Indexers

        public int DragSensitivity { get; set; } = 3;

        public int MaxValue { get; set; }

        public int MinValue { get; set; }


        public int Range => MaxValue - MinValue;

        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        #endregion

        #region Overrides

        protected override void OnKeyDown(KeyEventArgs e)
        {
            isShiftPressed = e.Shift;
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            isShiftPressed = e.Shift;
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Capture = true;
                startMouseY = e.Y;
                oldValue = Value;
            }

            if (e.Button == MouseButtons.Right)
            {
                Capture = false;
                Value = oldValue;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!Capture)
            {
                return;
            }

            int sense = isShiftPressed ? DragSensitivity * 2 : DragSensitivity;

            int diff = startMouseY - e.Y;
            _logger.Debug("KnobDiff: {0}", diff);
            int range = MaxValue - MinValue;
            value = oldValue + diff / sense;
            if (value < MinValue)
            {
                value = MinValue;
            }

            if (value > MaxValue)
            {
                value = MaxValue;
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
            oldValue = Value;
            OnValueChanged(new EventArgs());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color foregroundColor = Enabled ? Color.Black : Color.Gray;

            Pen blackPen = new Pen(foregroundColor, 1);
            Pen greyPen = new Pen(Color.Gray, 1);

            Rectangle rect = ClientRectangle;
            int height = Font.Height + 3;
            if (height > rect.Height)
            {
                height = rect.Height + 3;
            }

            rect.Inflate(-1, -height / 2);
            rect.Offset(0, -height / 2);
            e.Graphics.DrawEllipse(greyPen, rect);

            // rect = this.ClientRectangle;
            rect.Inflate(-2, -2);

            float startAngle = 135.0F + 270F * (value / (float)Range);
            float sweepAngle = 0.1F;
            e.Graphics.DrawPie(blackPen, rect, startAngle, sweepAngle);

            Brush brush = new SolidBrush(foregroundColor);
            RectangleF rectF = new RectangleF(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            e.Graphics.DrawString(string.Empty + value, Font, brush, rectF, stringFormat);

            blackPen.Dispose();
            greyPen.Dispose();
            brush.Dispose();
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

        #endregion
    }
}
