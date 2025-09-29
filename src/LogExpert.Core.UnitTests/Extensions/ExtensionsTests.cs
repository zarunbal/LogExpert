using LogExpert.Core.Extensions;

using NUnit.Framework;

using System.Collections.Generic;

namespace LogExpert.Core.UnitTests.Extensions;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void IsEmpty_NullIEnumerable_ReturnsTrue()
    {
        IEnumerable<string> collection = null;

        Assert.That(collection.IsEmpty(), Is.True);
    }

    [Test]
    public void IsEmpty_EmptyIEnumerable_ReturnsTrue()
    {
        IEnumerable<string> collection = new List<string>();

        Assert.That(collection.IsEmpty(), Is.True);
    }

    [Test]
    public void IsEmpty_NonEmptyIEnumerable_ReturnsFalse()
    {
        IEnumerable<string> collection = new List<string> { "item" };

        Assert.That(collection.IsEmpty(), Is.False);
    }

    [Test]
    public void IsEmpty_NullIList_ReturnsTrue()
    {
        IList<string> list = null;

        Assert.That(list.IsEmpty(), Is.True);
    }

    [Test]
    public void IsEmpty_EmptyIList_ReturnsTrue()
    {
        IList<string> list = new List<string>();

        Assert.That(list.IsEmpty(), Is.True);
    }

    [Test]
    public void IsEmpty_NonEmptyIList_ReturnsFalse()
    {
        IList<string> list = new List<string> { "item" };

        Assert.That(list.IsEmpty(), Is.False);
    }
}