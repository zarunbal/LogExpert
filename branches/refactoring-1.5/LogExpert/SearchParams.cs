using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  [Serializable]
  public class SearchParams
  {
    [NonSerialized]
    public bool isShiftF3Pressed = false;

    public bool isForward = true;
    public bool isRegex = false;
    public bool isFromTop = false;
    public bool isCaseSensitive = false;
    public bool isFindNext;
    public string searchText;
    public int currentLine;
    public List<string> historyList = new List<string>();
  }
}
