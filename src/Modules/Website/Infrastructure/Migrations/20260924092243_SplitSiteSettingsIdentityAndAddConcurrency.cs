using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitSiteSettingsIdentityAndAddConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: DefaultSeoTitle is dropped and Tagline added separately (not renamed) even
            // though the scaffolded diff first proposed a rename - they are different fields
            // (tagline vs. SEO title), not the same field under a new name. No production data
            // exists yet for this Faz 0 aggregate, so there is nothing to preserve either way.
            migrationBuilder.DropColumn(
                name: "DefaultSeoTitle",
                table: "SiteSettingsTranslations");

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "SiteSettingsTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.RenameColumn(
                name: "DefaultSeoDescription",
                table: "SiteSettingsTranslations",
                newName: "DefaultMetaDescription");

            migrationBuilder.RenameColumn(
                name: "ThemeLogoLightMediaAssetId",
                table: "SiteSettings",
                newName: "LogoLightMediaAssetId");

            migrationBuilder.RenameColumn(
                name: "ThemeLogoDarkMediaAssetId",
                table: "SiteSettings",
                newName: "LogoDarkMediaAssetId");

            migrationBuilder.RenameColumn(
                name: "ThemeFaviconMediaAssetId",
                table: "SiteSettings",
                newName: "FaviconMediaAssetId");

            migrationBuilder.AddColumn<string>(
                name: "DefaultMetaTitle",
                table: "SiteSettingsTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "SiteSettingsBankAccounts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultOgImageMediaId",
                table: "SiteSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SiteSettings",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultMetaTitle",
                table: "SiteSettingsTranslations");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SiteSettingsBankAccounts");

            migrationBuilder.DropColumn(
                name: "DefaultOgImageMediaId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "SiteSettingsTranslations");

            migrationBuilder.AddColumn<string>(
                name: "DefaultSeoTitle",
                table: "SiteSettingsTranslations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.RenameColumn(
                name: "DefaultMetaDescription",
                table: "SiteSettingsTranslations",
                newName: "DefaultSeoDescription");

            migrationBuilder.RenameColumn(
                name: "LogoLightMediaAssetId",
                table: "SiteSettings",
                newName: "ThemeLogoLightMediaAssetId");

            migrationBuilder.RenameColumn(
                name: "LogoDarkMediaAssetId",
                table: "SiteSettings",
                newName: "ThemeLogoDarkMediaAssetId");

            migrationBuilder.RenameColumn(
                name: "FaviconMediaAssetId",
                table: "SiteSettings",
                newName: "ThemeFaviconMediaAssetId");
        }
    }
}
