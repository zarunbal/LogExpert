using LogExpert.Core.Classes.DateTimeParser;

using NUnit.Framework;

using System.Collections.Generic;

namespace LogExpert.Core.UnitTests.Classes.DateTimeParser;

[TestFixture]
public class SectionTests
{
    [Test]
    public void Constructor_Default_InitializesCorrectly()
    {
        var section = new Section();

        Assert.That(section.SectionIndex, Is.EqualTo(0));
        Assert.That(section.GeneralTextDateDurationParts, Is.Null);
    }

    [Test]
    public void SectionIndex_SetAndGet_WorksCorrectly()
    {
        var section = new Section();
        const int expectedIndex = 5;

        section.SectionIndex = expectedIndex;

        Assert.That(section.SectionIndex, Is.EqualTo(expectedIndex));
    }

    [Test]
    public void GeneralTextDateDurationParts_SetAndGet_WorksCorrectly()
    {
        var section = new Section();
        var parts = new List<string> { "yyyy", "MM", "dd" };

        section.GeneralTextDateDurationParts = parts;

        Assert.That(section.GeneralTextDateDurationParts, Is.EqualTo(parts));
        Assert.That(section.GeneralTextDateDurationParts, Is.Not.Null);
        Assert.That(section.GeneralTextDateDurationParts.Count, Is.EqualTo(3));
    }

    [Test]
    public void GeneralTextDateDurationParts_SetToNull_AllowsNullValue()
    {
        var section = new Section
        {
            GeneralTextDateDurationParts = new List<string> { "test" }
        };

        section.GeneralTextDateDurationParts = null;

        Assert.That(section.GeneralTextDateDurationParts, Is.Null);
    }

    [Test]
    public void Properties_IndependentlyModifiable_DoNotAffectEachOther()
    {
        var section = new Section();
        const int index = 10;
        var parts = new List<string> { "HH", "mm", "ss" };

        section.SectionIndex = index;
        section.GeneralTextDateDurationParts = parts;

        Assert.That(section.SectionIndex, Is.EqualTo(index));
        Assert.That(section.GeneralTextDateDurationParts, Is.EqualTo(parts));
    }
}