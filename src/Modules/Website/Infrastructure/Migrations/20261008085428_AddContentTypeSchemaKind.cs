using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeSchemaKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchemaKind",
                table: "ContentTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("1e80f1f3-1ed3-e8f6-42ae-e89322662343"),
                column: "SchemaKind",
                value: "None");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("21d083a6-cbf8-d3ff-aea8-1c0fddae5ed0"),
                column: "SchemaKind",
                value: "None");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("27800450-ec5d-46eb-9628-0259f12d4ade"),
                column: "SchemaKind",
                value: "FaqPage");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("2e76848a-82a5-0370-a632-de4fc1d4e97a"),
                column: "SchemaKind",
                value: "None");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("556494b9-fe9a-55fe-1dc7-d22cdebc8469"),
                column: "SchemaKind",
                value: "NewsArticle");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("5ada11b7-3892-d01a-3a9d-06cf9fb7a779"),
                column: "SchemaKind",
                value: "None");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("97549a0c-9911-a5b4-3c2b-8c14220ed6e6"),
                column: "SchemaKind",
                value: "NewsArticle");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("9c144b0e-3872-3279-cb81-d767b94cce44"),
                column: "SchemaKind",
                value: "Article");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("a1924238-fa63-32bd-dcd5-3f8c1d477a72"),
                column: "SchemaKind",
                value: "NewsArticle");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d24c0018-525a-9148-c09d-5cb607d3b387"),
                column: "SchemaKind",
                value: "Article");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d30550f5-b105-d0cf-4bed-ad582a2460bf"),
                column: "SchemaKind",
                value: "None");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("db1c7e26-fc6e-0178-fb5a-10bec802139c"),
                column: "SchemaKind",
                value: "Article");

            migrationBuilder.UpdateData(
                table: "ContentTypes",
                keyColumn: "Id",
                keyValue: new Guid("f3c481f9-e264-bab5-9d1c-b08a5735cac9"),
                column: "SchemaKind",
                value: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchemaKind",
                table: "ContentTypes");
        }
    }
}
