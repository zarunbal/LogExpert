using CsvColumnizerType = CsvColumnizer.CsvColumnizer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace LogExpert.Tests
{
    [TestFixture]
    public class LogWindowTest
    {
        // TODO: Add more tests when DI container is ready.
        [TestCase(@".\TestData\JsonColumnizerTest_01.txt", typeof(DefaultLogfileColumnizer))]
        public void Instantiate_JsonFile_BuildCorrectColumnizer(string fileName, Type columnizerType)
        {
            LogWindow logWindow = Create(fileName);

            Assert.AreEqual(columnizerType, logWindow.CurrentColumnizer.GetType());
        }

        [TestCase(@".\TestData\XmlTest_01.xml")]
        [TestCase(@".\TestData\CsvTest_01.csv")]
        public void Instantiate_AnyFile_NotCrash(string fileName)
        {
            PluginRegistry.GetInstance().RegisteredColumnizers.Add(new Log4jXmlColumnizer());
            PluginRegistry.GetInstance().RegisteredColumnizers.Add(new CsvColumnizerType());

            LogWindow logWindow = Create(fileName);

            Assert.True(true);
        }

        private LogWindow Create(string fileName)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<LogTabWindow>();
            builder.RegisterType<LogWindow>();

            IContainer rootContainer = builder.Build();
            ILifetimeScope scope = rootContainer.BeginLifetimeScope();

            LogTabWindow logTabWindow = scope.Resolve<LogTabWindow>(new List<Parameter>(
                new[]
                {
                    new PositionalParameter(0, null),
                    new PositionalParameter(1, 0),
                    new PositionalParameter(2, false)
                }));

            LogWindow logWindow = scope.Resolve<LogWindow>(new List<Parameter>(
                new[]
                {
                    new PositionalParameter(0, logTabWindow),
                    new PositionalParameter(1, fileName),
                    new PositionalParameter(2, false),
                    new PositionalParameter(3, false)
                }));

            return logWindow;
        }
    }
}