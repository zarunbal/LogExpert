using System;
using System.IO;
using LogExpert.Classes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LogExpert.Tests
{
    [TestFixture]
    internal class LocalFileSystemTest : RolloverHandlerTestBase
    {
        [TearDown]
        public void TearDown()
        {
            Cleanup();
        }

        [OneTimeSetUp]
        public void Boot()
        {
            Cleanup();
        }


        [Test]
        public void TestUriHandle()
        {
            LocalFileSystem fs = new LocalFileSystem();
            ClassicAssert.True(fs.CanHandleUri("file:///c:/logfile.txt"));
            ClassicAssert.True(fs.CanHandleUri("file:///c:\\logfile.txt"));
            ClassicAssert.True(fs.CanHandleUri("c:/logfile.txt"));
            ClassicAssert.True(fs.CanHandleUri("c:\\logfile.txt"));
        }

        [Test]
        public void TestUriToFileStream()
        {
            DirectoryInfo dInfo = Directory.CreateDirectory(RolloverHandlerTest.TEST_DIR_NAME);
            string fullName = CreateFile(dInfo, "test.log");

            LocalFileSystem fs = new LocalFileSystem();
            ILogFileInfo info = fs.GetLogfileInfo(fullName);
            ClassicAssert.True(info.Length > 0);
            ClassicAssert.True(info.OriginalLength == info.Length);
            Stream stream = info.OpenStream();
            ClassicAssert.True(stream.CanSeek);
            StreamReader reader = new StreamReader(stream);
            string line = reader.ReadLine();
            ClassicAssert.True(line.StartsWith("line number", StringComparison.InvariantCultureIgnoreCase));
            reader.Close();
        }
    }
}