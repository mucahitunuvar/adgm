using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClearStaleNotFoundLogsForPathNormalizationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data-only fix: rows written before this fix may hold a raw, language-prefixed Path
            // (e.g. "/en/xyz" logged under LanguageCode "en") instead of the normalized,
            // language-stripped form the resolver now records ("xyz") and Redirect.FromPath expects.
            // NotFoundLog is a disposable hit counter, not source-of-truth data - visits to a genuinely
            // still-missing path simply recreate the row in the correct format, so the safe fix is to
            // clear every existing row rather than attempt an in-place rewrite.
            migrationBuilder.Sql("DELETE FROM [NotFoundLogs];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deleted rows were disposable hit counters (see Up) - nothing to restore.
        }
    }
}
