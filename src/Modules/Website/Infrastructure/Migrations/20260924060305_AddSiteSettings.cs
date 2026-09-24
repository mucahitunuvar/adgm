using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThemeLogoLightMediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThemeLogoDarkMediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThemeFaviconMediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThemePrimaryColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ThemeSecondaryColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ThemeFontFamily = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactWhatsApp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactMapEmbedUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GlobalSearchEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NewsletterEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PublicJobListingsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DonationPageEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BotProtectionEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceModeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettingsBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountHolder = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettingsBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettingsBankAccounts_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettingsSocialLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettingsSocialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettingsSocialLinks_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettingsTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefaultSeoTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultSeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FooterText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettingsTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettingsTranslations_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettingsBankAccounts_SiteSettingsId",
                table: "SiteSettingsBankAccounts",
                column: "SiteSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettingsSocialLinks_SiteSettingsId",
                table: "SiteSettingsSocialLinks",
                column: "SiteSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettingsTranslations_SiteSettingsId_LanguageCode",
                table: "SiteSettingsTranslations",
                columns: new[] { "SiteSettingsId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettingsBankAccounts");

            migrationBuilder.DropTable(
                name: "SiteSettingsSocialLinks");

            migrationBuilder.DropTable(
                name: "SiteSettingsTranslations");

            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}
