using System.Text;

using ColumnizerLib;

using NUnit.Framework;

namespace LogExpert.ColumnizerLib.Tests;

[TestFixture]
public class ColumnTests
{
    [SetUp]
    public void SetUp ()
    {
        // Reset to default before each test
        Column.SetMaxDisplayLength(20_000);
    }

    [Test]
    public void Column_DisplayMaxLength_DefaultIs20000 ()
    {
        Assert.That(Column.GetMaxDisplayLength(), Is.EqualTo(20_000));
    }

    [Test]
    public void Column_DisplayMaxLength_CanBeConfigured ()
    {
        Column.SetMaxDisplayLength(50_000);
        Assert.That(Column.GetMaxDisplayLength(), Is.EqualTo(50_000));

        // Reset for other tests
        Column.SetMaxDisplayLength(20_000);
    }

    [Test]
    public void Column_DisplayMaxLength_EnforcesMinimum ()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => Column.SetMaxDisplayLength(500));
    }

    [Test]
    public void Column_TruncatesAtConfiguredDisplayLength ()
    {
        Column.SetMaxDisplayLength(10_000);

        // Create a line longer than the display max length
        var longValue = new StringBuilder().Append('X', 15_000).ToString();

        Column column = new()
        {
            FullValue = longValue
        };

        // FullValue should contain the full line
        Assert.That(column.FullValue, Is.EqualTo(longValue));
        Assert.That(column.FullValue.Length, Is.EqualTo(15_000));

        // DisplayValue should be truncated at 10,000 with "..." appended
        Assert.That(column.DisplayValue.Length, Is.EqualTo(10_003)); // 10000 + "..."
        Assert.That(column.DisplayValue.EndsWith("...", StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(column.DisplayValue.StartsWith("XXX", StringComparison.OrdinalIgnoreCase), Is.True);

        // Reset for other tests
        Column.SetMaxDisplayLength(20_000);
    }

    [Test]
    public void Column_NoTruncationWhenBelowLimit ()
    {
        Column.SetMaxDisplayLength(20_000);

        var normalValue = new StringBuilder().Append('Y', 5_000).ToString();
        Column column = new()
        {
            FullValue = normalValue
        };

        Assert.That(column.DisplayValue, Is.EqualTo(column.FullValue));
        Assert.That(column.DisplayValue.Length, Is.EqualTo(5_000));
    }

    [Test]
    public void Column_NullCharReplacement ()
    {
        Column column = new()
        {
            FullValue = "asdf\0"
        };

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
    public void Column_TabReplacement ()
    {
        Column column = new()
        {
            FullValue = "asdf\t"
        };

        Assert.That(column.DisplayValue, Is.EqualTo("asdf  "));
        Assert.That(column.FullValue, Is.EqualTo("asdf\t"));
    }
}