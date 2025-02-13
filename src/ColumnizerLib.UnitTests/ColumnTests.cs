using LogExpert;
using NUnit.Framework;
using System;
using System.Text;

namespace ColumnizerLib.UnitTests
{
    [TestFixture]
    public class ColumnTests
    {
        [Test]
        public void Column_LineCutOf()
        {
            Column column = new();

            StringBuilder builder = new();

            for (int i = 0; i < 4675; i++)
            {
                builder.Append("6");
            }

            string expected = builder + "...";
            builder.Append("1234");

            column.FullValue = builder.ToString();

            Assert.That(column.DisplayValue, Is.EqualTo(expected));
            Assert.That(column.FullValue, Is.EqualTo(builder.ToString()));
        }

        [Test]
        public void Column_NoLineCutOf()
        {
            Column column = new();

            StringBuilder builder = new();

            for (int i = 0; i < 4675; i++)
            {
                builder.Append("6");
            }

            string expected = builder.ToString();

            column.FullValue = expected;

            Assert.That(column.DisplayValue, Is.EqualTo(expected));
            Assert.That(column.FullValue, Is.EqualTo(expected));
        }

        [Test]
        public void Column_NullCharReplacement()
        {
            Column column = new();

            column.FullValue = "asdf\0";

            //Switch between the different implementation for the windows versions
            //Not that great solution but currently I'm out of ideas, I know that currently 
            //only one implementation depending on the windows version is executed
            if (Environment.Version >= Version.Parse("6.2"))
            {
                Assert.That(column.DisplayValue, Is.EqualTo("asdf␀"));
            }
            else
            {
                Assert.That(column.DisplayValue, Is.EqualTo("asdf "));
            }

            Assert.That(column.FullValue, Is.EqualTo("asdf\0"));
        }

        [Test]
        public void Column_TabReplacement()
        {
            Column column = new();

            column.FullValue = "asdf\t";

            Assert.That(column.DisplayValue, Is.EqualTo("asdf  "));
            Assert.That(column.FullValue, Is.EqualTo("asdf\t"));
        }
    }
}