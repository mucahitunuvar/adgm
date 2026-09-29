using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentGalleryVideosAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoIds",
                table: "ContentItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "ContentItemAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItemAttachments_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItemGalleryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemGalleryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItemGalleryItems_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItemAttachmentTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    DisplayNameOverride = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContentItemAttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemAttachmentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItemAttachmentTranslations_ContentItemAttachments_ContentItemAttachmentId",
                        column: x => x.ContentItemAttachmentId,
                        principalTable: "ContentItemAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItemGalleryItemTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    AltTextOverride = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CaptionOverride = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContentItemGalleryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemGalleryItemTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItemGalleryItemTranslations_ContentItemGalleryItems_ContentItemGalleryItemId",
                        column: x => x.ContentItemGalleryItemId,
                        principalTable: "ContentItemGalleryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemAttachments_ContentItemId",
                table: "ContentItemAttachments",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemAttachmentTranslations_ContentItemAttachmentId_LanguageCode",
                table: "ContentItemAttachmentTranslations",
                columns: new[] { "ContentItemAttachmentId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemGalleryItems_ContentItemId",
                table: "ContentItemGalleryItems",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemGalleryItemTranslations_ContentItemGalleryItemId_LanguageCode",
                table: "ContentItemGalleryItemTranslations",
                columns: new[] { "ContentItemGalleryItemId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentItemAttachmentTranslations");

            migrationBuilder.DropTable(
                name: "ContentItemGalleryItemTranslations");

            migrationBuilder.DropTable(
                name: "ContentItemAttachments");

            migrationBuilder.DropTable(
                name: "ContentItemGalleryItems");

            migrationBuilder.DropColumn(
                name: "VideoIds",
                table: "ContentItems");
        }
    }
}
