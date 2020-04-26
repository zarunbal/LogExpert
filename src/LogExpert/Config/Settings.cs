using System;
using System.Collections.Generic;
using System.Drawing;

namespace LogExpert
{
    [Serializable]
    public class Settings
    {
        #region Fields

        public bool alwaysOnTop;

        public Rectangle appBounds;
        
        public Rectangle appBoundsFullscreen;
        
        public IList<ColumnizerHistoryEntry> columnizerHistoryList = new List<ColumnizerHistoryEntry>();
        
        public List<ColorEntry> fileColors = new List<ColorEntry>();
        
        public List<string> fileHistoryList = new List<string>();
        
        public List<string> filterHistoryList = new List<string>();
        
        public List<FilterParams> filterList = new List<FilterParams>();
        
        public FilterParams filterParams = new FilterParams();
        
        public List<string> filterRangeHistoryList = new List<string>();
        
        public bool hideLineColumn;

        public List<HilightEntry> hilightEntryList = new List<HilightEntry>(); // legacy. is automatically converted to highlight groups on settings load

        public List<HilightGroup> hilightGroupList = new List<HilightGroup>(); // should be in Preferences but is here for mistake. Maybe I migrate it some day.

        public bool isMaximized;
        
        public string lastDirectory;
        
        public List<string> lastOpenFilesList = new List<string>();

        public Preferences preferences = new Preferences();
        
        public RegexHistory regexHistory = new RegexHistory();
        
        public List<string> searchHistoryList = new List<string>();
        
        public SearchParams searchParams = new SearchParams();
        
        public IList<string> uriHistoryList = new List<string>();
        
        public int versionBuild;

        #endregion
    }
}