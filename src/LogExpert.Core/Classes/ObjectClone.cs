using Newtonsoft.Json;

namespace LogExpert.Core.Classes;

public static class ObjectClone
{
    #region Public methods

    /// <summary>
    /// Creates a deep clone of an object using JSON serialization.
    /// Uses Newtonsoft.Json to ensure proper handling of complex types like System.Drawing.Color.
    /// </summary>
    /// <typeparam name="T">Type of object to clone</typeparam>
    /// <param name="realObject">Object to clone</param>
    /// <returns>Deep clone of the object</returns>
    public static T Clone<T> (T realObject)
    {
        var json = JsonConvert.SerializeObject(realObject);
        return JsonConvert.DeserializeObject<T>(json);
    }

    #endregion
}