using LogExpert.Core.Classes.Attributes;
using LogExpert.Core.Classes.JsonConverters;

using Newtonsoft.Json;

using NUnit.Framework;

namespace LogExpert.Tests;

public class MockColumnizer : ILogLineColumnizer
{
    [JsonColumnizerProperty]
    public int IntProperty { get; set; }

    [JsonColumnizerProperty]
    public string StringProperty { get; set; }

    public string GetName () => "MockColumnizer";

    public string GetDescription () => "Test columnizer";

    public int GetColumnCount () => 1;

    public string GetColumnName (int column) => "Col";

    public string GetColumnValue (LogExpert.ILogLine line, int column) => "";

    public bool IsTimeshiftImplemented () => false;

    public void PushValue (LogExpert.ILogLine line, int column, string value) { }

    public void SetColumnNames (string[] names) { }

    public void SetParameters (string param) { }

    public void SetConfig (object config) { }

    public string[] GetColumnNames () => throw new NotImplementedException();

    public IColumnizedLogLine SplitLine (ILogLineColumnizerCallback callback, ILogLine line) => throw new NotImplementedException();

    public void SetTimeOffset (int msecOffset) => throw new NotImplementedException();

    public int GetTimeOffset () => throw new NotImplementedException();

    public DateTime GetTimestamp (ILogLineColumnizerCallback callback, ILogLine line) => throw new NotImplementedException();

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue) => throw new NotImplementedException();
}

[TestFixture]
public class ColumnizerJsonConverterTests
{
    [Test]
    public void SerializeDeserialize_MockColumnizer_RoundTripPreservesStateAndType ()
    {
        var original = new MockColumnizer
        {
            IntProperty = 42,
            StringProperty = "TestValue"
        };

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Converters = { new ColumnizerJsonConverter() }
        };

        var json = JsonConvert.SerializeObject(original, settings);
        var deserialized = JsonConvert.DeserializeObject<ILogLineColumnizer>(json, settings);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(original.GetName(), Is.EqualTo(deserialized.GetName()));
        Assert.That(42, Is.EqualTo(((MockColumnizer)deserialized).IntProperty));
        Assert.That("TestValue", Is.EqualTo(((MockColumnizer)deserialized).StringProperty));
    }
}