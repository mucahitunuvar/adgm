using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowSearchEngineIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Defaults existing rows to true (SiteSettings.AllowSearchEngineIndexing's own field
            // initializer default) - an already-deployed site was indexable before this column existed,
            // and this migration must not silently flip it to "disallow everything".
            migrationBuilder.AddColumn<bool>(
                name: "AllowSearchEngineIndexing",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowSearchEngineIndexing",
                table: "SiteSettings");
        }
    }
}
