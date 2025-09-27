
using ColumnizerLib;

using LogExpert.Core.Classes.Persister;

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
    public string GetColumnValue (LogLine line, int column) => "";
    public bool IsTimeshiftImplemented () => false;
    public void PushValue (LogLine line, int column, string value) { }
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

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.GetName(), deserialized.GetName());
        Assert.AreEqual(42, ((MockColumnizer)deserialized).IntProperty);
        Assert.AreEqual("TestValue", ((MockColumnizer)deserialized).StringProperty);
    }
}