using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LogExpert.Classes;
using LogExpert.Entities.EventArgs;

namespace LogExpert.Controls
{
    public partial class PatternWindow : Form
    {
        #region Fields

        private readonly List<List<PatternBlock>> blockList = new List<List<PatternBlock>>();
        private PatternBlock currentBlock;
        private List<PatternBlock> currentList;

        private readonly LogWindow.LogWindow logWindow;
        private PatternArgs patternArgs = new PatternArgs();

        #endregion

        #region cTor

        public PatternWindow()
        {
            InitializeComponent();
        }

        public PatternWindow(LogWindow.LogWindow logWindow)
        {
            this.logWindow = logWindow;
            InitializeComponent();
            this.recalcButton.Enabled = false;
        }

        #endregion

        #region Properties

        public int Fuzzy
        {
            set { this.fuzzyKnobControl.Value = value; }
            get { return this.fuzzyKnobControl.Value; }
        }

        public int MaxDiff
        {
            set { this.maxDiffKnobControl.Value = value; }
            get { return this.maxDiffKnobControl.Value; }
        }

        public int MaxMisses
        {
            set { this.maxMissesKnobControl.Value = value; }
            get { return this.maxMissesKnobControl.Value; }
        }

        public int Weight
        {
            set { this.weigthKnobControl.Value = value; }
            get { return this.weigthKnobControl.Value; }
        }

        #endregion

        #region Public methods

        public void SetBlockList(List<PatternBlock> flatBlockList, PatternArgs patternArgs)
        {
            this.patternArgs = patternArgs;
            blockList.Clear();
            List<PatternBlock> singeList = new List<PatternBlock>();
            //int blockId = -1;
            for (int i = 0; i < flatBlockList.Count; ++i)
            {
                PatternBlock block = flatBlockList[i];
                singeList.Add(block);
                //if (block.blockId != blockId)
                //{
                //  singeList = new List<PatternBlock>();
                //  PatternBlock selfRefBlock = new PatternBlock();
                //  selfRefBlock.targetStart = block.startLine;
                //  selfRefBlock.targetEnd = block.endLine;
                //  selfRefBlock.blockId = block.blockId;
                //  singeList.Add(selfRefBlock);
                //  singeList.Add(block);
                //  this.blockList.Add(singeList);
                //  blockId = block.blockId;
                //}
                //else
                //{
                //  singeList.Add(block);
                //}
            }
            this.blockList.Add(singeList);
            this.Invoke(new MethodInvoker(SetBlockListGuiStuff));
        }


        public void SetColumnizer(ILogLineColumnizer columnizer)
        {
            this.logWindow.SetColumnizer(columnizer, this.patternHitsDataGridView);
            this.logWindow.SetColumnizer(columnizer, this.contentDataGridView);
            this.patternHitsDataGridView.Columns[0].Width = 20;
            this.contentDataGridView.Columns[0].Width = 20;

            DataGridViewTextBoxColumn blockInfoColumn = new DataGridViewTextBoxColumn();
            blockInfoColumn.HeaderText = "Weight";
            blockInfoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
            blockInfoColumn.Resizable = DataGridViewTriState.False;
            blockInfoColumn.DividerWidth = 1;
            blockInfoColumn.ReadOnly = true;
            blockInfoColumn.Width = 50;

            DataGridViewTextBoxColumn contentInfoColumn = new DataGridViewTextBoxColumn();
            contentInfoColumn.HeaderText = "Diff";
            contentInfoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
            contentInfoColumn.Resizable = DataGridViewTriState.False;
            contentInfoColumn.DividerWidth = 1;
            contentInfoColumn.ReadOnly = true;
            contentInfoColumn.Width = 50;

            this.patternHitsDataGridView.Columns.Insert(1, blockInfoColumn);
            this.contentDataGridView.Columns.Insert(1, contentInfoColumn);
        }

        public void SetFont(string fontName, float fontSize)
        {
            Font font = new Font(new FontFamily(fontName), fontSize);
            int lineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);
            float lineSpacingPixel = font.Size * lineSpacing / font.FontFamily.GetEmHeight(FontStyle.Regular);

            this.patternHitsDataGridView.DefaultCellStyle.Font = font;
            this.contentDataGridView.DefaultCellStyle.Font = font;
            //this.lineHeight = font.Height + 4;
            this.patternHitsDataGridView.RowTemplate.Height = font.Height + 4;
            this.contentDataGridView.RowTemplate.Height = font.Height + 4;
        }

        #endregion

        #region Private Methods

        private void SetBlockListGuiStuff()
        {
            this.patternHitsDataGridView.RowCount = 0;
            this.blockCountLabel.Text = "0";
            this.contentDataGridView.RowCount = 0;
            this.blockLinesLabel.Text = "0";
            this.recalcButton.Enabled = true;
            this.setRangeButton.Enabled = true;
            if (this.blockList.Count > 0)
            {
                SetCurrentList(this.blockList[0]);
            }
        }

        private void SetCurrentList(List<PatternBlock> patternList)
        {
            this.patternHitsDataGridView.RowCount = 0;
            this.currentList = patternList;
            this.patternHitsDataGridView.RowCount = this.currentList.Count;
            this.patternHitsDataGridView.Refresh();
            this.blockCountLabel.Text = "" + this.currentList.Count;
        }

