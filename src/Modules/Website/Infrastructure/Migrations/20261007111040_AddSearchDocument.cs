using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    TypeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncludeInSitemap = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchSourceStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastStartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSucceededAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSourceStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_LanguageCode_TypeKey",
                table: "SearchDocuments",
                columns: new[] { "LanguageCode", "TypeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_PublishedAtUtc",
                table: "SearchDocuments",
                column: "PublishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_SourceKey_SourceId_LanguageCode",
                table: "SearchDocuments",
                columns: new[] { "SourceKey", "SourceId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchSourceStates_SourceKey",
                table: "SearchSourceStates",
                column: "SourceKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchDocuments");

            migrationBuilder.DropTable(
                name: "SearchSourceStates");
        }
    }
}
