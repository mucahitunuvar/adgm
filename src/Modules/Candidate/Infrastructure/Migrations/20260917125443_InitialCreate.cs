using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateCvContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputerSkills = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Hobbies = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CvFileKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CvFileOriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    CvFileContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CvFileSizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    CvFileUploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CvFileOwnerEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CvFileOwnerEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateCvs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotoFileKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotoOriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    PhotoContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhotoSizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    PhotoUploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhotoOwnerEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhotoOwnerEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DriversLicenseTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NetSalaryExpectation = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MilitaryStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisabilityCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisabilityPercentage = table.Column<int>(type: "int", nullable: true),
                    DisabilityDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DisabilityHasHealthReport = table.Column<bool>(type: "bit", nullable: true),
                    DisabilityUsesMedication = table.Column<bool>(type: "bit", nullable: true),
                    DisabilityHasChronicCondition = table.Column<bool>(type: "bit", nullable: true),
                    DisabilityHasContagiousDisease = table.Column<bool>(type: "bit", nullable: true),
                    DisabilityHasConsciousnessLossRisk = table.Column<bool>(type: "bit", nullable: true),
                    CareerAdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCvs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsNativeLanguage = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateLanguages_CandidateCvContents_CandidateCvContentId",
                        column: x => x.CandidateCvContentId,
                        principalTable: "CandidateCvContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateReferences_CandidateCvContents_CandidateCvContentId",
                        column: x => x.CandidateCvContentId,
                        principalTable: "CandidateCvContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IssuingInstitution = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CertificateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_CandidateCvContents_CandidateCvContentId",
                        column: x => x.CandidateCvContentId,
                        principalTable: "CandidateCvContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Educations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EducationLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DiplomaGradingSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiplomaGrade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolNameFreeText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Educations_CandidateCvContents_CandidateCvContentId",
                        column: x => x.CandidateCvContentId,
                        principalTable: "CandidateCvContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrentJob = table.Column<bool>(type: "bit", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiences_CandidateCvContents_CandidateCvContentId",
                        column: x => x.CandidateCvContentId,
                        principalTable: "CandidateCvContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateCvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialMediaLinks_CandidateCvs_CandidateCvId",
                        column: x => x.CandidateCvId,
                        principalTable: "CandidateCvs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvContents_CandidateCvId",
                table: "CandidateCvContents",
                column: "CandidateCvId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCvs_UserId",
                table: "CandidateCvs",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateLanguages_CandidateCvContentId",
                table: "CandidateLanguages",
                column: "CandidateCvContentId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReferences_CandidateCvContentId",
                table: "CandidateReferences",
                column: "CandidateCvContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CandidateCvContentId",
                table: "Certificates",
                column: "CandidateCvContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Educations_CandidateCvContentId",
                table: "Educations",
                column: "CandidateCvContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_CandidateCvContentId",
                table: "Experiences",
                column: "CandidateCvContentId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaLinks_CandidateCvId",
                table: "SocialMediaLinks",
                column: "CandidateCvId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateLanguages");

            migrationBuilder.DropTable(
                name: "CandidateReferences");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Educations");

            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropTable(
                name: "SocialMediaLinks");

            migrationBuilder.DropTable(
                name: "CandidateCvContents");

            migrationBuilder.DropTable(
                name: "CandidateCvs");
        }
    }
}
