using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using LogExpert.Classes.Log;


namespace LogExpert
{
    [TestFixture]
    internal class RollingNameTest
    {
        [Test]
        public void testFilename1()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*$D(yyyy-MM-dd)_$I.log");
            fnb.SetFileName("engine_2010-06-12_0.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12_0.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12_1.log", name);
        }

        [Test]
        public void testFilename2()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*$D(yyyy-MM-dd).log");
            fnb.SetFileName("engine_2010-06-12.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12.log", name);
        }

        [Test]
        public void testFilename3()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*_$I.log");
            fnb.SetFileName("engine_0.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine_0.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine_1.log", name);
        }

        [Test]
        public void testFilenameHiddenZero()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*.log$J(.)");
            fnb.SetFileName("engine.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine.log.1", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine.log.2", name);
        }

        [Test]
        public void testFilenameHiddenZero2()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("engine$J.log");
            fnb.SetFileName("engine.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine1.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine2.log", name);
        }

        [Test]
        public void testFilenameHiddenZero3()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("engine$J.log");
            fnb.SetFileName("engine1.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine1.log", name);
            fnb.Index = fnb.Index - 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine.log", name);
        }

        [Test]
        public void testFilenameHiddenZero4()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*$J(.)");
            fnb.SetFileName("engine.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine.log.1", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine.log.2", name);
        }

        [Test]
        public void testFilenameWithDateAndHiddenZero()
        {
            RolloverFilenameBuilder fnb = new RolloverFilenameBuilder("*$D(yyyy-MM-dd).log$J(.)");
            fnb.SetFileName("engine_2010-06-12.log");
            string name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12.log", name);
            fnb.Index = fnb.Index + 1;
            name = fnb.BuildFileName();
            Assert.AreEqual("engine_2010-06-12.log.1", name);
        }
    }
}