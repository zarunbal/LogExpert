using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using LogExpert;
using System.IO;
using System.Windows.Forms;

namespace UnitTests
{
  [TestFixture]
  class ReaderTest
  {
    [TestFixtureSetUp]
    public void Boot()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }


    [Test]
    public void compareReaderImplementations()
    {
      string DataPath = "..\\..\\data\\";

      string fileName = "50 MB.txt";
      EncodingOptions encOpts = new EncodingOptions();
      encOpts.Encoding = Encoding.UTF8;

      Stream s1 = new FileStream(DataPath + fileName, FileMode.Open, FileAccess.Read);
      PositionAwareStreamReader r1 = new PositionAwareStreamReader(s1, encOpts, false);

      Stream s2 = new FileStream(DataPath + fileName, FileMode.Open, FileAccess.Read);
      PositionAwareStreamReader r2 = new PositionAwareStreamReader(s2, encOpts, true);

      for (int lineNum = 0; ; lineNum++)
      {
        string line1 = r1.ReadLine();
        string line2 = r2.ReadLine();
        if (line1 == null && line2 == null)
        {
          break;
        }
        Assert.AreEqual(line1, line2);
        Assert.AreEqual(r1.Position, r2.Position, "Zeile " + lineNum);
      }
    }
  }
}
