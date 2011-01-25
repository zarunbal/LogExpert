using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;


namespace CsvColumnizer
{
  class CsvColumn
  {
    private string name;

    public CsvColumn(string name)
    {
      this.name = name;
    }


    public string Name
    {
      get { return this.name; }
    }
  }

  [Serializable]
  public class CsvColumnizerConfig
  {
    public char delimiterChar;
    public char quoteChar;
    public char escapeChar;
    public bool hasFieldNames;
    public char commentChar;

    public void InitDefaults()
    {
      this.delimiterChar = ';';
      this.escapeChar = '"';
      this.quoteChar = '"';
      this.commentChar = '#';
      this.hasFieldNames = true;
    }

  }

  /// <summary>
  /// This Columnizer can parse CSV files. It uses the IInitColumnizer interface for support of dynamic field count.
  /// The IPreProcessColumnizer is implemented to read field names from the very first line of the file. Then
  /// the line is dropped. So it's not seen by LogExpert. The field names will be used as column names.
  /// </summary>
  public class CsvColumnizer : ILogLineColumnizer, IInitColumnizer, IColumnizerConfigurator, IPreProcessColumnizer
  {
    private CsvColumnizerConfig config;

    private IList<CsvColumn> columnList = new List<CsvColumn>();

    private string firstLine;

    #region ILogLineColumnizer Member

    public string GetName()
    {
      return "CSV Columnizer";
    }

    public string GetDescription()
    {
      return "Splits CSV files into columns.\r\n\r\nCredits:\r\nThis Columnizer uses the CsvReader class written by Sébastien Lorion. Downloaded from codeproject.com.\r\n";
    }

    public int GetColumnCount()
    {
      return this.columnList.Count;
    }

    public string[] GetColumnNames()
    {
      int i = 0;
      string[] names = new string[GetColumnCount()];
      foreach (CsvColumn column in this.columnList)
      {
        names[i++] = column.Name;
      }
      return names;
    }

    public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
    {
      return SplitCsvLine(line);
    }

    public bool IsTimeshiftImplemented()
    {
      return false;
    }

    public void SetTimeOffset(int msecOffset)
    {
      throw new NotImplementedException();
    }

    public int GetTimeOffset()
    {
      throw new NotImplementedException();
    }

    public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
    {
      throw new NotImplementedException();
    }

    public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region IInitColumnizer Member

    public void Selected(ILogLineColumnizerCallback callback)
    {
      this.columnList.Clear();
      string line = this.config.hasFieldNames ? this.firstLine : callback.GetLogLine(0);
      int i = 1;
      if (line != null)
      {
        string[] fields = SplitCsvLine(line);
        foreach (string field in fields)
        {
          if (this.config.hasFieldNames)
            this.columnList.Add(new CsvColumn(field));
          else
            this.columnList.Add(new CsvColumn("Column " + (i++)));
        }
      }
    }

    public void DeSelected(ILogLineColumnizerCallback callback)
    { 
      // nothing to do 
    }

    #endregion

    public string Text
    {
      get { return GetName(); }
    }


    private string[] SplitCsvLine(string line)
    {
      CsvReader csv = new CsvReader(new StringReader(line),
        false,
        this.config.delimiterChar,
        this.config.quoteChar,
        this.config.escapeChar,   // is '\0' when not checked in config dlg
        this.config.commentChar, 
        false);
      csv.ReadNextRecord();
      int fieldCount = csv.FieldCount;
      string[] fields = new string[fieldCount];
      for (int i = 0; i < fieldCount; ++i)
      {
        fields[i] = csv[i];
      }
      csv.Dispose();
      return fields;
    }



    #region IColumnizerConfigurator Member

    public void Configure(ILogLineColumnizerCallback callback, string configDir)
    {
      string configPath = configDir + "\\csvcolumnizer.dat";
      CsvColumnizerConfigDlg dlg = new CsvColumnizerConfigDlg(this.config);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        BinaryFormatter formatter = new BinaryFormatter();
        Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
        formatter.Serialize(fs, this.config);
        fs.Close();
        Selected(callback);
      }
    }

    public void LoadConfig(string configDir)
    {
      string configPath = configDir + "\\csvcolumnizer.dat";

      if (!File.Exists(configPath))
      {
        this.config = new CsvColumnizerConfig();
        this.config.InitDefaults();
      }
      else
      {
        Stream fs = File.OpenRead(configPath);
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
          this.config = (CsvColumnizerConfig)formatter.Deserialize(fs);
        }
        catch (SerializationException e)
        {
          MessageBox.Show(e.Message, "Deserialize");
          this.config = new CsvColumnizerConfig();
          this.config.InitDefaults();
        }
        finally
        {
          fs.Close();
        }
      }
    }

    #endregion

    #region IPreProcessColumnizer Member

    public string PreProcessLine(string logLine, int lineNum, int realLineNum)
    {
      if (realLineNum == 0)
      {
        this.firstLine = logLine;   // store for later field names and field count retrieval
      }

      if (this.config.hasFieldNames && realLineNum == 0)
        return null;                // hide from LogExpert

      if (this.config.commentChar != ' ' && logLine.StartsWith("" + this.config.commentChar))
      {
        return null;
      }

      return logLine;
    }

    #endregion
  }
}
