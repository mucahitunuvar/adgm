using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSliders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sliders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sliders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SliderTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SliderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SliderTranslations_Sliders_SliderId",
                        column: x => x.SliderId,
                        principalTable: "Sliders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesktopImageMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MobileImageMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LinkContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkContentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkInternalPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublishAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnpublishAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SliderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slides_Sliders_SliderId",
                        column: x => x.SliderId,
                        principalTable: "Sliders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlideTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Eyebrow = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ButtonLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltTextOverride = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SlideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlideTranslations_Slides_SlideId",
                        column: x => x.SlideId,
                        principalTable: "Slides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sliders_Key",
                table: "Sliders",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SliderTranslations_SliderId_LanguageCode",
                table: "SliderTranslations",
                columns: new[] { "SliderId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slides_SliderId",
                table: "Slides",
                column: "SliderId");

            migrationBuilder.CreateIndex(
                name: "IX_SlideTranslations_SlideId_LanguageCode",
                table: "SlideTranslations",
                columns: new[] { "SlideId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SliderTranslations");

            migrationBuilder.DropTable(
                name: "SlideTranslations");

            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "Sliders");
        }
    }
}
