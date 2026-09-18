using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateSearchIndex",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullNameNormalized = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSearchIndex", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSearchIndexEducationLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateSearchIndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSearchIndexEducationLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateSearchIndexEducationLevels_CandidateSearchIndex_CandidateSearchIndexId",
                        column: x => x.CandidateSearchIndexId,
                        principalTable: "CandidateSearchIndex",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSearchIndexSectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateSearchIndexId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSearchIndexSectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateSearchIndexSectors_CandidateSearchIndex_CandidateSearchIndexId",
                        column: x => x.CandidateSearchIndexId,
                        principalTable: "CandidateSearchIndex",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndex_CompletionPercentage",
                table: "CandidateSearchIndex",
                column: "CompletionPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndex_DistrictId",
                table: "CandidateSearchIndex",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndex_FullNameNormalized",
                table: "CandidateSearchIndex",
                column: "FullNameNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndex_ProvinceId",
                table: "CandidateSearchIndex",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndexEducationLevels_CandidateSearchIndexId_EducationLevelId",
                table: "CandidateSearchIndexEducationLevels",
                columns: new[] { "CandidateSearchIndexId", "EducationLevelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndexEducationLevels_EducationLevelId",
                table: "CandidateSearchIndexEducationLevels",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndexSectors_CandidateSearchIndexId_SectorId",
                table: "CandidateSearchIndexSectors",
                columns: new[] { "CandidateSearchIndexId", "SectorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSearchIndexSectors_SectorId",
                table: "CandidateSearchIndexSectors",
                column: "SectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSearchIndexEducationLevels");

            migrationBuilder.DropTable(
                name: "CandidateSearchIndexSectors");

            migrationBuilder.DropTable(
                name: "CandidateSearchIndex");
        }
    }
}
