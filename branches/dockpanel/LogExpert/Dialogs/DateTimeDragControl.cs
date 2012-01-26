using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class DateTimeDragControl : UserControl
  {
    public enum DragOrientations
    {
      Horizonzal,
      Vertical,
      InvertedVertical
    }

    DateTime minDateTime = DateTime.MinValue;

    public DateTime MinDateTime
    {
      get { return minDateTime; }
      set { minDateTime = value; }
    }
    DateTime maxDateTime = DateTime.MaxValue;

    public DateTime MaxDateTime
    {
      get { return maxDateTime; }
      set { maxDateTime = value; }
    }

    public DragOrientations dragOrientation = DragOrientations.Vertical;

    public  DragOrientations DragOrientation
    {
      get { return dragOrientation; }
      set 
      { 
        dragOrientation = value;
        UpdateContextMenu();
      }
    }

    Color hoverColor;

    public Color HoverColor
    {
      get { return hoverColor; }
      set { hoverColor = value; }
    }



    DateTime dateTime = new DateTime();
    Rectangle [] digitRects = new Rectangle[7];
    StringFormat stringFormat = new StringFormat();
    ToolStripItem item1 = new ToolStripMenuItem();
    ToolStripItem item2 = new ToolStripMenuItem();
    ToolStripItem item3 = new ToolStripMenuItem();

    int startMouseY = 0;
    int startMouseX = 0;
    int oldValue = 0;
    int changeValue = 0;
    int draggedDigit = 0;


    public DateTime DateTime
    {
      get { return this.dateTime.Subtract(TimeSpan.FromMilliseconds(this.dateTime.Millisecond)); }
      set 
      { 
        this.dateTime = value;
        if (this.dateTime < this.minDateTime)
          this.dateTime = minDateTime;
        if (this.dateTime > this.maxDateTime)
          this.dateTime = maxDateTime;
      }

    }

    public DateTimeDragControl()
    {
      InitializeComponent();
    }

    private void DateTimeDragControl_Load(object sender, EventArgs e)
    {
      InitDigiRects();
      stringFormat.LineAlignment = StringAlignment.Center;
      stringFormat.Alignment = StringAlignment.Near;
      stringFormat.Trimming = StringTrimming.None;
      stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

      this.ContextMenuStrip = new ContextMenuStrip();
      this.ContextMenuStrip.Name= "Timestamp selector";
      this.ContextMenuStrip.Items.Add(item1);
      this.ContextMenuStrip.Items.Add(item2);
      this.ContextMenuStrip.Items.Add(item3);
      item1.Click += new EventHandler(item1_Click);
      item1.Text = "Drag horizontal";
      item2.Click += new EventHandler(item2_Click);
      item2.Text = "Drag vertical";
      item3.Click += new EventHandler(item3_Click);
      item3.Text = "Drag vertical inverted"; 
      UpdateContextMenu();
      this.draggedDigit = -1;

      ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
    }

    private void UpdateContextMenu()
    {
      item1.Enabled = this.DragOrientation != DragOrientations.Horizonzal;
      item2.Enabled = this.DragOrientation != DragOrientations.Vertical;
      item3.Enabled = this.DragOrientation != DragOrientations.InvertedVertical;
    }

    void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
      if (this.Capture)
      {
        e.Cancel = true;
      }
    }

    void item1_Click(object sender, EventArgs e)
    {
      this.DragOrientation = DragOrientations.Horizonzal;
      item1.Enabled = false;
      item2.Enabled = true;
      item3.Enabled = true;
    }

    void item2_Click(object sender, EventArgs e)
    {
      this.DragOrientation = DragOrientations.Vertical;
      item1.Enabled = true;
      item2.Enabled = false;
      item3.Enabled = true;
    }

    void item3_Click(object sender, EventArgs e)
    {
      this.DragOrientation = DragOrientations.InvertedVertical;
      item1.Enabled = true;
      item2.Enabled = true;
      item3.Enabled = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);

      string dts = this.dateTime.ToString("dd.MM.yyyy HH:mm:ss");
      if (dts.Length < 19)
        return;
      
      Brush hoverBrush = new SolidBrush(this.HoverColor);
      if (this.draggedDigit != -1)
      {
        e.Graphics.FillRectangle(hoverBrush, this.digitRects[this.draggedDigit]);
      }
      hoverBrush.Dispose();

      Brush brush = new SolidBrush(Color.Black);
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[0], dts.Substring(0, 3));
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[1], dts.Substring(3, 3));
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[2], dts.Substring(6, 4));
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[4], dts.Substring(11, 3));
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[5], dts.Substring(14, 3));
      DrawDigit(e.Graphics, brush, e.ClipRectangle, this.digitRects[6], dts.Substring(17, 2));

      brush.Dispose();
    }

    private void DrawDigit(Graphics g, Brush brush, Rectangle clip, Rectangle r, string value)
    {
      //Pen pen = new Pen(Color.Black);
      Rectangle rect = r;
      r.Offset(clip.Left, clip.Right);
      RectangleF rectF = new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
      g.DrawString(value, this.Font, brush, rectF, this.stringFormat);
      //g.DrawRectangle(pen, rect);
      //pen.Dispose();
    }

    private void DateTimeDragControl_Resize(object sender, EventArgs e)
    {
      InitDigiRects();
    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);

      if (e.Button == MouseButtons.Left)
      {
        this.draggedDigit = DetermineDraggedDigit(e);
        if (this.draggedDigit == -1)
          return;
        Capture = true;
        startMouseY = e.Y;
        startMouseX = e.X;
        this.oldValue = this.changeValue = GetDraggedValue();
      }
      if (e.Button == MouseButtons.Right && this.Capture)
      {
        Capture = false;
        SetDraggedValue(this.changeValue, this.oldValue);
        this.Invalidate();
      }
      if (e.Button == MouseButtons.Right && !this.Capture)
      {

      }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (!Capture)
        return;
      base.OnMouseUp(e);
      Capture = false;
      this.oldValue = this.changeValue;
      OnValueChanged(new EventArgs());
      this.draggedDigit = -1;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      if (!Capture)
      {
        //this.draggedDigit = DetermineDraggedDigit(e);
        //Refresh();
        return;
      }

      int diff;
      if (this.DragOrientation == DragOrientations.Vertical)
        diff = this.startMouseY - e.Y;
      else if (this.DragOrientation == DragOrientations.InvertedVertical)
        diff = this.startMouseY + e.Y;
      else
        diff = e.X - this.startMouseX;
      int newValue = this.oldValue + diff / 5;
      SetDraggedValue(this.changeValue, newValue);
      this.changeValue = newValue;
      this.Invalidate();
      OnValueDragged(new EventArgs());
    }


    private int DetermineDraggedDigit(MouseEventArgs e)
    {
      for (int i = 0; i < this.digitRects.Length; ++i)
      {
        if (this.digitRects[i].Contains(e.Location))
        {
          if (i == 3)
            return -1;
          return i;
        }
      }
      return -1;
    }

    private int GetDraggedValue()
    {
      switch (this.draggedDigit)
      {
        case 0:
          return this.dateTime.Day;
        case 1:
          return this.dateTime.Month;
        case 2:
          return this.dateTime.Year;
        case 3:
          return -1;
        case 4:
          return this.dateTime.Hour;
        case 5:
          return this.dateTime.Minute;
        case 6:
          return this.dateTime.Second;
        default:
          return -1;
      }
    }

    private void SetDraggedValue(int oldValue, int newValue)
    {
      try
      {
        switch (this.draggedDigit)
        {
          case 0:
            this.dateTime = this.dateTime.AddDays(newValue - oldValue);
            break;
          case 1:
            this.dateTime = this.dateTime.AddMonths(newValue - oldValue);
            break;
          case 2:
            this.dateTime = this.dateTime.AddYears(newValue - oldValue);
            break;
          case 4:
            this.dateTime = this.dateTime.AddHours(newValue - oldValue);
            break;
          case 5:
            this.dateTime = this.dateTime.AddMinutes(newValue - oldValue);
            break;
          case 6:
            this.dateTime = this.dateTime.AddSeconds(newValue - oldValue);
            break;
        }
      }
      catch (Exception)  // invalid value dragged
      { }

      if (this.dateTime > this.MaxDateTime)
      {
        this.dateTime = this.MaxDateTime;
      }
      if (this.dateTime < this.MinDateTime)
      {
        this.dateTime = this.MinDateTime;
      }
    }


    private void InitDigiRects()
    {
      Rectangle rect = this.ClientRectangle;
      int step = (rect.Width - (rect.Width / 19)) / 7;
      int left = rect.Left;
      for (int i = 0; i < this.digitRects.Length; ++i)
      {
        if (i == 2)
        {
          this.digitRects[i] = new Rectangle(left, rect.Top, step + (rect.Width / 19), rect.Height);
          left += (rect.Width / 19) + step;
        }
        else
        {
          this.digitRects[i] = new Rectangle(left, rect.Top, step, rect.Height);
          left += step;
        }
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

    public delegate void ValueDraggedEventHandler(object sender, EventArgs e);
    public event ValueDraggedEventHandler ValueDragged;
    protected void OnValueDragged(EventArgs e)
    {
      if (ValueDragged != null)
      {
        ValueDragged(this, e);
      }
    }

    private void DateTimeDragControl_MouseEnter(object sender, EventArgs e)
    {
    }

    private void DateTimeDragControl_MouseLeave(object sender, EventArgs e)
    {
      if (!this.Capture)
      {
        this.draggedDigit = -1;
        Refresh();
      }
    }
  }
}
