using System.Text.Json.Serialization;

namespace Fieldnotes.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Concept[]))]
[JsonSerializable(typeof(StudyDocument))]
[JsonSerializable(typeof(StudyExport))]
public partial class StudyJsonContext : JsonSerializerContext;