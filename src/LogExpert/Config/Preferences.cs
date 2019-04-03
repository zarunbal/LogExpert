using System;
using System.Collections.Generic;
using System.Drawing;
using LogExpert.Dialogs;

namespace LogExpert
{
    [Serializable]
    public class Preferences
    {
        #region Fields

        public bool allowOnlyOneInstance;
        public bool askForClose = false;
        public int bufferCount = 100;
        public List<ColumnizerMaskEntry> columnizerMaskList = new List<ColumnizerMaskEntry>();
        public string defaultEncoding;
        public bool filterSync = true;
        public bool filterTail = true;
        public bool followTail = true;
        public string fontName = "Courier New";
        public float fontSize = 9;
        public List<HighlightMaskEntry> highlightMaskList = new List<HighlightMaskEntry>();
        public bool isAutoHideFilterList = false;
        public bool isFilterOnLoad;
        public int lastColumnWidth = 2000;
        public int linesPerBuffer = 500;
        public int maximumFilterEntries = 30;
        public int maximumFilterEntriesDisplayed = 20;
        public bool maskPrio;
        public MultiFileOption multiFileOption;
        public MultifileOptions multifileOptions;
        public bool multiThreadFilter = true;
        public bool openLastFiles = true;
        public int pollingInterval = 250;
        public bool reverseAlpha = false;
        public string saveDirectory = null;
        public bool saveFilters = true;
        public SessionSaveLocation saveLocation = SessionSaveLocation.DocumentsDir;
        public bool saveSessions = true;
        public bool setLastColumnWidth;
        public bool showBubbles = true;
        public bool showColumnFinder;
        public Color showTailColor = Color.FromKnownColor(KnownColor.Blue);
        public bool showTailState = true;
        public bool showTimeSpread = false;
        public Color timeSpreadColor = Color.FromKnownColor(KnownColor.Gray);
        public bool timeSpreadTimeMode;
        public bool timestampControl = true;

        public DateTimeDragControl.DragOrientations timestampControlDragOrientation =
            DateTimeDragControl.DragOrientations.Horizontal;

        public List<ToolEntry> toolEntries = new List<ToolEntry>();
        public bool useLegacyReader;

        #endregion
    }
}