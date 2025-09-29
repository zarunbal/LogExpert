using LogExpert.Core.Config;

using NUnit.Framework;

namespace LogExpert.Core.UnitTests.Config;

[TestFixture]
public class ToolEntryTests
{
    [Test]
    public void Constructor_DefaultValues_InitializesCorrectly()
    {
        var entry = new ToolEntry();

        Assert.That(entry.Args, Is.EqualTo(string.Empty));
        Assert.That(entry.Cmd, Is.EqualTo(string.Empty));
        Assert.That(entry.ColumnizerName, Is.EqualTo(string.Empty));
        Assert.That(entry.IconFile, Is.Null);
        Assert.That(entry.IconIndex, Is.EqualTo(0));
        Assert.That(entry.IsFavourite, Is.False);
        Assert.That(entry.Name, Is.Null);
        Assert.That(entry.Sysout, Is.False);
        Assert.That(entry.WorkingDir, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToString_WithName_ReturnsName()
    {
        var entry = new ToolEntry { Name = "Test Tool", Cmd = "test.exe" };

        var result = entry.ToString();

        Assert.That(result, Is.EqualTo("Test Tool"));
    }

    [Test]
    public void ToString_WithoutName_ReturnsCmd()
    {
        var entry = new ToolEntry { Cmd = "test.exe" };

        var result = entry.ToString();

        Assert.That(result, Is.EqualTo("test.exe"));
    }

    [Test]
    public void ToString_NullName_ReturnsCmd()
    {
        var entry = new ToolEntry { Name = null, Cmd = "test.exe" };

        var result = entry.ToString();

        Assert.That(result, Is.EqualTo("test.exe"));
    }

    [Test]
    public void Clone_CreatesDeepCopy()
    {
        var original = new ToolEntry
        {
            Args = "arg1 arg2",
            Cmd = "test.exe",
            ColumnizerName = "TestColumnizer",
            IconFile = "icon.ico",
            IconIndex = 1,
            IsFavourite = true,
            Name = "Test Tool",
            Sysout = true,
            WorkingDir = @"C:\test"
        };

        var clone = original.Clone();

        Assert.That(clone, Is.Not.SameAs(original));
        Assert.That(clone.Args, Is.EqualTo(original.Args));
        Assert.That(clone.Cmd, Is.EqualTo(original.Cmd));
        Assert.That(clone.ColumnizerName, Is.EqualTo(original.ColumnizerName));
        Assert.That(clone.IconFile, Is.EqualTo(original.IconFile));
        Assert.That(clone.IconIndex, Is.EqualTo(original.IconIndex));
        Assert.That(clone.IsFavourite, Is.EqualTo(original.IsFavourite));
        Assert.That(clone.Name, Is.EqualTo(original.Name));
        Assert.That(clone.Sysout, Is.EqualTo(original.Sysout));
        Assert.That(clone.WorkingDir, Is.EqualTo(original.WorkingDir));
    }

    [Test]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new ToolEntry { Name = "Original", Cmd = "original.exe" };
        var clone = original.Clone();

        clone.Name = "Modified";
        clone.Cmd = "modified.exe";

        Assert.That(original.Name, Is.EqualTo("Original"));
        Assert.That(original.Cmd, Is.EqualTo("original.exe"));
        Assert.That(clone.Name, Is.EqualTo("Modified"));
        Assert.That(clone.Cmd, Is.EqualTo("modified.exe"));
    }
}