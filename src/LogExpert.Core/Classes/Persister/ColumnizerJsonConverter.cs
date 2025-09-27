using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Custom JsonConverter for ILogLineColumnizer implementations.
/// Serializes only properties marked with [JsonColumnizerProperty].
/// Uses GetName() for type identification.
/// </summary>
public class ColumnizerJsonConverter : JsonConverter
{
    public override bool CanConvert (Type objectType)
    {
        return typeof(ILogLineColumnizer).IsAssignableFrom(objectType);
    }

    public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not ILogLineColumnizer columnizer)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();
        var stateObj = new JObject();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<JsonColumnizerPropertyAttribute>() != null && prop.CanRead)
            {
                var propValue = prop.GetValue(value);
                stateObj[prop.Name] = propValue != null ? JToken.FromObject(propValue, serializer) : JValue.CreateNull();
            }
        }

        writer.WriteStartObject();
        writer.WritePropertyName("Type");
        writer.WriteValue(columnizer.GetName());
        writer.WritePropertyName("State");
        stateObj.WriteTo(writer);
        writer.WriteEndObject();
    }

    public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeName = jo["Type"]?.ToString();
        if (typeName == null || jo["State"] is not JObject state)
        {
            return null;
        }

        // Find the columnizer type by GetName()
        var columnizerType = FindColumnizerTypeByName(typeName) ?? throw new JsonSerializationException($"Columnizer type '{typeName}' not found.");

        var instance = Activator.CreateInstance(columnizerType);
        foreach (var prop in columnizerType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<JsonColumnizerPropertyAttribute>() != null && prop.CanWrite)
            {
                var token = state[prop.Name];
                if (token != null && token.Type != JTokenType.Null)
                {
                    var value = token.ToObject(prop.PropertyType, serializer);
                    prop.SetValue(instance, value);
                }
            }
        }
        return instance;
    }

    private Type FindColumnizerTypeByName (string name)
    {
        // Search all loaded assemblies for a type implementing ILogLineColumnizer with matching GetName()
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes().Where(t => typeof(ILogLineColumnizer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
            {
                try
                {
                    if (Activator.CreateInstance(type) is ILogLineColumnizer inst && inst.GetName() == name)
                    {
                        return type;
                    }
                }
                catch { }
            }
        }
        return null;
    }
}