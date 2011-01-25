using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class BookmarkWindow : Form
  {
    readonly LogWindow logWindow;

    public BookmarkWindow()
    {
      InitializeComponent();
    }

    public BookmarkWindow(LogWindow logWindow)
    {
      InitializeComponent();
      this.logWindow = logWindow;

      this.bookmarkDataGridView.CellValueNeeded += boomarkDataGridView_CellValueNeeded;
      this.bookmarkDataGridView.CellPainting += boomarkDataGridView_CellPainting;
    }

    public void SetColumnizer(ILogLineColumnizer columnizer)
    {
      this.logWindow.SetColumnizer(columnizer, this.bookmarkDataGridView);
      if (this.bookmarkDataGridView.ColumnCount > 0)
      {
        this.bookmarkDataGridView.Columns[0].Width = 20;
      }

      DataGridViewTextBoxColumn commentColumn = new DataGridViewTextBoxColumn();
      commentColumn.HeaderText = "Bookmark Comment";
      commentColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      commentColumn.Resizable = DataGridViewTriState.NotSet;
      commentColumn.DividerWidth = 1;
      commentColumn.ReadOnly = true;
      commentColumn.Width = 250;
      commentColumn.MinimumWidth = 130;
      this.bookmarkDataGridView.Columns.Insert(1, commentColumn);
      ShowCommentColumn(this.commentColumnCheckBox.Checked);
      ResizeColumns();
    }

    public void SetFont(string fontName, float fontSize)
    {
      Font font = new Font(new FontFamily(fontName), fontSize);
      //int lineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);
      //float lineSpacingPixel = font.Size * lineSpacing / font.FontFamily.GetEmHeight(FontStyle.Regular);

      this.bookmarkDataGridView.DefaultCellStyle.Font = font;
      this.bookmarkDataGridView.RowTemplate.Height = font.Height + 4;
      this.bookmarkDataGridView.Refresh();
    }

    /// <summary>
    /// Called from LogWindow after reloading and when double clicking a header divider.
    /// </summary>
    public void ResizeColumns()
    {
      //this.bookmarkDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
      for (int i = 2; i < this.bookmarkDataGridView.ColumnCount; ++i)
      {
        this.bookmarkDataGridView.AutoResizeColumn(i, DataGridViewAutoSizeColumnMode.DisplayedCells);
      }
    }


    void boomarkDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      try
      {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.BookmarkList.Count <= e.RowIndex)
        {
          e.Handled = false;
          return;
        }
        int lineNum = this.BookmarkList.Values[e.RowIndex].LineNum;

        //if (e.ColumnIndex == 1)
        //{
        //  CommentPainting(this.bookmarkDataGridView, lineNum, e);
        //}
        //else
        {
          this.logWindow.CellPainting(this.bookmarkDataGridView, lineNum, e);
        }
      }
      catch (Exception ex)
      {
        Logger.logError(ex.StackTrace);
      }
    }

    void boomarkDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
      if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.BookmarkList.Count <= e.RowIndex)
      {
        e.Value = "";
        return;
      }
      int lineNum = this.BookmarkList.Values[e.RowIndex].LineNum;
      if (e.ColumnIndex == 1)
      {
        e.Value = this.BookmarkList.Values[e.RowIndex].Text != null ? this.BookmarkList.Values[e.RowIndex].Text.Replace('\n', ' ').Replace('\r', ' ') : null;
      }
      else
      {
        int columnIndex = e.ColumnIndex > 1 ? e.ColumnIndex - 1 : e.ColumnIndex;
        e.Value = this.logWindow.GetCellValue(lineNum, columnIndex);
      }
    }


    private void CommentPainting(DataGridView gridView, int rowIndex, DataGridViewCellPaintingEventArgs e)
    {
      if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
      {
        /**/
        Color backColor = e.CellStyle.SelectionBackColor;
        Brush brush;
        if (gridView.Focused)
        {
          //Logger.logDebug("CellPaint Focus");
          brush = new SolidBrush(e.CellStyle.SelectionBackColor);
        }
        else
        {
          //Logger.logDebug("CellPaint No Focus");
          Color color = Color.FromArgb(255, 170, 170, 170);
          brush = new SolidBrush(color);
        }
        e.Graphics.FillRectangle(brush, e.CellBounds);
        brush.Dispose();
        /**/
      }
      else
      {
        e.CellStyle.BackColor = Color.White;
        e.PaintBackground(e.CellBounds, false);
      }
      e.PaintContent(e.CellBounds);
    }


    private void boomarkDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      //if (this.bookmarkDataGridView.CurrentRow != null)
      //{
      //  int lineNum = this.BookmarkList.Values[this.bookmarkDataGridView.CurrentRow.Index].LineNum;
      //  this.logWindow.SelectLogLine(lineNum);
      //}
    }

    private void boomarkDataGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
    {
      e.Handled = true;
      ResizeColumns();
    }

    private void bookmarkGridView_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        if (this.bookmarkDataGridView.CurrentCellAddress.Y >= 0 && this.bookmarkDataGridView.CurrentCellAddress.Y < this.BookmarkList.Count)
        {
          int lineNum = this.BookmarkList.Values[this.bookmarkDataGridView.CurrentCellAddress.Y].LineNum;
          this.logWindow.SelectLogLine(lineNum);
        }
        e.Handled = true;
      }
      if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
      {
        DeleteSelectedBookmarks();
      }
      if (e.KeyCode == Keys.Tab)
      {
        if (this.bookmarkDataGridView.Focused)
        {
          this.bookmarkTextBox.Focus();
          e.Handled = true;
        }
      }
    }

    private void bookmarkGridView_Enter(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.bookmarkDataGridView);
    }

    private void bookmarkGridView_Leave(object sender, EventArgs e)
    {
      InvalidateCurrentRow(this.bookmarkDataGridView);
    }

    private void DeleteSelectedBookmarks()
    {
      bool bookmarksPresent = false;
      List<int> lineNumList = new List<int>();
      foreach (DataGridViewRow row in this.bookmarkDataGridView.SelectedRows)
      {
        if (row.Index != -1)
        {
          lineNumList.Add(this.BookmarkList.Values[row.Index].LineNum);
          if (this.BookmarkList.Values[row.Index].Text != null && this.BookmarkList.Values[row.Index].Text.Length > 0)
          {
            bookmarksPresent = true;
          }
        }
      }
      if (bookmarksPresent)
      {
        if (MessageBox.Show("There are some comments in the bookmarks. Really remove bookmarks?", "LogExpert", MessageBoxButtons.YesNo) == DialogResult.No)
        {
          return;
        }
      }
      SortedList<int, Bookmark> newBookmarkList = new SortedList<int, Bookmark>();
      foreach (Bookmark bookmark in this.BookmarkList.Values)
      {
        if (!lineNumList.Contains(bookmark.LineNum))
        {
          newBookmarkList.Add(bookmark.LineNum, bookmark);
        }
      }
      this.BookmarkList = newBookmarkList;
      this.bookmarkDataGridView.RowCount = this.BookmarkList.Count;
      this.bookmarkDataGridView.Refresh();
    }

    private static void InvalidateCurrentRow(DataGridView gridView)
    {
      if (gridView.CurrentCellAddress.Y > -1)
        gridView.InvalidateRow(gridView.CurrentCellAddress.Y);
    }

    public void UpdateView()
    {
      this.bookmarkDataGridView.RowCount = 0;
      this.bookmarkDataGridView.RowCount = this.BookmarkList.Count;
      this.bookmarkDataGridView.Refresh();
    }

    protected SortedList<int, Bookmark> BookmarkList
    {
      get { return this.logWindow.BookmarkList; }
      set { this.logWindow.BookmarkList = value; }
    }

    private void deleteBookmarkssToolStripMenuItem_Click(object sender, EventArgs e)
    {
      DeleteSelectedBookmarks();
    }

    private void bookmarkTextBox_TextChanged(object sender, EventArgs e)
    {
      int rowIndex = this.bookmarkDataGridView.CurrentCellAddress.Y;
      if (rowIndex == -1)
      {
        return;
      }
      Bookmark bookmark = this.BookmarkList.Values[rowIndex];
      bookmark.Text = this.bookmarkTextBox.Text;
      OnBookmarkCommentChanged();
    }

    /// <summary>
    /// Called from LogWindow if the bookmark text was changed via popup window
    /// </summary>
    /// <param name="bookmark"></param>
    public void BookmarkTextChanged(Bookmark bookmark)
    {
      int rowIndex = this.bookmarkDataGridView.CurrentCellAddress.Y;
      if (rowIndex == -1)
      {
        return;
      }
      if (this.BookmarkList.Values[rowIndex] == bookmark)
      {
        this.bookmarkTextBox.Text = bookmark.Text;
      }
      this.bookmarkDataGridView.Refresh();
    }

    private void boomarkDataGridView_CurrentCellChanged(object sender, EventArgs e)
    {
      int rowIndex = this.bookmarkDataGridView.CurrentCellAddress.Y;
      CurrentRowChanged(rowIndex);
    }


    private void CurrentRowChanged(int rowIndex)
    {
      if (rowIndex == -1)
      {
        this.bookmarkTextBox.Text = "";
        return;
      }
      Bookmark bookmark = this.BookmarkList.Values[rowIndex];
      this.bookmarkTextBox.Text = bookmark.Text;
    }


    public void SelectBookmark(int lineNum)
    {
      if (this.BookmarkList.ContainsKey(lineNum))
      {
        if (this.bookmarkDataGridView.Rows.Count < this.BookmarkList.Count)
        {
          // just for the case... There was an exception but I cannot find the cause
          this.UpdateView();
        }
        int row = this.BookmarkList.IndexOfKey(lineNum);
        this.bookmarkDataGridView.CurrentCell = this.bookmarkDataGridView.Rows[row].Cells[0];
      }
    }

    public bool LineColumnVisible
    {
      set
      {
        this.bookmarkDataGridView.Columns[2].Visible = value;
      }
    }


    public delegate void BookmarkCommentChangedEventHandler(object sender, EventArgs e);
    public event BookmarkCommentChangedEventHandler BookmarkCommentChanged;
    protected void OnBookmarkCommentChanged()
    {
      BookmarkCommentChangedEventHandler handler = BookmarkCommentChanged;
      if (handler != null)
      {
        handler(this, new EventArgs());
      }
    }


    private void bookmarkDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
    {
      
    }

    private void bookmarkDataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      if (e.KeyCode == Keys.Tab)
      {
        e.IsInputKey = true;
      }
    }

    private void bookmarkDataGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
    {
      if (e.ColumnIndex != 0 || e.RowIndex <= -1 || e.RowIndex >= this.BookmarkList.Count)
      {
        return;
      }
      Bookmark bookmark = this.BookmarkList.Values[e.RowIndex];
      if (!string.IsNullOrEmpty(bookmark.Text))
      {
        e.ToolTipText = bookmark.Text;
        return;
      }
    }


    private void bookmarkDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 0 && e.RowIndex >= 0 && this.bookmarkDataGridView.CurrentRow != null)
      {
        int index = this.bookmarkDataGridView.CurrentRow.Index;
        int lineNum = this.BookmarkList.Values[this.bookmarkDataGridView.CurrentRow.Index].LineNum;
        this.logWindow.ToggleBookmark(lineNum);
        //this.BookmarkList.Remove(lineNum);
        //this.bookmarkDataGridView.RowCount = this.bookmarkDataGridView.RowCount - 1;
        //this.bookmarkDataGridView.Refresh();
        //this.logWindow.Refresh();
        if (index < this.bookmarkDataGridView.RowCount)
        {
          this.bookmarkDataGridView.CurrentCell = this.bookmarkDataGridView.Rows[index].Cells[0];
        }
        else
        {
          if (this.bookmarkDataGridView.RowCount > 0)
          {
            this.bookmarkDataGridView.CurrentCell = this.bookmarkDataGridView.Rows[this.bookmarkDataGridView.RowCount - 1].Cells[0];
          }
        }
        if (this.bookmarkDataGridView.RowCount > index)
        {
          CurrentRowChanged(index);
        }
        else
        {
          if (this.bookmarkDataGridView.RowCount > 0)
          {
            CurrentRowChanged(this.bookmarkDataGridView.RowCount - 1);
          }
          else
          {
            CurrentRowChanged(-1);
          }
        }
        return;
      }

      if (this.bookmarkDataGridView.CurrentRow != null && e.RowIndex >= 0)
      {
        int lineNum = this.BookmarkList.Values[this.bookmarkDataGridView.CurrentRow.Index].LineNum;
        this.logWindow.SelectAndEnsureVisible(lineNum, true);
      }
    }

    private void removeCommentsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Really remove bookmark comments for selected lines?", "LogExpert", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        List<int> lineNumList = new List<int>();
        foreach (DataGridViewRow row in this.bookmarkDataGridView.SelectedRows)
        {
          if (row.Index != -1)
          {
            this.BookmarkList.Values[row.Index].Text = "";
          }
        }
        this.bookmarkDataGridView.Refresh();
        this.logWindow.Refresh();
      }
    }


    private void ShowCommentColumn(bool show)
    {
      this.bookmarkDataGridView.Columns[1].Visible = show;
    }

    private void commentColumnCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      ShowCommentColumn(this.commentColumnCheckBox.Checked);
    }

    public bool ShowBookmarkCommentColumn
    {
      get { return this.commentColumnCheckBox.Checked; }
      set { this.commentColumnCheckBox.Checked = value; ShowCommentColumn(value); }
    }

  }
}
