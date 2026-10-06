using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThirdPartyScriptsAndCookieConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CookieBannerText",
                table: "SiteSettingsTranslations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CookieBannerTitle",
                table: "SiteSettingsTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CookieCategoryAnalyticsDescription",
                table: "SiteSettingsTranslations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CookieCategoryMarketingDescription",
                table: "SiteSettingsTranslations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CookieCategoryNecessaryDescription",
                table: "SiteSettingsTranslations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CookiePolicyKey",
                table: "SiteSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CookieConsentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Categories = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolicyKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PolicyVersion = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CookieConsentRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyScripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MeasurementId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContainerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PixelId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Src = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Async = table.Column<bool>(type: "bit", nullable: false),
                    Defer = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Placement = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyScripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyScriptTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ThirdPartyScriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyScriptTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyScriptTranslations_ThirdPartyScripts_ThirdPartyScriptId",
                        column: x => x.ThirdPartyScriptId,
                        principalTable: "ThirdPartyScripts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CookieConsentRecords_ConsentId",
                table: "CookieConsentRecords",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_CookieConsentRecords_RecordedAtUtc",
                table: "CookieConsentRecords",
                column: "RecordedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyScriptTranslations_ThirdPartyScriptId_LanguageCode",
                table: "ThirdPartyScriptTranslations",
                columns: new[] { "ThirdPartyScriptId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CookieConsentRecords");

            migrationBuilder.DropTable(
                name: "ThirdPartyScriptTranslations");

            migrationBuilder.DropTable(
                name: "ThirdPartyScripts");

            migrationBuilder.DropColumn(
                name: "CookieBannerText",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "CookieBannerTitle",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "CookieCategoryAnalyticsDescription",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "CookieCategoryMarketingDescription",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "CookieCategoryNecessaryDescription",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "CookiePolicyKey",
                table: "SiteSettings");
        }
    }
}
