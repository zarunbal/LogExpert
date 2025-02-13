using System;
using System.Collections.Generic;
using LogExpert.Classes.Log;
using LogExpert.Entities;
using NUnit.Framework;

namespace LogExpert.Tests
{
    [TestFixture]
    internal class RolloverHandlerTest : RolloverHandlerTestBase
    {
        [Test]
        [TestCase("*$J(.)", 66)]
        public void TestFilenameListWithAppendedIndex(string format, int retries)
        {
            MultiFileOptions options = new MultiFileOptions();
            options.FormatPattern = format;
            options.MaxDayTry = retries;

            LinkedList<string> files = CreateTestFilesWithoutDate();
            
            string firstFile = files.Last.Value;
            
            ILogFileInfo info = new LogFileInfo(new Uri(firstFile));
            RolloverFilenameHandler handler = new RolloverFilenameHandler(info, options);
            LinkedList<string> fileList = handler.GetNameList();

            Assert.That(fileList, Is.EqualTo(files));
            
            Cleanup();
        }
        
        [Test]
        [TestCase("*$D(YYYY-mm-DD)_$I.log", 3)]
        public void TestFilenameListWithDate(string format, int retries)
        {
            MultiFileOptions options = new MultiFileOptions();
            options.FormatPattern = format;
            options.MaxDayTry = retries;

            LinkedList<string> files = CreateTestFilesWithDate();
            
            string firstFile = files.Last.Value;
            
            ILogFileInfo info = new LogFileInfo(new Uri(firstFile));
            RolloverFilenameHandler handler = new RolloverFilenameHandler(info, options);
            LinkedList<string> fileList = handler.GetNameList();

            Assert.That(fileList, Is.EqualTo(files));
            
            Cleanup();
        }
    }
}