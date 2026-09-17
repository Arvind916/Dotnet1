using Fieldnotes.Models;

namespace Fieldnotes.Services;

public static class StudyPlanner
{
    private static readonly int[] ReviewIntervals = [1, 3, 7, 14, 30];

    public static void RecordAnswer(LearningProgress progress, bool isCorrect, DateTime now)
    {
        progress.Attempts++;
        progress.CorrectAnswers += isCorrect ? 1 : 0;
        RecordReview(progress, isCorrect, now);
    }

    public static void RecordReview(LearningProgress progress, bool remembered, DateTime now)
    {
        progress.ReviewStage = remembered
            ? Math.Min(progress.ReviewStage + 1, ReviewIntervals.Length)
            : 0;
        progress.LastStudiedAt = now;
        progress.NextReviewAt = remembered
            ? now.AddDays(ReviewIntervals[progress.ReviewStage - 1])
            : now.AddMinutes(10);
    }

    public static bool IsWeak(LearningProgress progress) =>
        progress.Attempts > 0 && (double)progress.CorrectAnswers / progress.Attempts < 0.7;

    public static bool IsDue(LearningProgress progress, DateTime now) =>
        progress.NextReviewAt is { } due && due <= now;

    public static int Accuracy(int correctAnswers, int attempts) =>
        attempts == 0 ? 0 : (int)Math.Round(100.0 * correctAnswers / attempts);

    public static int Streak(IEnumerable<DateTime> activityDates, DateTime now)
    {
        var days = activityDates.Select(date => date.Date).ToHashSet();
        var currentDay = days.Contains(now.Date) ? now.Date : now.Date.AddDays(-1);
        var streak = 0;
        while (days.Contains(currentDay))
        {
            streak++;
            currentDay = currentDay.AddDays(-1);
        }

        return streak;
    }
}