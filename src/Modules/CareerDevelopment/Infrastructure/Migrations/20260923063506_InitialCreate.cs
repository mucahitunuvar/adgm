using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvisorRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvisorRecommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TargetPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SetByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillGaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentifiedByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdentifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrainingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendedByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecommendedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecommendations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvisorRecommendations_CandidateCvId",
                table: "AdvisorRecommendations",
                column: "CandidateCvId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerGoals_CandidateCvId",
                table: "CareerGoals",
                column: "CandidateCvId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGaps_CandidateCvId",
                table: "SkillGaps",
                column: "CandidateCvId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecommendations_CandidateCvId",
                table: "TrainingRecommendations",
                column: "CandidateCvId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvisorRecommendations");

            migrationBuilder.DropTable(
                name: "CareerGoals");

            migrationBuilder.DropTable(
                name: "SkillGaps");

            migrationBuilder.DropTable(
                name: "TrainingRecommendations");
        }
    }
}
