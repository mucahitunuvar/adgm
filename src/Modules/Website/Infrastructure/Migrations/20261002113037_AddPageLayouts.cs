using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPageLayouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HasUnpublishedChanges = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageLayoutDraftBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockTypeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayoutDraftBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageLayoutDraftBlocks_PageLayouts_PageLayoutId",
                        column: x => x.PageLayoutId,
                        principalTable: "PageLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageLayoutPublishedBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockTypeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayoutPublishedBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageLayoutPublishedBlocks_PageLayouts_PageLayoutId",
                        column: x => x.PageLayoutId,
                        principalTable: "PageLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageLayoutDraftBlockTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    TextsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LayoutBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayoutDraftBlockTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageLayoutDraftBlockTranslations_PageLayoutDraftBlocks_LayoutBlockId",
                        column: x => x.LayoutBlockId,
                        principalTable: "PageLayoutDraftBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageLayoutPublishedBlockTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    TextsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LayoutBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayoutPublishedBlockTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageLayoutPublishedBlockTranslations_PageLayoutPublishedBlocks_LayoutBlockId",
                        column: x => x.LayoutBlockId,
                        principalTable: "PageLayoutPublishedBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PageLayouts",
                columns: new[] { "Id", "ContentItemId", "CreatedAtUtc", "CreatedByUserId", "HasUnpublishedChanges", "PublishedAtUtc", "PublishedByUserId", "RowVersion", "TargetKind", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { new Guid("32f97881-23f2-6a71-abbd-ebb7300e8d56"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), false, null, null, new byte[] { 84, 145, 72, 93, 5, 57, 150, 227, 92, 180, 53, 63, 104, 4, 60, 150 }, "Home", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_PageLayoutDraftBlocks_PageLayoutId",
                table: "PageLayoutDraftBlocks",
                column: "PageLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayoutDraftBlockTranslations_LayoutBlockId_LanguageCode",
                table: "PageLayoutDraftBlockTranslations",
                columns: new[] { "LayoutBlockId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageLayoutPublishedBlocks_PageLayoutId",
                table: "PageLayoutPublishedBlocks",
                column: "PageLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayoutPublishedBlockTranslations_LayoutBlockId_LanguageCode",
                table: "PageLayoutPublishedBlockTranslations",
                columns: new[] { "LayoutBlockId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageLayouts_ContentItemId",
                table: "PageLayouts",
                column: "ContentItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageLayoutDraftBlockTranslations");

            migrationBuilder.DropTable(
                name: "PageLayoutPublishedBlockTranslations");

            migrationBuilder.DropTable(
                name: "PageLayoutDraftBlocks");

            migrationBuilder.DropTable(
                name: "PageLayoutPublishedBlocks");

            migrationBuilder.DropTable(
                name: "PageLayouts");
        }
    }
}
