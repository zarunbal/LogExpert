using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;


namespace LogExpert
{
    [TestFixture]
    internal class RolloverHandlerTest : RolloverHandlerTestBase
    {
        [Test]
        public void testFilenameListWithAppendedIndex()
        {
            MultiFileOptions options = new MultiFileOptions();
            options.FormatPattern = "*$J(.)";
            options.MaxDayTry = 66;
            LinkedList<string> files = CreateTestfilesWithoutDate();
            string firstFile = files.Last.Value;
            ILogFileInfo info = new LogFileInfo(new Uri(firstFile));
            RolloverFilenameHandler handler = new RolloverFilenameHandler(info, options);
            LinkedList<string> fileList = handler.GetNameList();
            Assert.AreEqual(files, fileList);
            Cleanup();
        }

        [Test]
        public void testFilenameListWithDate()
        {
            MultiFileOptions options = new MultiFileOptions();
            options.FormatPattern = "*$D(YYYY-mm-DD)_$I.log";
            options.MaxDayTry = 3;
            LinkedList<string> files = CreateTestfilesWithDate();
            string firstFile = files.Last.Value;
            ILogFileInfo info = new LogFileInfo(new Uri(firstFile));
            RolloverFilenameHandler handler = new RolloverFilenameHandler(info, options);
            LinkedList<string> fileList = handler.GetNameList();
            Assert.AreEqual(files, fileList);
            Cleanup();
        }
    }
}