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
        Assert.IsTrue(info.IsEmpty);
        Assert.IsFalse(info.HasColumnizer);
        Assert.IsFalse(info.HasFileSystem);
        Assert.IsFalse(info.HasContextMenu);
        Assert.IsFalse(info.HasKeywordAction);
    }

    [Test]
    public void IsEmpty_WhenHasColumnizer_ReturnsFalse()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.IsFalse(info.IsEmpty);
    }

    [Test]
    public void IsSingleType_WhenOnlyColumnizer_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.IsTrue(info.IsSingleType);
        Assert.AreEqual(1, info.TypeCount);
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
        Assert.IsFalse(info.IsSingleType);
        Assert.IsTrue(info.IsMultiType);
        Assert.AreEqual(2, info.TypeCount);
    }

    [Test]
    public void IsColumnizerOnly_WhenOnlyColumnizer_ReturnsTrue()
    {
        // Arrange
        var info = new PluginTypeInfo { HasColumnizer = true };

        // Act & Assert
        Assert.IsTrue(info.IsColumnizerOnly);
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
        Assert.IsFalse(info.IsColumnizerOnly);
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
        Assert.AreEqual(4, info.TypeCount);
        Assert.IsFalse(info.IsSingleType);
        Assert.IsTrue(info.IsMultiType);
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
        Assert.IsTrue(info.IsMultiType);
        Assert.IsFalse(info.IsSingleType);
        Assert.IsFalse(info.IsEmpty);
    }
}
