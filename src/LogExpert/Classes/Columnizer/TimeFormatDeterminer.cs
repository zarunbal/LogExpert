
namespace LogExpert
{
    using System.Globalization;

    internal class TimeFormatDeterminer
    {
        #region FormatInfo helper class

        public class FormatInfo
        {
            #region cTor

            public FormatInfo(string dateFormat, string timeFormat, CultureInfo cultureInfo)
            {
                this.DateFormat = dateFormat;
                this.TimeFormat = timeFormat;
                this.CultureInfo = cultureInfo;
            }

            #endregion

            #region Properties

            public string DateFormat { get; }

            public string TimeFormat { get; }

            public CultureInfo CultureInfo { get; }

            public string DateTimeFormat
            {
                get { return this.DateFormat + " " + this.TimeFormat; }
            }

            public bool IgnoreFirstChar { get; set; }

            #endregion
        }

        #endregion

        protected FormatInfo formatInfo1 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo2 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo3 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo4 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss", new CultureInfo("en-US"));
        protected FormatInfo formatInfo5 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo6 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo7 = new FormatInfo("dd.MM.yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo8 = new FormatInfo("yyyy/MM/dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo9 = new FormatInfo("yyyy.MM.dd", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo10 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo11 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo12 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss", new CultureInfo("en-US"));
        protected FormatInfo formatInfo13 = new FormatInfo("dd MMM yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo14 = new FormatInfo("dd MMM yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo15 = new FormatInfo("dd MMM yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo16 = new FormatInfo("dd.MM.yy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
        protected FormatInfo formatInfo17 = new FormatInfo("yyyy-MM-dd", "HH:mm:ss:ffff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo18 = new FormatInfo("dd/MM/yyyy", "HH:mm:ss.fff", new CultureInfo("en-US"));
        protected FormatInfo formatInfo19 = new FormatInfo("dd/MM/yyyy", "HH:mm:ss:fff", new CultureInfo("en-US"));


        public FormatInfo DetermineDateTimeFormatInfo(string line)
        {
            if (line.Length < 21)
            {
                return null;
            }

            string temp = line;
            bool ignoreFirst = false;

            // determine if string starts with bracket and remove it
            if (temp[0] == '[' || temp[0] == '(' || temp[0] == '{')
            {
                temp = temp.Substring(1);
                ignoreFirst = true;

            }

            // dirty hardcoded probing of date/time format (much faster than DateTime.ParseExact()
            if (temp[2] == '.' && temp[5] == '.' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo1.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo1;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo7.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo7;
                }
                else
                {
                    this.formatInfo2.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo2;
                }
            }
            else if (temp[2] == '/' && temp[5] == '/' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo18.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo18;
                }
                else if (temp[19] == ':')
                {
                    this.formatInfo19.IgnoreFirstChar = ignoreFirst;
                    return formatInfo19;
                }
            }
            else if (temp[4] == '/' && temp[7] == '/' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo3.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo3;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo8.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo8;
                }
                else
                {
                    this.formatInfo4.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo4;
                }
            }
            else if (temp[4] == '.' && temp[7] == '.' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo5.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo5;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo9.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo9;
                }
                else
                {
                    this.formatInfo6.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo6;
                }
            }
            else if (temp[4] == '-' && temp[7] == '-' && temp[13] == ':' && temp[16] == ':')
            {
                if (temp[19] == '.')
                {
                    this.formatInfo10.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo10;
                }
                else if (temp[19] == ',')
                {
                    this.formatInfo11.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo11;
                }
                else if (temp[19] == ':')
                {
                    this.formatInfo17.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo17;
                }
                else
                {
                    this.formatInfo12.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo12;
                }
            }
            else if (temp[2] == ' ' && temp[6] == ' ' && temp[14] == ':' && temp[17] == ':')
            {
                if (temp[20] == ',')
                {
                    this.formatInfo13.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo13;
                }
                else if (temp[20] == '.')
                {
                    this.formatInfo14.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo14;
                }
                else
                {
                    this.formatInfo15.IgnoreFirstChar = ignoreFirst;
                    return this.formatInfo15;
                }
            }
            //dd.MM.yy HH:mm:ss.fff
            else if (temp[2] == '.' && temp[5] == '.' && temp[11] == ':' && temp[14] == ':' && temp[17] == '.')
            {
                this.formatInfo16.IgnoreFirstChar = ignoreFirst;
                return this.formatInfo16;
            }

            return null;
        }

        public FormatInfo DetermineTimeFormatInfo(string field)
        {
            // dirty hardcoded probing of time format (much faster than DateTime.ParseExact()
            if (field[2] == ':' && field[5] == ':')
            {
                if (field.Length > 8)
                {
                    if (field[8] == '.')
                    {
                        return this.formatInfo1;
                    }
                    else if (field[8] == ',')
                    {
                        return this.formatInfo7;
                    }
                }
                else
                {
                    return this.formatInfo2;
                }
            }
            return null;
        }
    }
}