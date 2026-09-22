using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Matching.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuggestingAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSuggestions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSuggestions_PersonnelNeedId_CandidateCvId",
                table: "CandidateSuggestions",
                columns: new[] { "PersonnelNeedId", "CandidateCvId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSuggestions");
        }
    }
}
