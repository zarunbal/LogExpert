namespace LogExpert.Core.Classes.Attributes;

/// <summary>
/// Marks a property for inclusion in columnizer JSON serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonColumnizerPropertyAttribute : Attribute
{
}

