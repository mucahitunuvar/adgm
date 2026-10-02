using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnPageLayoutTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PageLayouts_ContentItemId",
                table: "PageLayouts");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayouts_TargetKind_ContentItemId",
                table: "PageLayouts",
                columns: new[] { "TargetKind", "ContentItemId" },
                unique: true,
                filter: "[ContentItemId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PageLayouts_TargetKind_ContentItemId",
                table: "PageLayouts");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayouts_ContentItemId",
                table: "PageLayouts",
                column: "ContentItemId");
        }
    }
}
