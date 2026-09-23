using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevelopmentPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevelopmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillGapId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CareerGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_CandidateCvId",
                table: "DevelopmentPlans",
                column: "CandidateCvId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevelopmentPlans");
        }
    }
}
