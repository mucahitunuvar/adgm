using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LinkKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LinkContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkContentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkInternalPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OpenInNewTab = table.Column<bool>(type: "bit", nullable: false),
                    IconKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemTranslations_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedByUserId", "Location", "RowVersion", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("5c91bc6e-f0c8-9cf7-7fd1-54bbdba4411f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Header", new byte[] { 232, 0, 72, 230, 28, 215, 49, 106, 14, 9, 190, 126, 165, 84, 3, 161 }, null, null },
                    { new Guid("923791e2-28cd-b330-f215-a83d3e87621f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Utility", new byte[] { 217, 33, 176, 168, 225, 235, 219, 127, 223, 228, 240, 52, 13, 230, 87, 92 }, null, null },
                    { new Guid("c404fec9-1c2e-1259-d822-a1cad7dbca3a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "Footer", new byte[] { 5, 172, 104, 38, 29, 23, 83, 111, 50, 68, 126, 151, 19, 104, 25, 45 }, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuId_ParentId",
                table: "MenuItems",
                columns: new[] { "MenuId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemTranslations_MenuItemId_LanguageCode",
                table: "MenuItemTranslations",
                columns: new[] { "MenuItemId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_Location",
                table: "Menus",
                column: "Location",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemTranslations");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
