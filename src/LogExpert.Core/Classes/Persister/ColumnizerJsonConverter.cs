using System.Reflection;
using System.Runtime.InteropServices;

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

    public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

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

    public override object ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);

        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var jObject = JObject.Load(reader);
        var typeName = jObject["Type"]?.ToString();
        if (typeName == null || jObject["State"] is not JObject state)
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

    private static Type FindColumnizerTypeByName (string name)
    {
        // Search all loaded assemblies for a type implementing ILogLineColumnizer with matching GetName()
        foreach (var currentAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in currentAssembly.GetTypes().Where(t => typeof(ILogLineColumnizer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
            {
                try
                {
                    if (Activator.CreateInstance(type) is ILogLineColumnizer inst && inst.GetName() == name)
                    {
                        return type;
                    }
                }
                catch (Exception ex) when (ex is ArgumentNullException or
                                           ArgumentException or
                                           NotSupportedException or
                                           TargetInvocationException or
                                           MethodAccessException or
                                           MemberAccessException or
                                           InvalidComObjectException or
                                           MissingMethodException or
                                           COMException or
                                           TypeLoadException)
                {
                    // intentionally ignored
                }
            }
        }

        return null;
    }
}