using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using LogExpert;
using System.IO;
using System.Windows.Forms;
using LogExpert.Classes.Log;
using LogExpert.Entities;
using LogExpert.Interface;

namespace UnitTests
{
    [TestFixture]
    internal class ReaderTest
    {
        [TearDown]
        public void TearDown()
        {
        }

        [TestFixtureSetUp]
        public void Boot()
        {
        }

        private void compareReaderImplementations(string fileName, Encoding enc, int maxPosition)
        {
            string DataPath = "..\\..\\data\\";
            EncodingOptions encOpts = new EncodingOptions();
            encOpts.Encoding = enc;

            using (Stream s1 = new FileStream(DataPath + fileName, FileMode.Open, FileAccess.Read))
            using (Stream s2 = new FileStream(DataPath + fileName, FileMode.Open, FileAccess.Read))
            {
                using (ILogStreamReader r1 = new PositionAwareStreamReaderLegacy(s1, encOpts))
                using (ILogStreamReader r2 = new PositionAwareStreamReaderSystem(s2, encOpts))
                {
                    for (int lineNum = 0; ; lineNum++)
                    {
                        string line1 = r1.ReadLine();
                        string line2 = r2.ReadLine();
                        if (line1 == null && line2 == null)
                        {
                            break;
                        }

                        Assert.AreEqual(line1, line2, "File " + fileName);
                        if (r1.Position != maxPosition)
                        {
                            Assert.AreEqual(r1.Position, r2.Position, "Line " + lineNum + ", File: " + fileName);
                        }
                        else
                        {
                            //Its desired that the position of the new implementation is 2 bytes ahead to fix the problem of empty lines every time a new line is appended.
                            Assert.AreEqual(r1.Position, r2.Position - 2, "Line " + lineNum + ", File: " + fileName);
                        }
                    }
                }
            }
        }

        [Test]
        public void compareReaderImplementations()
        {
            compareReaderImplementations("50 MB.txt", Encoding.Default, 50000000);
            compareReaderImplementations("50 MB UTF16.txt", Encoding.Unicode, 49999998);
            compareReaderImplementations("50 MB UTF8.txt", Encoding.UTF8, 50000000);
        }
    }
}