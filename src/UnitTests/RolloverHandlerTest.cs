using System;
using System.Collections.Generic;
using LogExpert;
using LogExpert.Classes.Log;
using LogExpert.Entities;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    internal class RolloverHandlerTest : RolloverHandlerTestBase
    {
        [Test]
        [TestCase("*$J(.)", 66)]
        [TestCase("*$D(YYYY-mm-DD)_$I.log", 3)]
        public void TestFilenameListWithAppendedIndex(string format, int retries)
        {
            MultiFileOptions options = new MultiFileOptions();
            options.FormatPattern = format;
            options.MaxDayTry = retries;

            LinkedList<string> files = CreateTestfilesWithoutDate();
            
            string firstFile = files.Last.Value;
            
            ILogFileInfo info = new LogFileInfo(new Uri(firstFile));
            RolloverFilenameHandler handler = new RolloverFilenameHandler(info, options);
            LinkedList<string> fileList = handler.GetNameList();

            Assert.AreEqual(files, fileList);
            
            Cleanup();
        }
    }
}