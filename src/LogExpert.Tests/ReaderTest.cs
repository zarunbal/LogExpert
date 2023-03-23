﻿using System.IO;
using System.Text;
using LogExpert.Classes.Log;
using LogExpert.Entities;
using LogExpert.Interface;
using NUnit.Framework;

namespace LogExpert.Tests
{
    [TestFixture]
    internal class ReaderTest
    {
        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeSetUp]
        public void Boot()
        {
        }

        private void CompareReaderImplementationsInternal(string fileName, Encoding enc, int maxPosition)
        {
            string path = "..\\..\\data\\";
            EncodingOptions encOpts = new EncodingOptions();
            encOpts.Encoding = enc;

            using (Stream s1 = new FileStream(path + fileName, FileMode.Open, FileAccess.Read))
                using (Stream s2 = new FileStream(path + fileName, FileMode.Open, FileAccess.Read))
                {
                    using (ILogStreamReader r1 = new PositionAwareStreamReaderLegacy(s1, encOpts))
                        using (ILogStreamReader r2 = new PositionAwareStreamReaderSystem(s2, encOpts))
                        {
                            for (int lineNum = 0;; lineNum++)
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
        [TestCase("50 MB.txt", "Windows-1252", 50000000)]
        [TestCase("50 MB UTF16.txt", "Unicode", 49999998)]
        [TestCase("50 MB UTF8.txt", "UTF-8", 50000000)]
        public void CompareReaderImplementations(string fileName, string encoding, int maxPosition)
        {
            CompareReaderImplementationsInternal(fileName, Encoding.GetEncoding(encoding), maxPosition);
        }
    }
}