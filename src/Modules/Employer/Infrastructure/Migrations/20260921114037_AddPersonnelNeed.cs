using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnelNeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonnelNeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkLocationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExperienceLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetailsText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PooledByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PooledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FulfilledByCandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelNeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelNeedDrivingLicensePreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DriversLicenseTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelNeedDrivingLicensePreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelNeedDrivingLicensePreferences_PersonnelNeeds_PersonnelNeedId",
                        column: x => x.PersonnelNeedId,
                        principalTable: "PersonnelNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelNeedEducationLevelPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelNeedEducationLevelPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelNeedEducationLevelPreferences_PersonnelNeeds_PersonnelNeedId",
                        column: x => x.PersonnelNeedId,
                        principalTable: "PersonnelNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelNeedGenderPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelNeedGenderPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelNeedGenderPreferences_PersonnelNeeds_PersonnelNeedId",
                        column: x => x.PersonnelNeedId,
                        principalTable: "PersonnelNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelNeedMilitaryStatusPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilitaryStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelNeedMilitaryStatusPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelNeedMilitaryStatusPreferences_PersonnelNeeds_PersonnelNeedId",
                        column: x => x.PersonnelNeedId,
                        principalTable: "PersonnelNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelNeedDrivingLicensePreferences_PersonnelNeedId",
                table: "PersonnelNeedDrivingLicensePreferences",
                column: "PersonnelNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelNeedEducationLevelPreferences_PersonnelNeedId",
                table: "PersonnelNeedEducationLevelPreferences",
                column: "PersonnelNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelNeedGenderPreferences_PersonnelNeedId",
                table: "PersonnelNeedGenderPreferences",
                column: "PersonnelNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelNeedMilitaryStatusPreferences_PersonnelNeedId",
                table: "PersonnelNeedMilitaryStatusPreferences",
                column: "PersonnelNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelNeeds_CompanyId",
                table: "PersonnelNeeds",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonnelNeedDrivingLicensePreferences");

            migrationBuilder.DropTable(
                name: "PersonnelNeedEducationLevelPreferences");

            migrationBuilder.DropTable(
                name: "PersonnelNeedGenderPreferences");

            migrationBuilder.DropTable(
                name: "PersonnelNeedMilitaryStatusPreferences");

            migrationBuilder.DropTable(
                name: "PersonnelNeeds");
        }
    }
}
