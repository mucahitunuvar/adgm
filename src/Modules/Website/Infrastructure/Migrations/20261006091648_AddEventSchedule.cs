using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OnlineLink = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    RegistrationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationOpensAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationClosesAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinAge = table.Column<int>(type: "int", nullable: true),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    AutoConfirm = table.Column<bool>(type: "bit", nullable: false),
                    WaitlistEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedCount = table.Column<int>(type: "int", nullable: false),
                    WaitlistedCount = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventScheduleTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VenueAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FeeInfo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Instructors = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProgramFlow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessibilityNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EventScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventScheduleTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventScheduleTranslations_EventSchedules_EventScheduleId",
                        column: x => x.EventScheduleId,
                        principalTable: "EventSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSchedules_ContentItemId",
                table: "EventSchedules",
                column: "ContentItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventScheduleTranslations_EventScheduleId_LanguageCode",
                table: "EventScheduleTranslations",
                columns: new[] { "EventScheduleId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventScheduleTranslations");

            migrationBuilder.DropTable(
                name: "EventSchedules");
        }
    }
}
