using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPopups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Popups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImageMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LinkContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkContentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkInternalPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TargetingKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetingContentItemIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetingPaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceTarget = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnpublishAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DelaySeconds = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrequencyDays = table.Column<int>(type: "int", nullable: true),
                    Dismissible = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Popups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PopupTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PopupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopupTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PopupTranslations_Popups_PopupId",
                        column: x => x.PopupId,
                        principalTable: "Popups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PopupTranslations_PopupId_LanguageCode",
                table: "PopupTranslations",
                columns: new[] { "PopupId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PopupTranslations");

            migrationBuilder.DropTable(
                name: "Popups");
        }
    }
}
