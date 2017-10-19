using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class QualityInfo
  {
    public int quality;
  }

  public class PatternBlock
  {
    public int startLine;
    public int endLine;
    public int targetStart;
    public int targetEnd;
    public int weigth;
    public int blockId;
    public SortedDictionary<int, int> srcLines = new SortedDictionary<int, int>();
    public SortedDictionary<int, int> targetLines = new SortedDictionary<int, int>();
    // key: line num
    public Dictionary<int, QualityInfo> qualityInfoList = new Dictionary<int, QualityInfo>();

    public override string ToString()
    {
      return "srcStart=" + startLine + ", srcEnd=" + endLine + ", targetStart=" + targetStart +
        ", targetEnd=" + targetEnd + ", weight=" + weigth;
    }
  }
}
