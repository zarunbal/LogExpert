using System;
using System.Text;
using LogExpert;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

            ClassicAssert.AreEqual(expected, column.DisplayValue);
            ClassicAssert.AreEqual(builder.ToString(), column.FullValue);
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

            ClassicAssert.AreEqual(expected, column.DisplayValue);
            ClassicAssert.AreEqual(expected, column.FullValue);
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
                ClassicAssert.AreEqual("asdf␀", column.DisplayValue);
            }
            else
            {
                ClassicAssert.AreEqual("asdf ", column.DisplayValue);
            }

            ClassicAssert.AreEqual("asdf\0", column.FullValue);
        }

        [Test]
        public void Column_TabReplacement()
        {
            Column column = new();

            column.FullValue = "asdf\t";

            ClassicAssert.AreEqual("asdf  ", column.DisplayValue);
            ClassicAssert.AreEqual("asdf\t", column.FullValue);
        }
    }
}