namespace Fieldnotes.Models;

public sealed class LearningProgress
{
    public string ConceptId { get; set; } = "";
    public bool IsCompleted { get; set; }
    public bool IsBookmarked { get; set; }
    public int Attempts { get; set; }
    public int CorrectAnswers { get; set; }
    public int ReviewStage { get; set; }
    public DateTime? LastStudiedAt { get; set; }
    public DateTime? NextReviewAt { get; set; }
}