namespace Fieldnotes.Models;

public sealed class StudyDocument
{
    public int SchemaVersion { get; set; } = 1;
    public long Revision { get; set; }
    public List<LearningProgress> Progress { get; set; } = [];
    public List<QuizAttempt> Attempts { get; set; } = [];
    public List<StudyActivity> Activities { get; set; } = [];
}