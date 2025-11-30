using System.Text.Json;

namespace LogExpert.Core.Classes;

public static class ObjectClone
{
    #region Public methods

    public static T Clone<T> (T realObject)
    {
        using MemoryStream objectStream = new();

        JsonSerializer.Serialize(objectStream, realObject);
        _ = objectStream.Seek(0, SeekOrigin.Begin);
        return JsonSerializer.Deserialize<T>(objectStream);
    }

    #endregion
}