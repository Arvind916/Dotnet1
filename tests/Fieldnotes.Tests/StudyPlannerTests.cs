using Fieldnotes.Models;
using Fieldnotes.Services;

namespace Fieldnotes.Tests;

public sealed class StudyPlannerTests
{
    private static readonly DateTime Now = new(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CorrectAnswerSchedulesSpacedReviewWithoutCompletingLesson()
    {
        var progress = new LearningProgress();

        StudyPlanner.RecordAnswer(progress, true, Now);

        Assert.Equal(1, progress.Attempts);
        Assert.Equal(1, progress.CorrectAnswers);
        Assert.Equal(Now.AddDays(1), progress.NextReviewAt);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void WrongAnswerResetsReviewIntervalAndIdentifiesWeakTopic()
    {
        var progress = new LearningProgress();
        StudyPlanner.RecordAnswer(progress, true, Now);

        StudyPlanner.RecordAnswer(progress, false, Now);

        Assert.Equal(0, progress.ReviewStage);
        Assert.Equal(Now.AddMinutes(10), progress.NextReviewAt);
        Assert.True(StudyPlanner.IsWeak(progress));
        Assert.False(StudyPlanner.IsDue(progress, Now));
        Assert.True(StudyPlanner.IsDue(progress, Now.AddMinutes(10)));
    }

    [Fact]
    public void ReviewIntervalCapsAtThirtyDays()
    {
        var progress = new LearningProgress();
        foreach (var attempt in Enumerable.Range(0, 10))
        {
            StudyPlanner.RecordAnswer(progress, true, Now);
        }

        Assert.Equal(5, progress.ReviewStage);
        Assert.Equal(Now.AddDays(30), progress.NextReviewAt);
    }

    [Fact]
    public void UnattemptedConceptIsNotWeak()
    {
        Assert.False(StudyPlanner.IsWeak(new LearningProgress()));
        Assert.Equal(0, StudyPlanner.Accuracy(0, 0));
    }

    [Fact]
    public void StreakCountsDistinctUtcDaysAndAllowsTodayToBeUnfinished()
    {
        DateTime[] activity = [Now.AddDays(-1), Now.AddDays(-1), Now.AddDays(-2), Now.AddDays(-4)];

        Assert.Equal(2, StudyPlanner.Streak(activity, Now));
        Assert.Equal(0, StudyPlanner.Streak([], Now));
    }
}