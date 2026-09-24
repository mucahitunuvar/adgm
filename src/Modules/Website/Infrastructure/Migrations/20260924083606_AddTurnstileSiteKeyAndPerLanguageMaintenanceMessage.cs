using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnstileSiteKeyAndPerLanguageMaintenanceMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintenanceMessage",
                table: "SiteSettings");

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceMessage",
                table: "SiteSettingsTranslations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TurnstileSiteKey",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintenanceMessage",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "TurnstileSiteKey",
                table: "SiteSettings");

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceMessage",
                table: "SiteSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