        private int GetLineForHitGrid(int rowIndex)
        {
            int line;
            line = currentList[rowIndex].targetStart;
            return line;
        }

        private int GetLineForContentGrid(int rowIndex)
        {
            int line;
            line = currentBlock.targetStart + rowIndex;
            return line;
        }

        #endregion

        #region Events handler

        private void patternHitsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (this.currentList == null || e.RowIndex < 0)
            {
                return;
            }
            int rowIndex = GetLineForHitGrid(e.RowIndex);
            int colIndex = e.ColumnIndex;
            if (colIndex == 1)
            {
                e.Value = currentList[e.RowIndex].weigth;
            }
            else
            {
                if (colIndex > 1)
                {
                    colIndex--; // correct the additional inserted col
                }
                e.Value = this.logWindow.GetCellValue(rowIndex, colIndex);
            }
        }

        private void patternHitsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (this.currentList == null || e.RowIndex < 0)
            {
                return;
            }
            if (e.ColumnIndex == 1)
            {
                e.PaintBackground(e.CellBounds, false);
                int selCount = this.patternArgs.endLine - this.patternArgs.startLine;
                int maxWeight = this.patternArgs.maxDiffInBlock * selCount + selCount;
                if (maxWeight > 0)
                {
                    int width = (int) ((double) (int) e.Value / (double) maxWeight * (double) e.CellBounds.Width);
                    Rectangle rect = new Rectangle(e.CellBounds.X, e.CellBounds.Y, width, e.CellBounds.Height);
                    int alpha = 90 + (int) ((double) (int) e.Value / (double) maxWeight * (double) 165);
                    Color color = Color.FromArgb(alpha, 170, 180, 150);
                    Brush brush = new SolidBrush(color);
                    rect.Inflate(-2, -1);
                    e.Graphics.FillRectangle(brush, rect);
                    brush.Dispose();
                }
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
            else
            {
                DataGridView gridView = (DataGridView) sender;
                int rowIndex = GetLineForHitGrid(e.RowIndex);
                this.logWindow.CellPainting(gridView, rowIndex, e);
            }
        }

        private void patternHitsDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (this.currentList == null || patternHitsDataGridView.CurrentRow == null)
            //  return;
            //int rowIndex = GetLineForHitGrid(patternHitsDataGridView.CurrentRow.Index);

            //this.logWindow.SelectLogLine(rowIndex);
        }

        private void patternHitsDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (this.currentList == null || patternHitsDataGridView.CurrentRow == null)
            {
                return;
            }
            if (patternHitsDataGridView.CurrentRow.Index > this.currentList.Count - 1)
            {
                return;
            }
            this.contentDataGridView.RowCount = 0;
            this.currentBlock = this.currentList[patternHitsDataGridView.CurrentRow.Index];
            this.contentDataGridView.RowCount = this.currentBlock.targetEnd - this.currentBlock.targetStart + 1;
            this.contentDataGridView.Refresh();
            this.contentDataGridView.CurrentCell = this.contentDataGridView.Rows[0].Cells[0];
            this.blockLinesLabel.Text = "" + this.contentDataGridView.RowCount;
        }

        private void contentDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (this.currentBlock == null || e.RowIndex < 0)
            {
                return;
            }
            int rowIndex = GetLineForContentGrid(e.RowIndex);
            int colIndex = e.ColumnIndex;
            if (colIndex == 1)
            {
                QualityInfo qi;
                if (this.currentBlock.qualityInfoList.TryGetValue(rowIndex, out qi))
                {
                    e.Value = qi.quality;
                }
                else
                {
                    e.Value = "";
                }
            }
            else
            {
                if (colIndex != 0)
                {
                    colIndex--; // adjust the inserted column
                }
                e.Value = this.logWindow.GetCellValue(rowIndex, colIndex);
            }
        }

        private void contentDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (this.currentBlock == null || e.RowIndex < 0)
            {
                return;
            }
            DataGridView gridView = (DataGridView) sender;
            int rowIndex = GetLineForContentGrid(e.RowIndex);
            this.logWindow.CellPainting(gridView, rowIndex, e);
        }

        private void contentDataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.currentBlock == null || contentDataGridView.CurrentRow == null)
            {
                return;
            }
            int rowIndex = GetLineForContentGrid(contentDataGridView.CurrentRow.Index);

            this.logWindow.SelectLogLine(rowIndex);
        }

        private void recalcButton_Click(object sender, EventArgs e)
        {
            patternArgs.fuzzy = this.fuzzyKnobControl.Value;
            patternArgs.maxDiffInBlock = this.maxDiffKnobControl.Value;
            patternArgs.maxMisses = this.maxMissesKnobControl.Value;
            patternArgs.minWeight = this.weigthKnobControl.Value;
            this.logWindow.PatternStatistic(patternArgs);
            this.recalcButton.Enabled = false;
            this.setRangeButton.Enabled = false;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void contentDataGridView_ColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            this.contentDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        private void patternHitsDataGridView_ColumnDividerDoubleClick(object sender,
            DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            e.Handled = true;
            this.patternHitsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        private void setRangeButton_Click(object sender, EventArgs e)
        {
            this.logWindow.PatternStatisticSelectRange(patternArgs);
            this.recalcButton.Enabled = true;
            this.rangeLabel.Text = "Start: " + patternArgs.startLine + "\r\nEnd: " + patternArgs.endLine;
        }

        #endregion
    }
}