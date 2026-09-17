namespace Fieldnotes.Models;

public sealed record Concept
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Track { get; init; }
    public required string Level { get; init; }
    public required int Minutes { get; init; }
    public required string Summary { get; init; }
    public required string Explanation { get; init; }
    public required string Why { get; init; }
    public required string UseCase { get; init; }
    public required string WhenToUse { get; init; }
    public required string[] Pitfalls { get; init; }
    public required string[] BestPractices { get; init; }
    public required string SimpleCode { get; init; }
    public required string PracticalCode { get; init; }
    public required string[] Takeaways { get; init; }
    public required InterviewQuestion[] Interview { get; init; }
    public required QuizQuestion[] Quiz { get; init; }
    public required string[] Related { get; init; }
    public required string DocsUrl { get; init; }
}

public sealed record InterviewQuestion(string Question, string Answer);
public sealed record QuizQuestion(string Id, string Question, string[] Options, int CorrectIndex, string Explanation);
public sealed record QuizItem(Concept Concept, QuizQuestion Question);