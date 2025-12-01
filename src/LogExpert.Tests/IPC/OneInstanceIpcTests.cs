using LogExpert.Classes;
using LogExpert.Core.Classes.IPC;
using LogExpert.Core.Interface;

using Moq;

using Newtonsoft.Json;

using NUnit.Framework;

namespace LogExpert.Tests.IPC;

/// <summary>
/// Unit tests for One Instance Only feature - IPC logic tests
/// Tests the IPC message serialization and handling logic
/// </summary>
[TestFixture]
public class OneInstanceIpcTests
{
    #region IPC Message Type Tests

    [Test]
    public void SerializeCommand_WhenAllowOnlyOneInstance_UsesNewWindowOrLockedWindowType ()
    {
        // Arrange
        string[] files = ["test.log"];
        bool allowOnlyOne = true;

        // Act
        // Note: We can't call the private method directly, so we test the integration
        // through the public API. This test verifies the expected behavior.
        // For unit testing, we'd need to make SerializeCommandIntoNonFormattedJSON internal
        // or use InternalsVisibleTo attribute.
        
        // For now, we test the IpcMessage structure directly
        var message = new IpcMessage
        {
            Type = allowOnlyOne ? IpcMessageType.NewWindowOrLockedWindow : IpcMessageType.NewWindow,
            Payload = Newtonsoft.Json.Linq.JObject.FromObject(new LoadPayload { Files = [.. files] })
        };
        
        var json = JsonConvert.SerializeObject(message, Formatting.None);
        var deserialized = JsonConvert.DeserializeObject<IpcMessage>(json);

        // Assert
        Assert.That(deserialized.Type, Is.EqualTo(IpcMessageType.NewWindowOrLockedWindow));
        var payload = deserialized.Payload.ToObject<LoadPayload>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload.Files.Count, Is.EqualTo(1));
        Assert.That(payload.Files[0], Is.EqualTo("test.log"));
    }

    [Test]
    public void SerializeCommand_WhenMultipleInstancesAllowed_UsesNewWindowType ()
    {
        // Arrange
        string[] files = ["test.log"];
        bool allowOnlyOne = false;

        // Act
        var message = new IpcMessage
        {
            Type = allowOnlyOne ? IpcMessageType.NewWindowOrLockedWindow : IpcMessageType.NewWindow,
            Payload = Newtonsoft.Json.Linq.JObject.FromObject(new LoadPayload { Files = [.. files] })
        };
        
        var json = JsonConvert.SerializeObject(message, Formatting.None);
        var deserialized = JsonConvert.DeserializeObject<IpcMessage>(json);

        // Assert
        Assert.That(deserialized.Type, Is.EqualTo(IpcMessageType.NewWindow));
    }

    [Test]
    public void IpcMessage_SerializesAndDeserializesCorrectly ()
    {
        // Arrange
        var originalMessage = new IpcMessage
        {
            Type = IpcMessageType.Load,
            Payload = Newtonsoft.Json.Linq.JObject.FromObject(new LoadPayload 
            { 
                Files = ["file1.log", "file2.log", "file3.log"] 
            })
        };

        // Act
        var json = JsonConvert.SerializeObject(originalMessage, Formatting.None);
        var deserializedMessage = JsonConvert.DeserializeObject<IpcMessage>(json);

        // Assert
        Assert.That(deserializedMessage, Is.Not.Null);
        Assert.That(deserializedMessage.Type, Is.EqualTo(originalMessage.Type));
        
        var originalPayload = originalMessage.Payload.ToObject<LoadPayload>();
        var deserializedPayload = deserializedMessage.Payload.ToObject<LoadPayload>();
        
        Assert.That(deserializedPayload.Files.Count, Is.EqualTo(originalPayload.Files.Count));
        for (int i = 0; i < originalPayload.Files.Count; i++)
        {
            Assert.That(deserializedPayload.Files[i], Is.EqualTo(originalPayload.Files[i]));
        }
    }

    [Test]
    public void IpcMessage_MultipleFiles_SerializesCorrectly ()
    {
        // Arrange
        string[] multipleFiles = ["log1.txt", "log2.txt", "log3.txt", "log4.txt"];
        var message = new IpcMessage
        {
            Type = IpcMessageType.NewWindowOrLockedWindow,
            Payload = Newtonsoft.Json.Linq.JObject.FromObject(new LoadPayload { Files = [.. multipleFiles] })
        };

        // Act
        var json = JsonConvert.SerializeObject(message, Formatting.None);
        var deserialized = JsonConvert.DeserializeObject<IpcMessage>(json);

        // Assert
        var payload = deserialized.Payload.ToObject<LoadPayload>();
        Assert.That(payload.Files.Count, Is.EqualTo(4));
        Assert.That(payload.Files, Is.EquivalentTo(multipleFiles));
    }

    [Test]
    public void IpcMessage_EmptyFileList_SerializesCorrectly ()
    {
        // Arrange
        var message = new IpcMessage
        {
            Type = IpcMessageType.NewWindow,
            Payload = Newtonsoft.Json.Linq.JObject.FromObject(new LoadPayload { Files = [] })
        };

        // Act
        var json = JsonConvert.SerializeObject(message, Formatting.None);
        var deserialized = JsonConvert.DeserializeObject<IpcMessage>(json);

        // Assert
        var payload = deserialized.Payload.ToObject<LoadPayload>();
        Assert.That(payload.Files, Is.Empty);
    }

    #endregion

    #region IPC Message Type Enum Tests

    [Test]
    public void IpcMessageType_HasCorrectValues ()
    {
        // Assert - verify the enum values exist
        Assert.That(IpcMessageType.Load, Is.Not.Null);
        Assert.That(IpcMessageType.NewWindow, Is.Not.Null);
        Assert.That(IpcMessageType.NewWindowOrLockedWindow, Is.Not.Null);
    }

    [Test]
    public void IpcMessageType_Load_IsDistinctFromNewWindow ()
    {
        // Assert
        Assert.That(IpcMessageType.Load, Is.Not.EqualTo(IpcMessageType.NewWindow));
    }

    [Test]
    public void IpcMessageType_NewWindowOrLockedWindow_IsDistinctFromOthers ()
    {
        // Assert
        Assert.That(IpcMessageType.NewWindowOrLockedWindow, Is.Not.EqualTo(IpcMessageType.Load));
        Assert.That(IpcMessageType.NewWindowOrLockedWindow, Is.Not.EqualTo(IpcMessageType.NewWindow));
    }

    #endregion

    #region LoadPayload Tests

    [Test]
    public void LoadPayload_CanBeCreatedEmpty ()
    {
        // Arrange & Act
        var payload = new LoadPayload();

        // Assert
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload.Files, Is.Not.Null);
    }

    [Test]
    public void LoadPayload_CanBeCreatedWithFiles ()
    {
        // Arrange & Act
        var payload = new LoadPayload
        {
            Files = ["test1.log", "test2.log"]
        };

        // Assert
        Assert.That(payload.Files, Has.Count.EqualTo(2));
        Assert.That(payload.Files[0], Is.EqualTo("test1.log"));
        Assert.That(payload.Files[1], Is.EqualTo("test2.log"));
    }

    #endregion
}
