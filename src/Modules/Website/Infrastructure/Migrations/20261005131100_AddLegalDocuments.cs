using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocumentTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocumentTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDocumentTranslations_LegalDocuments_LegalDocumentId",
                        column: x => x.LegalDocumentId,
                        principalTable: "LegalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocumentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangeSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LegalDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDocumentVersions_LegalDocuments_LegalDocumentId",
                        column: x => x.LegalDocumentId,
                        principalTable: "LegalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocumentVersionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalDocumentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocumentVersionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDocumentVersionTranslations_LegalDocumentVersions_LegalDocumentVersionId",
                        column: x => x.LegalDocumentVersionId,
                        principalTable: "LegalDocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_Key",
                table: "LegalDocuments",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentTranslations_LegalDocumentId_LanguageCode",
                table: "LegalDocumentTranslations",
                columns: new[] { "LegalDocumentId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentVersions_LegalDocumentId_VersionNumber",
                table: "LegalDocumentVersions",
                columns: new[] { "LegalDocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocumentVersionTranslations_LegalDocumentVersionId_LanguageCode",
                table: "LegalDocumentVersionTranslations",
                columns: new[] { "LegalDocumentVersionId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalDocumentTranslations");

            migrationBuilder.DropTable(
                name: "LegalDocumentVersionTranslations");

            migrationBuilder.DropTable(
                name: "LegalDocumentVersions");

            migrationBuilder.DropTable(
                name: "LegalDocuments");
        }
    }
}
