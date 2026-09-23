using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteLanguages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteLanguages",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedByUserId", "IsActive", "IsDefault", "Name", "SortOrder", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("a25a5495-704f-8605-9105-af910a4e37d5"), "en", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), false, false, "English", 2, null, null },
                    { new Guid("b20fca3c-d17b-8010-fcd8-bd94e7db0ca4"), "tr", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), true, true, "Türkçe", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteLanguages_Code",
                table: "SiteLanguages",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteLanguages");
        }
    }
}
