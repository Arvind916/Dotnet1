using System.Text.Json;
using Fieldnotes.Models;

namespace Fieldnotes.Services;

public sealed class ConceptCatalog
{
    public IReadOnlyList<Concept> All { get; }
    public string[] Tracks => All.Select(concept => concept.Track).Distinct().ToArray();
    private readonly Dictionary<string, Concept> byId;

    public ConceptCatalog(IEnumerable<Concept> concepts)
    {
        var entries = concepts.ToArray();
        if (entries.Length == 0 || entries.Select(item => item.Id).Distinct().Count() != entries.Length)
        {
            throw new InvalidDataException("The catalog must contain concepts with unique IDs.");
        }

        byId = entries.ToDictionary(item => item.Id);
        var questionIds = new HashSet<string>();
        foreach (var concept in entries)
        {
            string[] required = [concept.Id, concept.Title, concept.Track, concept.Summary, concept.Explanation,
                concept.Why, concept.UseCase, concept.WhenToUse, concept.SimpleCode, concept.PracticalCode];
            if (required.Any(string.IsNullOrWhiteSpace) || concept.Minutes < 1 ||
                concept.Level is not ("Beginner" or "Intermediate" or "Advanced") ||
                concept.Takeaways.Length == 0 || concept.Pitfalls.Length == 0 || concept.BestPractices.Length == 0 ||
                concept.Interview.Length == 0 || concept.Quiz.Length == 0 ||
                concept.Related.Any(related => !byId.ContainsKey(related)) ||
                !Uri.TryCreate(concept.DocsUrl, UriKind.Absolute, out var docsUri) || docsUri.Scheme != "https")
            {
                throw new InvalidDataException($"Concept '{concept.Id}' is incomplete or has invalid references.");
            }

            foreach (var question in concept.Quiz)
            {
                if (string.IsNullOrWhiteSpace(question.Id) || !questionIds.Add(question.Id) ||
                    string.IsNullOrWhiteSpace(question.Question) || string.IsNullOrWhiteSpace(question.Explanation) ||
                    question.Options.Length < 2 || question.Options.Any(string.IsNullOrWhiteSpace) ||
                    question.CorrectIndex < 0 || question.CorrectIndex >= question.Options.Length)
                {
                    throw new InvalidDataException($"Concept '{concept.Id}' has an invalid quiz question.");
                }
            }
        }

        All = Array.AsReadOnly(entries);
    }

    public static ConceptCatalog Load(string directory)
    {
        var concepts = Directory.EnumerateFiles(directory, "*.json").Order()
            .SelectMany(path => JsonSerializer.Deserialize(File.ReadAllText(path), StudyJsonContext.Default.ConceptArray)
                ?? throw new InvalidDataException($"Cannot load '{path}'."));
        return new ConceptCatalog(concepts);
    }

    public static ConceptCatalog LoadBundled()
    {
        var assembly = typeof(ConceptCatalog).Assembly;
        var concepts = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("Fieldnotes.Content.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .SelectMany(name =>
            {
                using var stream = assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidDataException($"Cannot load '{name}'.");
                return JsonSerializer.Deserialize(stream, StudyJsonContext.Default.ConceptArray)
                    ?? throw new InvalidDataException($"Cannot load '{name}'.");
            });
        return new ConceptCatalog(concepts);
    }

    public Concept? Find(string id) => byId.GetValueOrDefault(id);

    public IEnumerable<Concept> Search(string? query, string? track, string? level)
    {
        return All.Where(concept =>
            (string.IsNullOrWhiteSpace(track) || concept.Track == track) &&
            (string.IsNullOrWhiteSpace(level) || concept.Level == level) &&
            (string.IsNullOrWhiteSpace(query) ||
             $"{concept.Title} {concept.Track} {concept.Summary} {concept.Explanation}"
                 .Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)));
    }
}