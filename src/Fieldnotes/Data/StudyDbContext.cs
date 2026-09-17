using Fieldnotes.Models;
using Microsoft.EntityFrameworkCore;

namespace Fieldnotes.Data;

public sealed class StudyDbContext(DbContextOptions<StudyDbContext> options) : DbContext(options)
{
    public DbSet<LearningProgress> Progress => Set<LearningProgress>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<StudyActivity> Activities => Set<StudyActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningProgress>().HasIndex(item => new { item.LearnerId, item.ConceptId }).IsUnique();
        modelBuilder.Entity<QuizAttempt>().HasIndex(item => new { item.LearnerId, item.AnsweredAt });
        modelBuilder.Entity<StudyActivity>().HasIndex(item => new { item.LearnerId, item.CreatedAt });
    }
}