namespace LogExpert.Core.Extensions;

public static class Extensions
{
    public static bool IsEmpty<T> (this IEnumerable<T> collection)
    {
        return collection == null || !collection.Any();
    }

    public static bool IsEmpty<T> (this IList<T> list)
    {
        return list == null || list.Count == 0;
    }
}