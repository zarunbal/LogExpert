using NUnit.Framework;
using LogExpert.PluginRegistry;

namespace LogExpert.PluginRegistry.Tests;

[TestFixture]
public class PluginTypeInfoTests
{
    [Test]
    public void IsEmpty_WhenNoPluginTypes_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo();

        // Act & Assert
        Assert.That(info.IsEmpty, Is.True);
        Assert.That(info.HasColumnizer, Is.False);
        Assert.That(info.HasFileSystem, Is.False);
        Assert.That(info.HasContextMenu, Is.False);
        Assert.That(info.HasKeywordAction, Is.False);
    }

    [Test]
    public void IsEmpty_WhenHasColumnizer_ReturnsFalse()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.That(info.IsEmpty, Is.False);
    }

    [Test]
    public void IsSingleType_WhenOnlyColumnizer_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.That(info.IsSingleType, Is.True);
        Assert.That(info.TypeCount, Is.EqualTo(1));
    }

    [Test]
    public void IsSingleType_WhenMultipleTypes_ReturnsFalse()
    {
        // Arrange
        var info = new PluginTypeInfo
        {
            HasColumnizer = true,
            HasFileSystem = true
        };

        // Act & Assert
        Assert.That(info.IsSingleType, Is.False);
        Assert.That(info.IsMultiType, Is.True);
        Assert.That(info.TypeCount, Is.EqualTo(2));
    }

    [Test]
    public void IsColumnizerOnly_WhenOnlyColumnizer_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.That(info.IsColumnizerOnly, Is.True);
    }

    [Test]
    public void IsColumnizerOnly_WhenColumnizerAndOthers_ReturnsFalse()
    {
        // Arrange
        var info = new PluginTypeInfo
        {
            HasColumnizer = true,
            HasFileSystem = true
        };

        // Act & Assert
        Assert.That(info.IsColumnizerOnly, Is.False);
    }

    [Test]
    public void TypeCount_WhenAllTypes_ReturnsFour()
    {
        // Arrange
        var info = new PluginTypeInfo
        {
            HasColumnizer = true,
            HasFileSystem = true,
            HasContextMenu = true,
            HasKeywordAction = true
        };

        // Act & Assert
        Assert.That(info.TypeCount, Is.EqualTo(4));
        Assert.That(info.IsSingleType, Is.False);
        Assert.That(info.IsMultiType, Is.True);
    }

    [Test]
    public void IsMultiType_WhenTwoTypes_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo
        {
            HasColumnizer = true,
            HasFileSystem = true
        };

        // Act & Assert
        Assert.That(info.IsMultiType, Is.True);
        Assert.That(info.IsSingleType, Is.False);
        Assert.That(info.IsEmpty, Is.False);
    }
}
