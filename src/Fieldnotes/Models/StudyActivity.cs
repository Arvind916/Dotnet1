namespace Fieldnotes.Models;

public sealed class StudyActivity
{
    public string ConceptId { get; set; } = "";
    public string Kind { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class QuizAttempt
{
    public Guid Id { get; set; }
    public string ConceptId { get; set; } = "";
    public string QuestionId { get; set; } = "";
    public int SelectedIndex { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}

public sealed record StudySnapshot(
    IReadOnlyList<LearningProgress> Progress,
    IReadOnlyList<QuizAttempt> Attempts,
    IReadOnlyList<StudyActivity> Activities)
{
    public static StudySnapshot Empty { get; } = new([], [], []);
    public LearningProgress? For(string conceptId) => Progress.FirstOrDefault(item => item.ConceptId == conceptId);
    public int Completed => Progress.Count(item => item.IsCompleted);
    public int Accuracy => Services.StudyPlanner.Accuracy(Attempts.Count(item => item.IsCorrect), Attempts.Count);
}