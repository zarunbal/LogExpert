namespace LogExpert.Core.Classes.Persister;

/// <summary>
/// Marks a property for inclusion in columnizer JSON serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class JsonColumnizerPropertyAttribute : Attribute
{
}

