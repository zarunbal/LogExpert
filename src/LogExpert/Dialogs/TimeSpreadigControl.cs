using LogExpert.Classes;

using NLog;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class TimeSpreadingControl : UserControl
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        #region Fields

        private Bitmap _bitmap = new(1, 1);
        private int _displayHeight = 1;
        private readonly int _edgeOffset = (int)Win32.GetSystemMetricsForDpi(Win32.SM_CYVSCROLL);
        private int _lastMouseY = 0;
        private readonly object _monitor = new();
        private int _rectHeight = 1;

        private TimeSpreadCalculator _timeSpreadCalc;
        private readonly ToolTip _toolTip;

        #endregion

        #region cTor

        public TimeSpreadingControl()
        {
            InitializeComponent();
            _toolTip = new ToolTip();
            Font = new Font("Courier New", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _toolTip.InitialDelay = 0;
            _toolTip.ReshowDelay = 0;
            _toolTip.ShowAlways = true;
            DoubleBuffered = false;
        }

        #endregion

        #region Delegates

        public delegate void LineSelectedEventHandler(object sender, SelectLineEventArgs e);

        #endregion

        #region Events

        public event LineSelectedEventHandler LineSelected;

        #endregion

        #region Properties

        public bool ReverseAlpha { get; set; }

        internal TimeSpreadCalculator TimeSpreadCalc
        {
            get => _timeSpreadCalc;
            set
            {
                //timeSpreadCalc.CalcDone -= timeSpreadCalc_CalcDone;
                _timeSpreadCalc = value;
                _timeSpreadCalc.CalcDone += TimeSpreadCalc_CalcDone;
                _timeSpreadCalc.StartCalc += TimeSpreadCalc_StartCalc;
            }
        }

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            lock (_monitor)
            {
                if (DesignMode)
                {
                    Brush bgBrush = new SolidBrush(Color.FromKnownColor(KnownColor.LightSkyBlue));
                    Rectangle rect = ClientRectangle;
                    rect.Inflate(0, -_edgeOffset);
                    e.Graphics.FillRectangle(bgBrush, rect);
                    bgBrush.Dispose();
                }
                else
                {
                    e.Graphics.DrawImage(_bitmap, 0, _edgeOffset);
                }
            }
        }

        #endregion

        #region Private Methods

        private TimeSpreadCalculator.SpreadEntry GetEntryForMouse(MouseEventArgs e)
        {
            List<TimeSpreadCalculator.SpreadEntry> list = TimeSpreadCalc.DiffList;
            int y = e.Y - _edgeOffset;
            if (y < 0)
            {
                y = 0;
            }
            else if (y >= ClientRectangle.Height - _edgeOffset * 3)
            {
                y = list.Count - 1;
            }
            else
            {
                y /= _rectHeight;
            }

            lock (_monitor)
            {
                if (y >= list.Count || y < 0)
                {
                    return null;
                }
                return list[y];
            }
        }

        private void DragContrast(MouseEventArgs e)
        {
            if (_lastMouseY == 0)
            {
                _lastMouseY = _lastMouseY = e.Y;
                return;
            }
            _timeSpreadCalc.Contrast += (_lastMouseY - e.Y) * 5;
            _lastMouseY = e.Y;
        }

        private void OnLineSelected(SelectLineEventArgs e)
        {
            LineSelected?.Invoke(this, e);
        }

        #endregion

        #region Events handler

        private void TimeSpreadCalc_CalcDone(object sender, EventArgs e)
        {
            _logger.Debug("timeSpreadCalc_CalcDone()");
            lock (_monitor)
            {
                Invalidate();
                Rectangle rect = ClientRectangle;
                rect.Size = new Size(rect.Width, rect.Height - _edgeOffset * 3);
                if (rect.Height < 1)
                {
                    return;
                }
                _bitmap = new Bitmap(rect.Width, rect.Height);
                Graphics gfx = Graphics.FromImage(_bitmap);
                Brush bgBrush = new SolidBrush(BackColor);
                gfx.FillRectangle(bgBrush, rect);
                bgBrush.Dispose();

                List<TimeSpreadCalculator.SpreadEntry> list = TimeSpreadCalc.DiffList;
                int step;
                if (list.Count >= _displayHeight)
                {
                    step = (int)Math.Round(list.Count / (double)_displayHeight);
                    _rectHeight = 1;
                }
                else
                {
                    step = 1;
                    _rectHeight = (int)Math.Round(_displayHeight / (double)list.Count);
                }
                Rectangle fillRect = new(0, 0, rect.Width, _rectHeight);

                lock (list)
                {
                    for (int i = 0; i < list.Count; i += step)
                    {
                        TimeSpreadCalculator.SpreadEntry entry = list[i];
                        int color = ReverseAlpha ? entry.Value : 255 - entry.Value;
                        if (color > 255)
                        {
                            color = 255;
                        }
                        if (color < 0)
                        {
                            color = 0;
                        }
                        Brush brush = new SolidBrush(Color.FromArgb(color, ForeColor));
                        //Brush brush = new SolidBrush(Color.FromArgb(color, color, color, color));
                        gfx.FillRectangle(brush, fillRect);
                        brush.Dispose();
                        fillRect.Offset(0, _rectHeight);
                    }
                }
            }
            BeginInvoke(new MethodInvoker(Refresh));
        }

        private void TimeSpreadCalc_StartCalc(object sender, EventArgs e)
        {
            lock (_monitor)
            {
                Invalidate();
                Rectangle rect = ClientRectangle;
                rect.Size = new Size(rect.Width, rect.Height - _edgeOffset * 3);
                if (rect.Height < 1)
                {
                    return;
                }
                //this.bmp = new Bitmap(rect.Width, rect.Height);
                Graphics gfx = Graphics.FromImage(_bitmap);

                Brush bgBrush = new SolidBrush(BackColor);
                Brush fgBrush = new SolidBrush(ForeColor);
                //gfx.FillRectangle(bgBrush, rect);

                StringFormat format = new(StringFormatFlags.DirectionVertical | StringFormatFlags.NoWrap);
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;

                RectangleF rectf = new(rect.Left, rect.Top, rect.Width, rect.Height);

                gfx.DrawString("Calculating time spread view...", Font, fgBrush, rectf, format);

                bgBrush.Dispose();
                fgBrush.Dispose();
            }

            BeginInvoke(new MethodInvoker(Refresh));
        }

        private void TimeSpreadingControl_SizeChanged(object sender, EventArgs e)
        {
            if (TimeSpreadCalc != null)
            {
                _displayHeight = ClientRectangle.Height - _edgeOffset * 3;
                TimeSpreadCalc.SetDisplayHeight(_displayHeight);
            }
        }

        private void TimeSpreadingControl_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void TimeSpreadingControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TimeSpreadCalculator.SpreadEntry entry = GetEntryForMouse(e);
                if (entry == null)
                {
                    return;
                }
                OnLineSelected(new SelectLineEventArgs(entry.LineNum));
            }
        }

        private void TimeSpreadingControl_MouseEnter(object sender, EventArgs e)
        {
            _toolTip.Active = true;
        }

        private void TimeSpreadingControl_MouseLeave(object sender, EventArgs e)
        {
            _toolTip.Active = false;
        }

        private void TimeSpreadingControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Y == _lastMouseY)
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                DragContrast(e);
                return;
            }

            TimeSpreadCalculator.SpreadEntry entry = GetEntryForMouse(e);
            if (entry == null)
            {
                return;
            }
            _lastMouseY = e.Y;
            string dts = entry.Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
            _toolTip.SetToolTip(this, "Line " + (entry.LineNum + 1) + "\n" + dts);
        }

        #endregion
    }
}