# EncodingJsonConverter Documentation

## Purpose
`EncodingJsonConverter` is a custom `JsonConverter` for serializing and deserializing .NET `Encoding` objects in LogExpert's persistence and configuration files. It stores the encoding as its canonical name (e.g., "utf-8", "windows-1252") in JSON, allowing for accurate restoration of encoding settings across sessions.

## Usage
- The converter is applied to the `Preferences.DefaultEncoding` property, which is now of type `Encoding`.
- It is registered in the Persister so all encoding properties are handled automatically during save/load.

## Serialization
- When saving, the converter writes the encoding's `WebName` (e.g., "utf-8") as a string value in JSON.
- When loading, it reads the string and uses `Encoding.GetEncoding(name)` to restore the correct encoding. If the name is invalid or missing, it falls back to `Encoding.Default`.

## Example
```json
{
  "DefaultEncoding": "utf-8"
}
```

## Benefits
- Ensures encoding settings are preserved and portable.
- Handles null and invalid encoding names gracefully.

## Implementation Location
- Converter: `LogExpert.Core\Classes\Persister\EncodingJsonConverter.cs`
- Usage: `LogExpert.Core\Config\Preferences.cs` and `LogExpert.Core\Classes\Persister\Persister.cs`

## Migration Notes
- If you previously stored encoding as a string, migration is seamless as the converter accepts string names.
- No changes required for existing JSON files unless you want to use more specific encodings.

---
**Author:** GitHub Copilot
**Date:** 2024-06-10
