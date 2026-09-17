using System.Text.Json;
using Fieldnotes.Models;
using Fieldnotes.Services;

namespace Fieldnotes.Tests;

public sealed class StudyServiceTests
{
    private readonly Dictionary<string, TestStudyStore> stores = [];
    private readonly ConceptCatalog catalog = new([new Concept
    {
        Id = "async", Title = "Async / await", Track = "Runtime", Level = "Intermediate", Minutes = 5,
        Summary = "Await I/O without blocking.", Explanation = "An asynchronous wait releases the calling thread.",
        Why = "Keep the application responsive.", UseCase = "Fetch an order.", WhenToUse = "I/O operations.",
        Pitfalls = ["Blocking on Result."], BestPractices = ["Pass cancellation tokens."],
        SimpleCode = "await Task.Delay(10);", PracticalCode = "await client.GetAsync(uri, cancellationToken);",
        Takeaways = ["Async is not necessarily parallel."], Interview = [new("Does await start a thread?", "No.")],
        Quiz = [new("async-1", "Does await start a thread?", ["Yes", "No"], 1, "Await does not create a thread.")],
        Related = [], DocsUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/"
    }]);

    [Fact]
    public async Task ProgressIsPersistentAndIsolatedBetweenBrowserStores()
    {
        await Service("alice").ToggleCompletedAsync("async");
        await Service("alice").ToggleBookmarkAsync("async");

        var alice = await Service("alice").GetSnapshotAsync();
        var bob = await Service("bob").GetSnapshotAsync();

        Assert.True(Assert.Single(alice.Progress).IsCompleted);
        Assert.True(alice.Progress[0].IsBookmarked);
        Assert.Empty(bob.Progress);
        Assert.Empty(bob.Activities);
    }

    [Fact]
    public async Task ReplayedQuizSubmissionDoesNotDoubleCount()
    {
        var attemptId = Guid.NewGuid();
        var service = Service("alice");

        Assert.True(await service.SubmitAnswerAsync(attemptId, "async", "async-1", 1));
        Assert.True(await service.SubmitAnswerAsync(attemptId, "async", "async-1", 1));

        var snapshot = await service.GetSnapshotAsync();
        Assert.Single(snapshot.Attempts);
        Assert.Equal(1, Assert.Single(snapshot.Progress).Attempts);
        Assert.Equal(100, snapshot.Accuracy);
    }

    [Fact]
    public async Task InvalidAnswerAndConceptCannotWriteProgress()
    {
        var service = Service("alice");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.SubmitAnswerAsync(Guid.NewGuid(), "async", "async-1", 5));
        await Assert.ThrowsAsync<ArgumentException>(() => service.ToggleCompletedAsync("unknown"));
        Assert.Empty((await service.GetSnapshotAsync()).Progress);
    }

    [Fact]
    public async Task FlashcardReviewDoesNotInflateQuizAccuracy()
    {
        var service = Service("alice");
        await service.ReviewAsync("async", true);

        var snapshot = await service.GetSnapshotAsync();
        Assert.Empty(snapshot.Attempts);
        Assert.Equal(0, Assert.Single(snapshot.Progress).Attempts);
        Assert.NotNull(snapshot.Progress[0].NextReviewAt);
        Assert.Equal("Recalled", Assert.Single(snapshot.Activities).Kind);
    }

    [Fact]
    public async Task RepeatedReadsOnSameDayDoNotDuplicateHistory()
    {
        var service = Service("alice");
        await service.RecordReadAsync("async");
        await service.RecordReadAsync("async");

        Assert.Single((await service.GetSnapshotAsync()).Activities);
    }

    [Fact]
    public async Task ConcurrentTabSaveIsRetriedWithoutLosingChanges()
    {
        var service = Service("alice");
        stores["alice"].BeforeNextSave = document =>
        {
            document.Revision++;
            document.Progress.Add(new LearningProgress { ConceptId = "async", IsBookmarked = true });
        };

        await service.ToggleCompletedAsync("async");

        var progress = Assert.Single((await service.GetSnapshotAsync()).Progress);
        Assert.True(progress.IsBookmarked);
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public async Task ExportPreservesProgressWithoutStorageIdentifiers()
    {
        var service = Service("alice");
        await service.ToggleCompletedAsync("async");
        await service.SubmitAnswerAsync(Guid.NewGuid(), "async", "async-1", 1);

        var json = await service.ExportAsync();
        var export = JsonSerializer.Deserialize(json, StudyJsonContext.Default.StudyExport)!;

        Assert.Equal(1, export.Version);
        Assert.True(Assert.Single(export.Progress).IsCompleted);
        Assert.True(Assert.Single(export.QuizAttempts).IsCorrect);
        Assert.Equal(2, export.History.Count);
        Assert.DoesNotContain("learnerId", json);
        Assert.DoesNotContain("\"id\":", json);
    }

    [Fact]
    public async Task ConflictingQuizReplayIsRejected()
    {
        var service = Service("alice");
        var attemptId = Guid.NewGuid();
        await service.SubmitAnswerAsync(attemptId, "async", "async-1", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitAnswerAsync(attemptId, "async", "async-1", 0));
        Assert.Single((await service.GetSnapshotAsync()).Attempts);
    }

    [Fact]
    public async Task StorageFailureDoesNotReportSuccessOrMutateSavedProgress()
    {
        var service = Service("alice");
        stores["alice"].FailWrites = true;

        await Assert.ThrowsAsync<IOException>(() => service.ToggleCompletedAsync("async"));
        Assert.Empty((await service.GetSnapshotAsync()).Progress);
    }

    private StudyService Service(string browser)
    {
        if (!stores.TryGetValue(browser, out var store))
        {
            store = new TestStudyStore();
            stores.Add(browser, store);
        }

        return new StudyService(store, catalog, TimeProvider.System);
    }

    private sealed class TestStudyStore : IStudyStore
    {
        private StudyDocument saved = new();
        public Action<StudyDocument>? BeforeNextSave { get; set; }
        public bool FailWrites { get; set; }

        public Task<StudyDocument> LoadAsync() => Task.FromResult(Clone(saved));

        public Task<bool> TrySaveAsync(StudyDocument document, long expectedRevision)
        {
            if (FailWrites)
            {
                throw new IOException("Browser storage is unavailable.");
            }

            BeforeNextSave?.Invoke(saved);
            BeforeNextSave = null;
            if (saved.Revision != expectedRevision)
            {
                return Task.FromResult(false);
            }

            saved = Clone(document);
            return Task.FromResult(true);
        }

        private static StudyDocument Clone(StudyDocument document) =>
            JsonSerializer.Deserialize<StudyDocument>(JsonSerializer.Serialize(document))!;
    }
}