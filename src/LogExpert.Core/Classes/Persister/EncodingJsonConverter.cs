using System.Text;

using Newtonsoft.Json;

namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Custom JsonConverter for Encoding objects.
/// Serializes the encoding as its name (e.g. "utf-8").
/// </summary>
public class EncodingJsonConverter : JsonConverter
{
    public override bool CanConvert (Type objectType)
    {
        return typeof(Encoding).IsAssignableFrom(objectType);
    }

    public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value is not Encoding encoding)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(encoding.WebName);
    }

    public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var encodingName = reader.Value?.ToString();
        if (string.IsNullOrEmpty(encodingName))
        {
            return Encoding.Default;
        }

        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException)
        {
            return Encoding.Default;
        }
    }
}
