
using CsvColumnizerType=CsvColumnizer.CsvColumnizer;
using NUnit.Framework;
using System;
using LogExpert.Classes;
using LogExpert.Controls.LogTabWindow;
using LogExpert.Controls.LogWindow;
using LogExpert.Entities;

namespace LogExpert.Tests
{
    [TestFixture]
    public class LogWindowTest
    {
        // TODO: Add more tests when DI container is ready.
        [TestCase(@".\TestData\JsonColumnizerTest_01.txt", typeof(DefaultLogfileColumnizer))]
        public void Instantiate_JsonFile_BuildCorrectColumnizer(string fileName, Type columnizerType)
        {
            LogTabWindow logTabWindow = new LogTabWindow(null, 0, false);
            LogWindow logWindow =
                new LogWindow(logTabWindow, fileName, false, false);

            Assert.AreEqual(columnizerType, logWindow.CurrentColumnizer.GetType());
        }

        [TestCase(@".\TestData\XmlTest_01.xml")]
        [TestCase(@".\TestData\CsvTest_01.csv")]
        public void Instantiate_AnyFile_NotCrash(string fileName)
        {
            PluginRegistry.GetInstance().RegisteredColumnizers.Add(new Log4jXmlColumnizer());
            PluginRegistry.GetInstance().RegisteredColumnizers.Add(new CsvColumnizerType());

            LogTabWindow logTabWindow = new LogTabWindow(null, 0, false);
            LogWindow logWindow = new LogWindow(logTabWindow, fileName, false, false);

            Assert.True(true);
        }
    }
}
