using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "Companies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoFileKey",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoOriginalFileName",
                table: "Companies",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoOwnerEntityId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoOwnerEntityType",
                table: "Companies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LogoSizeInBytes",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogoUploadedAtUtc",
                table: "Companies",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoFileKey",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoOriginalFileName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoOwnerEntityId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoOwnerEntityType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoSizeInBytes",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoUploadedAtUtc",
                table: "Companies");
        }
    }
}
