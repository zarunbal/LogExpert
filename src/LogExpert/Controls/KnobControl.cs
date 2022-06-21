using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;

namespace LogExpert.Controls
{
    public partial class KnobControl : UserControl
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        private readonly StringFormat stringFormat = new StringFormat();

        private bool isShiftPressed = false;

        private int oldValue = 0;
        private int startMouseY = 0;
        private int value;

        #endregion

        #region cTor

        public KnobControl()
        {
            InitializeComponent();
            stringFormat.LineAlignment = StringAlignment.Far;
            stringFormat.Alignment = StringAlignment.Center;
        }

        #endregion

        #region Delegates

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event ValueChangedEventHandler ValueChanged;

        #endregion

        #region Properties

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public int Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                Refresh();
            }
        }


        public int Range
        {
            get { return this.MaxValue - this.MinValue; }
        }

        public int DragSensitivity { get; set; } = 3;

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color foregroundColor = this.Enabled ? Color.Black : Color.Gray;

            Pen blackPen = new Pen(foregroundColor, 1);
            Pen greyPen = new Pen(Color.Gray, 1);

            Rectangle rect = this.ClientRectangle;
            int height = this.Font.Height + 3;
            if (height > rect.Height)
            {
                height = rect.Height + 3;
            }
            rect.Inflate(-1, -height / 2);
            rect.Offset(0, -height / 2);
            e.Graphics.DrawEllipse(greyPen, rect);

            //rect = this.ClientRectangle;
            rect.Inflate(-2, -2);

            float startAngle = 135.0F + 270F * ((float) this.value / (float) this.Range);
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
            {
                return;
            }

            int sense = this.isShiftPressed ? this.DragSensitivity * 2 : this.DragSensitivity;

            int diff = this.startMouseY - e.Y;
            _logger.Debug("KnobDiff: {0}", diff);
            int range = this.MaxValue - this.MinValue;
            this.value = this.oldValue + diff / sense;
            if (this.value < this.MinValue)
            {
                this.value = this.MinValue;
            }
            if (this.value > this.MaxValue)
            {
                this.value = this.MaxValue;
            }
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

        #endregion

        protected void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }
    }
}