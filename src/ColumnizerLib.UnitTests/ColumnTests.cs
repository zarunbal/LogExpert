using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogExpert;
using NUnit.Framework;

namespace ColumnizerLib.UnitTests
{
    [TestFixture]
    public class ColumnTests
    {
        [Test]
        public void Column_LineCutOf()
        {
            Column column = new Column();

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < 4675; i++)
            {
                builder.Append("6");
            }

            string expected = builder + "...";
            builder.Append("1234");

            column.FullValue = builder.ToString();

            Assert.AreEqual(expected, column.DisplayValue);
            Assert.AreEqual(builder.ToString(), column.FullValue);
        }

        [Test]
        public void Column_NoLineCutOf()
        {
            Column column = new Column();

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < 4675; i++)
            {
                builder.Append("6");
            }

            string expected = builder.ToString();

            column.FullValue = expected;

            Assert.AreEqual(expected, column.DisplayValue);
            Assert.AreEqual(expected, column.FullValue);
        }

        [Test]
        public void Column_NullCharReplacement()
        {
            Column column = new Column();

            column.FullValue = "asdf\0";

            //Switch between the different implementation for the windows versions
            //Not that great solution but currently I'm out of ideas, I know that currently 
            //only one implementation depending on the windows version is executed
            if (Environment.Version >= Version.Parse("6.2"))
            {
                Assert.AreEqual("asdf␀", column.DisplayValue);
            }
            else
            {
                Assert.AreEqual("asdf ", column.DisplayValue);
            }

            Assert.AreEqual("asdf\0", column.FullValue);
        }

        [Test]
        public void Column_TabReplacement()
        {
            Column column = new Column();

            column.FullValue = "asdf\t";

            Assert.AreEqual("asdf  ", column.DisplayValue);
            Assert.AreEqual("asdf\t", column.FullValue);
        }
    }
}