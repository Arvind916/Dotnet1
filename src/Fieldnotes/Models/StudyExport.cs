namespace Fieldnotes.Models;

public sealed record StudyExport(
    int Version,
    DateTimeOffset ExportedAt,
    IReadOnlyList<LearningProgress> Progress,
    IReadOnlyList<ExportedQuizAnswer> QuizAttempts,
    IReadOnlyList<StudyActivity> History);

public sealed record ExportedQuizAnswer(
    string ConceptId,
    string QuestionId,
    int SelectedIndex,
    bool IsCorrect,
    DateTime AnsweredAt);