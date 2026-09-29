using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagIds",
                table: "ContentItemTranslations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CategoryIds",
                table: "ContentItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "ContentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_ContentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentCategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeoMetaTitle = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    SeoMetaDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SeoMetaKeywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SeoOgTitle = table.Column<string>(type: "nvarchar(95)", maxLength: 95, nullable: false),
                    SeoOgDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeoOgImageMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeoCanonicalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SeoNoIndex = table.Column<bool>(type: "bit", nullable: false),
                    ContentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentCategoryTranslations_ContentCategories_ContentCategoryId",
                        column: x => x.ContentCategoryId,
                        principalTable: "ContentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentCategories_ContentTypeId",
                table: "ContentCategories",
                column: "ContentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCategoryTranslations_ContentCategoryId_LanguageCode",
                table: "ContentCategoryTranslations",
                columns: new[] { "ContentCategoryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentTags_LanguageCode_Slug",
                table: "ContentTags",
                columns: new[] { "LanguageCode", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentCategoryTranslations");

            migrationBuilder.DropTable(
                name: "ContentTags");

            migrationBuilder.DropTable(
                name: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "TagIds",
                table: "ContentItemTranslations");

            migrationBuilder.DropColumn(
                name: "CategoryIds",
                table: "ContentItems");
        }
    }
}
