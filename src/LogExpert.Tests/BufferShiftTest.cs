using System.Collections.Generic;
using System.Text;
using LogExpert.Classes.Log;
using LogExpert.Entities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LogExpert.Tests
{
    [TestFixture]
    internal class BufferShiftTest : RolloverHandlerTestBase
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
        public void TestShiftBuffers1()
        {
            int linesPerFile = 10;
            MultiFileOptions options = new MultiFileOptions();
            options.MaxDayTry = 0;
            options.FormatPattern = "*$J(.)";
            LinkedList<string> files = CreateTestFilesWithoutDate();
            EncodingOptions encodingOptions = new EncodingOptions();
            encodingOptions.Encoding = Encoding.Default;
            LogfileReader reader = new LogfileReader(files.Last.Value, encodingOptions, true, 40, 50, options);
            reader.ReadFiles();

            IList<ILogFileInfo> lil = reader.GetLogFileInfoList();
            ClassicAssert.AreEqual(files.Count, lil.Count);
            LinkedList<string>.Enumerator enumerator = files.GetEnumerator();
            enumerator.MoveNext();
            foreach (LogFileInfo li in lil)
            {
                string fileName = enumerator.Current;
                ClassicAssert.AreEqual(fileName, li.FullName);
                enumerator.MoveNext();
            }
            int oldCount = lil.Count;

            // Simulate rollover
            //
            files = RolloverSimulation(files, "*$J(.)", false);

            // Simulate rollover detection 
            //
            reader.ShiftBuffers();

            lil = reader.GetLogFileInfoList();
            ClassicAssert.AreEqual(oldCount + 1, lil.Count);

            ClassicAssert.AreEqual(linesPerFile * lil.Count, reader.LineCount);

            // Check if rollover'd file names have been handled by LogfileReader
            //
            ClassicAssert.AreEqual(files.Count, lil.Count);
            enumerator = files.GetEnumerator();
            enumerator.MoveNext();
            foreach (LogFileInfo li in lil)
            {
                string fileName = enumerator.Current;
                ClassicAssert.AreEqual(fileName, li.FullName);
                enumerator.MoveNext();
            }

            // Check if file buffers have correct files. Assuming here that one buffer fits for a 
            // complete file
            //
            enumerator = files.GetEnumerator();
            enumerator.MoveNext();
            IList<LogBuffer> logBuffers = reader.GetBufferList();
            int startLine = 0;
            foreach (LogBuffer logBuffer in logBuffers)
            {
                ClassicAssert.AreEqual(logBuffer.FileInfo.FullName, enumerator.Current);
                ClassicAssert.AreEqual(startLine, logBuffer.StartLine);
                startLine += 10;
                enumerator.MoveNext();
            }

            // Checking file content
            //
            enumerator = files.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext(); // move to 2nd entry. The first file now contains 2nd file's content (because rollover)
            logBuffers = reader.GetBufferList();
            int i;
            for (i = 0; i < logBuffers.Count - 2; ++i)
            {
                LogBuffer logBuffer = logBuffers[i];
                ILogLine line = logBuffer.GetLineOfBlock(0);
                ClassicAssert.IsTrue(line.FullLine.Contains(enumerator.Current));
                enumerator.MoveNext();
            }
            enumerator.MoveNext();
            // the last 2 files now contain the content of the previously watched file
            for (; i < logBuffers.Count; ++i)
            {
                LogBuffer logBuffer = logBuffers[i];
                ILogLine line = logBuffer.GetLineOfBlock(0);
                ClassicAssert.IsTrue(line.FullLine.Contains(enumerator.Current));
            }

            oldCount = lil.Count;

            // Simulate rollover again - now latest file will be deleted (simulates logger's rollover history limit)
            //
            files = RolloverSimulation(files, "*$J(.)", true);

            // Simulate rollover detection 
            //
            reader.ShiftBuffers();
            lil = reader.GetLogFileInfoList();

            ClassicAssert.AreEqual(oldCount, lil.Count); // same count because oldest file is deleted
            ClassicAssert.AreEqual(files.Count, lil.Count);
            ClassicAssert.AreEqual(linesPerFile * lil.Count, reader.LineCount);

            // Check first line to see if buffers are correct
            //
            ILogLine firstLine = reader.GetLogLine(0);
            string[] names = new string[files.Count];
            files.CopyTo(names, 0);
            ClassicAssert.IsTrue(firstLine.FullLine.Contains(names[2]));
        }
    }
}