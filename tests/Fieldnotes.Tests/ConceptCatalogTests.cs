using Fieldnotes.Services;

namespace Fieldnotes.Tests;

public sealed class ConceptCatalogTests
{
    private static ConceptCatalog Load() => ConceptCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content"));

    [Fact]
    public void BrowserBundleContainsTheValidatedSourceCurriculum()
    {
        var bundled = ConceptCatalog.LoadBundled();
        var source = Load();

        Assert.Equal(source.All.Select(concept => concept.Id), bundled.All.Select(concept => concept.Id));
        Assert.Equal(source.All.SelectMany(concept => concept.Quiz).Select(question => question.Id),
            bundled.All.SelectMany(concept => concept.Quiz).Select(question => question.Id));
    }

    [Fact]
    public void CurriculumCoversEveryRequiredTopicAndHasCompleteExamples()
    {
        var catalog = Load();
        string[] requiredIds = ["csharp-types", "oop", "solid", "collections", "generics", "delegates-events",
            "linq", "async-await", "threading", "exceptions", "memory", "dependency-injection", "design-patterns",
            "aspnet-core", "web-api", "middleware", "authentication", "authorization", "ef-core", "unit-testing",
            "integration-testing", "logging", "configuration", "caching", "performance", "clean-architecture", "microservices"];

        Assert.True(catalog.All.Count >= 30);
        Assert.Equal(6, catalog.Tracks.Length);
        Assert.All(requiredIds, id => Assert.NotNull(catalog.Find(id)));
        Assert.All(catalog.All, concept =>
        {
            Assert.True(concept.Quiz.Length >= 2);
            Assert.True(concept.Interview.Length >= 2);
            Assert.True(concept.Takeaways.Length >= 3);
            Assert.True(concept.Related.Length >= 2);
        });
    }

    [Fact]
    public void SearchCombinesTextTrackAndDifficulty()
    {
        var results = Load().Search("ASYNC", "Runtime & concurrency", "Intermediate").ToList();

        Assert.Contains(results, concept => concept.Id == "async-await");
        Assert.All(results, concept => Assert.Equal("Intermediate", concept.Level));
        Assert.Empty(Load().Search("no-such-concept-12345", null, null));
    }

    [Fact]
    public void InvalidCorrectOptionIsRejected()
    {
        var original = Load().All[0];
        var invalid = original with
        {
            Related = [],
            Quiz = [original.Quiz[0] with { CorrectIndex = 99 }]
        };

        Assert.Throws<InvalidDataException>(() => new ConceptCatalog([invalid]));
    }

    [Fact]
    public void BrokenRelatedLinkIsRejected()
    {
        var original = Load().All[0] with { Related = ["missing"] };

        Assert.Throws<InvalidDataException>(() => new ConceptCatalog([original]));
    }
}