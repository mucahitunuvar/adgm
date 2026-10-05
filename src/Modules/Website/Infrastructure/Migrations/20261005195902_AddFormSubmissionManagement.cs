using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormSubmissionManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnonymizedAtUtc",
                table: "FormSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveEligibleSinceUtc",
                table: "FormSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "FormSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "FormSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAtUtc",
                table: "FormSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormSubmissionInternalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissionInternalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissionInternalNotes_FormSubmissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "FormSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmissionPendingFileDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissionPendingFileDeletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissionPendingFileDeletions_FormSubmissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "FormSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmissionStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissionStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissionStatusHistory_FormSubmissions_FormSubmissionId",
                        column: x => x.FormSubmissionId,
                        principalTable: "FormSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalDataAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDataAccessLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_ArchivedAtUtc",
                table: "FormSubmissions",
                column: "ArchivedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AssignedToUserId",
                table: "FormSubmissions",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionInternalNotes_FormSubmissionId",
                table: "FormSubmissionInternalNotes",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionPendingFileDeletions_FormSubmissionId",
                table: "FormSubmissionPendingFileDeletions",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionStatusHistory_FormSubmissionId",
                table: "FormSubmissionStatusHistory",
                column: "FormSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDataAccessLogs_AccessedAtUtc",
                table: "PersonalDataAccessLogs",
                column: "AccessedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormSubmissionInternalNotes");

            migrationBuilder.DropTable(
                name: "FormSubmissionPendingFileDeletions");

            migrationBuilder.DropTable(
                name: "FormSubmissionStatusHistory");

            migrationBuilder.DropTable(
                name: "PersonalDataAccessLogs");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_ArchivedAtUtc",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AssignedToUserId",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "AnonymizedAtUtc",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ArchiveEligibleSinceUtc",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                table: "FormSubmissions");
        }
    }
}
