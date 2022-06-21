using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using LogExpert;
using System.IO;
using LogExpert.Classes;

namespace UnitTests
{
    [TestFixture]
    internal class LocalFileSystemTest : RolloverHandlerTestBase
    {
        [TearDown]
        public void TearDown()
        {
            Cleanup();
        }

        [TestFixtureSetUp]
        public void Boot()
        {
            Cleanup();
        }


        [Test]
        public void testUriHandle()
        {
            LocalFileSystem fs = new LocalFileSystem();
            Assert.True(fs.CanHandleUri("file:///c:/logfile.txt"));
            Assert.True(fs.CanHandleUri("file:///c:\\logfile.txt"));
            Assert.True(fs.CanHandleUri("c:/logfile.txt"));
            Assert.True(fs.CanHandleUri("c:\\logfile.txt"));
        }

        [Test]
        public void testUriToFileStream()
        {
            DirectoryInfo dInfo = Directory.CreateDirectory(RolloverHandlerTest.TEST_DIR_NAME);
            string fullName = CreateFile(dInfo, "test.log");

            LocalFileSystem fs = new LocalFileSystem();
            ILogFileInfo info = fs.GetLogfileInfo(fullName);
            Assert.True(info.Length > 0);
            Assert.True(info.OriginalLength == info.Length);
            Stream stream = info.OpenStream();
            Assert.True(stream.CanSeek);
            StreamReader reader = new StreamReader(stream);
            string line = reader.ReadLine();
            Assert.True(line.StartsWith("line number", StringComparison.InvariantCultureIgnoreCase));
            reader.Close();
        }
    }
}