using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fieldnotes.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LearnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ConceptId = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Progress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LearnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ConceptId = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBookmarked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewStage = table.Column<int>(type: "INTEGER", nullable: false),
                    LastStudiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearnerId = table.Column<string>(type: "TEXT", nullable: false),
                    ConceptId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    SelectedIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_LearnerId_CreatedAt",
                table: "Activities",
                columns: new[] { "LearnerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Progress_LearnerId_ConceptId",
                table: "Progress",
                columns: new[] { "LearnerId", "ConceptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_LearnerId_AnsweredAt",
                table: "QuizAttempts",
                columns: new[] { "LearnerId", "AnsweredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Progress");

            migrationBuilder.DropTable(
                name: "QuizAttempts");
        }
    }
}
