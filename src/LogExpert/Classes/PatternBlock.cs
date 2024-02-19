using System.Collections.Generic;

namespace LogExpert.Classes
{
    public class QualityInfo
    {
        #region Fields

        public int quality;

        #endregion
    }

    public class PatternBlock
    {
        #region Fields

        public int blockId;

        public int endLine;

        // key: line num
        public Dictionary<int, QualityInfo> qualityInfoList = [];

        public SortedDictionary<int, int> srcLines = [];
        public int startLine;
        public int targetEnd;
        public SortedDictionary<int, int> targetLines = [];
        public int targetStart;
        public int weigth;

        #endregion

        #region Public methods

        public override string ToString()
        {
            return "srcStart=" + startLine + ", srcEnd=" + endLine + ", targetStart=" + targetStart +
                   ", targetEnd=" + targetEnd + ", weight=" + weigth;
        }

        #endregion
    }
}