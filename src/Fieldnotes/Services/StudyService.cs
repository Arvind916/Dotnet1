using System.Text.Json;
using Fieldnotes.Models;

namespace Fieldnotes.Services;

public sealed class StudyService(
    IStudyStore store,
    ConceptCatalog catalog,
    TimeProvider clock)
{
    public async Task<StudySnapshot> GetSnapshotAsync()
    {
        var document = await ReadAsync();
        return new StudySnapshot(
            document.Progress,
            document.Attempts.OrderByDescending(item => item.AnsweredAt).ToArray(),
            document.Activities.OrderByDescending(item => item.CreatedAt).ToArray());
    }

    public async Task<string> ExportAsync()
    {
        var snapshot = await GetSnapshotAsync();
        var export = new StudyExport(1, clock.GetUtcNow(), snapshot.Progress,
            snapshot.Attempts.Select(item => new ExportedQuizAnswer(
                item.ConceptId, item.QuestionId, item.SelectedIndex, item.IsCorrect, item.AnsweredAt)).ToArray(),
            snapshot.Activities);
        return JsonSerializer.Serialize(export, StudyJsonContext.Default.StudyExport);
    }

    public Task ToggleBookmarkAsync(string conceptId) => MutateAsync(conceptId, (progress, now) =>
    {
        progress.IsBookmarked = !progress.IsBookmarked;
        return null;
    });

    public Task ToggleCompletedAsync(string conceptId) => MutateAsync(conceptId, (progress, now) =>
    {
        progress.IsCompleted = !progress.IsCompleted;
        progress.LastStudiedAt = now;
        if (progress.IsCompleted)
        {
            progress.NextReviewAt ??= now.AddDays(1);
        }

        return progress.IsCompleted ? "Completed" : "Reopened";
    });

    public Task ReviewAsync(string conceptId, bool remembered) => MutateAsync(conceptId, (progress, now) =>
    {
        StudyPlanner.RecordReview(progress, remembered, now);
        return remembered ? "Recalled" : "Revisit";
    });

    public Task RecordReadAsync(string conceptId)
    {
        EnsureConcept(conceptId);
        return UpdateAsync(document =>
        {
            var now = clock.GetUtcNow().UtcDateTime;
            FindOrCreate(document, conceptId).LastStudiedAt = now;
            if (!document.Activities.Any(item => item.ConceptId == conceptId &&
                item.Kind == "Read" && item.CreatedAt.Date == now.Date))
            {
                AddActivity(document, conceptId, "Read", now);
            }

            return true;
        });
    }

    public async Task<bool> SubmitAnswerAsync(Guid attemptId, string conceptId, string questionId, int selectedIndex)
    {
        var concept = EnsureConcept(conceptId);
        var question = concept.Quiz.SingleOrDefault(item => item.Id == questionId)
            ?? throw new ArgumentException("Unknown question.", nameof(questionId));
        if (attemptId == Guid.Empty || selectedIndex < 0 || selectedIndex >= question.Options.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedIndex));
        }

        return await UpdateAsync(document =>
        {
            var existing = document.Attempts.SingleOrDefault(item => item.Id == attemptId);
            if (existing is not null)
            {
                if (existing.ConceptId != conceptId || existing.QuestionId != questionId || existing.SelectedIndex != selectedIndex)
                {
                    throw new InvalidOperationException("This attempt was already submitted with different data.");
                }

                return existing.IsCorrect;
            }

            var now = clock.GetUtcNow().UtcDateTime;
            var isCorrect = question.CorrectIndex == selectedIndex;
            StudyPlanner.RecordAnswer(FindOrCreate(document, conceptId), isCorrect, now);
            document.Attempts.Add(new QuizAttempt
            {
                Id = attemptId, ConceptId = conceptId, QuestionId = questionId,
                SelectedIndex = selectedIndex, IsCorrect = isCorrect, AnsweredAt = now
            });
            AddActivity(document, conceptId, isCorrect ? "Quiz correct" : "Quiz missed", now);
            return isCorrect;
        });
    }

    private Task MutateAsync(string conceptId, Func<LearningProgress, DateTime, string?> mutation)
    {
        EnsureConcept(conceptId);
        return UpdateAsync(document =>
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var kind = mutation(FindOrCreate(document, conceptId), now);
            if (kind is not null)
            {
                AddActivity(document, conceptId, kind, now);
            }

            return true;
        });
    }

    private async Task<StudyDocument> ReadAsync()
    {
        var document = await store.LoadAsync();
        if (document.SchemaVersion != 1)
        {
            throw new InvalidDataException("This progress was saved by an unsupported application version.");
        }

        return document;
    }

    private async Task<TResult> UpdateAsync<TResult>(Func<StudyDocument, TResult> mutation)
    {
        for (var retry = 0; retry < 8; retry++)
        {
            var document = await ReadAsync();
            var expectedRevision = document.Revision;
            var result = mutation(document);
            document.Revision = checked(expectedRevision + 1);
            if (await store.TrySaveAsync(document, expectedRevision))
            {
                return result;
            }
        }

        throw new InvalidOperationException("Progress changed in another tab. Please try again.");
    }

    private static LearningProgress FindOrCreate(StudyDocument document, string conceptId)
    {
        var progress = document.Progress.SingleOrDefault(item => item.ConceptId == conceptId);
        if (progress is not null)
        {
            return progress;
        }

        progress = new LearningProgress { ConceptId = conceptId };
        document.Progress.Add(progress);
        return progress;
    }

    private static void AddActivity(StudyDocument document, string conceptId, string kind, DateTime now) =>
        document.Activities.Add(new StudyActivity
        {
            ConceptId = conceptId, Kind = kind, CreatedAt = now
        });

    private Concept EnsureConcept(string conceptId) => catalog.Find(conceptId)
        ?? throw new ArgumentException("Unknown concept.", nameof(conceptId));
}