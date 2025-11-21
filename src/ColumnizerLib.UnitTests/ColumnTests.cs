using LogExpert;

using NUnit.Framework;

using System;
using System.Text;

namespace ColumnizerLib.UnitTests;

[TestFixture]
public class ColumnTests
{
    [Test]
    public void Column_NoTruncationAtDisplayLevel ()
    {
        // Line truncation is now handled at the reader level (PositionAwareStreamReader)
        // based on the configurable MaxLineLength setting. The Column class no longer
        // truncates DisplayValue, allowing full lines to be displayed and copied.
        var longValue = new StringBuilder().Append('6', 10000).ToString();

        Column column = new()
        {
            FullValue = longValue
        };

        // DisplayValue should equal FullValue (no truncation at display level)
        Assert.That(column.DisplayValue, Is.EqualTo(column.FullValue));
        Assert.That(column.DisplayValue.Length, Is.EqualTo(10000));
    }

    [Test]
    public void Column_ShortLine ()
    {
        var expected = new StringBuilder().Append('6', 100).ToString();
        Column column = new()
        {
            FullValue = expected
        };

        Assert.That(column.DisplayValue, Is.EqualTo(column.FullValue));
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