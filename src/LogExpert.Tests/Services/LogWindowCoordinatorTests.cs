using System.Runtime.Versioning;

using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Services.LogWindowCoordinatorService;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests.Services;

[TestFixture]
[Apartment(ApartmentState.STA)]
[SupportedOSPlatform("windows")]
public class LogWindowCoordinatorTests
{
    private Mock<IConfigManager> _configManagerMock;
    private LogWindowCoordinator _coordinator;
    private Settings _settings;
    private Preferences _preferences;

    [SetUp]
    public void Setup ()
    {
        _configManagerMock = new Mock<IConfigManager>();
        _settings = new Settings();
        _preferences = _settings.Preferences;
        _ = _configManagerMock.Setup(cm => cm.Settings).Returns(_settings);

        _coordinator = new LogWindowCoordinator(_configManagerMock.Object);
    }

    [Test]
    public void ResolveHighlightGroup_WithGroupName_ReturnsNameMatch ()
    {
        // Arrange
        var group = new HighlightGroup { GroupName = "MyGroup" };
        _coordinator.UpdateHighlightGroups([group]);

        // Act
        var result = _coordinator.ResolveHighlightGroup("MyGroup", null);

        // Assert
        Assert.That(result, Is.SameAs(group));
    }

    [Test]
    public void ResolveHighlightGroup_WithFileName_ReturnsFileMaskMatch ()
    {
        // Arrange
        var group = new HighlightGroup { GroupName = "LogGroup" };
        _coordinator.UpdateHighlightGroups([group]);
        _preferences.HighlightMaskList.Add(new HighlightMaskEntry { Mask = @"\.log$", HighlightGroupName = "LogGroup" });

        // Act
        var result = _coordinator.ResolveHighlightGroup(null, "test.log");

        // Assert
        Assert.That(result, Is.SameAs(group));
    }

    [Test]
    public void ResolveHighlightGroup_FileMaskTakesPriority_WhenBothProvided ()
    {
        // Arrange
        var maskGroup = new HighlightGroup { GroupName = "MaskGroup" };
        var nameGroup = new HighlightGroup { GroupName = "NameGroup" };
        _coordinator.UpdateHighlightGroups([maskGroup, nameGroup]);
        _preferences.HighlightMaskList.Add(new HighlightMaskEntry { Mask = @"\.log$", HighlightGroupName = "MaskGroup" });

        // Act
        var result = _coordinator.ResolveHighlightGroup("NameGroup", "test.log");

        // Assert
        Assert.That(result, Is.SameAs(maskGroup));
    }

    [Test]
    public void ResolveHighlightGroup_FallsBackToName_WhenFileMaskNoMatch ()
    {
        // Arrange
        var group = new HighlightGroup { GroupName = "NameGroup" };
        _coordinator.UpdateHighlightGroups([group]);
        _preferences.HighlightMaskList.Add(new HighlightMaskEntry { Mask = @"\.xml$", HighlightGroupName = "OtherGroup" });

        // Act
        var result = _coordinator.ResolveHighlightGroup("NameGroup", "test.log");

        // Assert
        Assert.That(result, Is.SameAs(group));
    }

    [Test]
    public void ResolveHighlightGroup_NoMatch_ReturnsFirstGroup ()
    {
        // Arrange
        var firstGroup = new HighlightGroup { GroupName = "First" };
        var secondGroup = new HighlightGroup { GroupName = "Second" };
        _coordinator.UpdateHighlightGroups([firstGroup, secondGroup]);

        // Act
        var result = _coordinator.ResolveHighlightGroup("NonExistent", null);

        // Assert
        Assert.That(result, Is.SameAs(firstGroup));
    }

    [Test]
    public void ResolveHighlightGroup_EmptyList_ReturnsNewEmptyGroup ()
    {
        // Arrange
        _coordinator.UpdateHighlightGroups([]);

        // Act
        var result = _coordinator.ResolveHighlightGroup("NonExistent", null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.GroupName, Is.Not.Null);
    }

    [Test]
    public void ResolveHighlightGroup_NeverReturnsNull ()
    {
        // Arrange
        _coordinator.UpdateHighlightGroups([]);

        // Act
        var result = _coordinator.ResolveHighlightGroup(null, null);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ResolveHighlightGroup_MalformedRegex_SkipsAndContinues ()
    {
        // Arrange
        var group = new HighlightGroup { GroupName = "GoodGroup" };
        _coordinator.UpdateHighlightGroups([group]);
        _preferences.HighlightMaskList.Add(new HighlightMaskEntry { Mask = @"[invalid", HighlightGroupName = "BadGroup" });
        _preferences.HighlightMaskList.Add(new HighlightMaskEntry { Mask = @"\.log$", HighlightGroupName = "GoodGroup" });

        // Act
        var result = _coordinator.ResolveHighlightGroup(null, "test.log");

        // Assert
        Assert.That(result, Is.SameAs(group));
    }

    [Test]
    public void HighlightSettingsChanged_FiresAfterMutation ()
    {
        // Arrange
        var eventFired = false;
        _coordinator.HighlightSettingsChanged += (_, _) => eventFired = true;

        // Act
        _coordinator.OnHighlightSettingsChanged();

        // Assert
        Assert.That(eventFired, Is.True);
    }
}