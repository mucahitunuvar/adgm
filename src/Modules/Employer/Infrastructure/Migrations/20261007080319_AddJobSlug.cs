using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Jobs",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Slug",
                table: "Jobs",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_Slug",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Jobs");
        }
    }
}
