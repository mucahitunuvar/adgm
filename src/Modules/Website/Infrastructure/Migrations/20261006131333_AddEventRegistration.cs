using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventPrivacyNoticeKey",
                table: "SiteSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcceptedPrivacyNoticeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcceptedPrivacyNoticeVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WaitlistedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VerificationTokenHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VerificationTokenExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastVerificationEmailSentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnonymizedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRegistrationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrationStatusHistory_EventRegistrations_EventRegistrationId",
                        column: x => x.EventRegistrationId,
                        principalTable: "EventRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_CancelToken",
                table: "EventRegistrations",
                column: "CancelToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_ContentItemId",
                table: "EventRegistrations",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_ContentItemId_Email",
                table: "EventRegistrations",
                columns: new[] { "ContentItemId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_Status",
                table: "EventRegistrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_VerificationTokenHash",
                table: "EventRegistrations",
                column: "VerificationTokenHash",
                unique: true,
                filter: "[VerificationTokenHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrationStatusHistory_EventRegistrationId",
                table: "EventRegistrationStatusHistory",
                column: "EventRegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrationStatusHistory");

            migrationBuilder.DropTable(
                name: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "EventPrivacyNoticeKey",
                table: "SiteSettings");
        }
    }
}
