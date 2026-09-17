using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminManagedStartingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("36079b50-2960-c876-3181-ed6145fd5e3b"), "EUR", "Euro", true, 2 },
                    { new Guid("a4140ef9-35d1-a389-0052-da8d76adc325"), "USD", "Amerikan Doları", true, 1 },
                    { new Guid("d2b3605f-b349-2017-cabf-1bd1ab9bd69d"), "TRY", "Türk Lirası", true, 0 }
                });

            migrationBuilder.InsertData(
                table: "DiplomaGradingSystems",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("25b70796-a8c7-d016-d11a-a1d7c3ea2b47"), "SCALE_4", "4'lük Sistem", true, 0 },
                    { new Guid("4b9636ba-4981-bd33-182c-345df29cfb01"), "SCALE_100", "100'lük Sistem", true, 1 }
                });

            migrationBuilder.InsertData(
                table: "DisabilityCategories",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("180db52f-62cb-6cd3-2e73-12ef5a1fe372"), "ORTHOPEDIC", "Ortopedik Engel", true, 2 },
                    { new Guid("524ba622-4a04-f51d-b73a-80b9410676ed"), "CHRONIC_ILLNESS", "Süreğen Hastalık", true, 5 },
                    { new Guid("957e0b71-f8d4-7c1f-0258-0697685c79f7"), "HEARING", "İşitme Engeli", true, 1 },
                    { new Guid("95d8b2c1-ad5a-c2d7-16a4-c722112b2922"), "VISUAL", "Görme Engeli", true, 0 },
                    { new Guid("cb54671a-333e-c7a0-84a1-092b99842abb"), "MENTAL_EMOTIONAL", "Ruhsal ve Duygusal Engel", true, 4 },
                    { new Guid("dd29f8b7-762b-9de1-40a0-2ae0cfcb1fa2"), "INTELLECTUAL", "Zihinsel Engel", true, 3 },
                    { new Guid("fe795048-7dfd-8018-4cd0-b0179b816aa8"), "SPEECH_LANGUAGE", "Dil ve Konuşma Güçlüğü", true, 6 }
                });

            migrationBuilder.InsertData(
                table: "EmploymentTypes",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("8d13bc0f-9a61-6142-54c7-45e3ec18a311"), "FULL_TIME", "Tam Zamanlı", true, 0 },
                    { new Guid("dd879bbb-34cb-f1ae-e3af-2941010bfa9b"), "PART_TIME", "Yarı Zamanlı", true, 1 }
                });

            migrationBuilder.InsertData(
                table: "ReferenceTypes",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("1df36615-39e6-2ea0-e74e-9ac219d70f7a"), "COLLEAGUE", "İş Arkadaşı", true, 1 },
                    { new Guid("342957c1-4d1a-6487-ea23-b7c9acfe6c3a"), "ACADEMIC", "Akademisyen / Öğretim Görevlisi", true, 2 },
                    { new Guid("661bb0cf-c646-d837-c7be-115bd197c543"), "CLIENT", "Müşteri", true, 3 },
                    { new Guid("983e38f6-800f-34ae-9270-cd44ac06164f"), "MANAGER", "Yönetici / Amir", true, 0 },
                    { new Guid("d0506695-b6de-2242-0100-c5b6f22da422"), "OTHER", "Diğer", true, 4 }
                });

            migrationBuilder.InsertData(
                table: "SchoolCategories",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("10ce2afc-9383-71d7-ba49-3b7bf451cf6e"), "HIGH_SCHOOL", "Lise", true, 2 },
                    { new Guid("a6f3639a-7f1b-f965-28bd-11ef6cb2e3b1"), "PRIMARY", "İlkokul", true, 0 },
                    { new Guid("a8cae304-9188-dbdd-8351-e4ca2039c057"), "UNIVERSITY", "Üniversite", true, 3 },
                    { new Guid("f354dfdf-42a8-8607-2dbd-071d119fe962"), "MIDDLE", "Ortaokul", true, 1 }
                });

            migrationBuilder.InsertData(
                table: "WorkLocationTypes",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("8417e5e9-9e50-c7ee-b250-30b12578c006"), "REMOTE", "Uzaktan", true, 0 },
                    { new Guid("8afd22ec-de87-97dd-6a88-8f82e71f838c"), "OFFICE", "Ofis", true, 2 },
                    { new Guid("c373ad42-02cc-bb44-fe5e-ebe6405d947d"), "HYBRID", "Hibrit", true, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("36079b50-2960-c876-3181-ed6145fd5e3b"));

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("a4140ef9-35d1-a389-0052-da8d76adc325"));

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("d2b3605f-b349-2017-cabf-1bd1ab9bd69d"));

            migrationBuilder.DeleteData(
                table: "DiplomaGradingSystems",
                keyColumn: "Id",
                keyValue: new Guid("25b70796-a8c7-d016-d11a-a1d7c3ea2b47"));

            migrationBuilder.DeleteData(
                table: "DiplomaGradingSystems",
                keyColumn: "Id",
                keyValue: new Guid("4b9636ba-4981-bd33-182c-345df29cfb01"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("180db52f-62cb-6cd3-2e73-12ef5a1fe372"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("524ba622-4a04-f51d-b73a-80b9410676ed"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("957e0b71-f8d4-7c1f-0258-0697685c79f7"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("95d8b2c1-ad5a-c2d7-16a4-c722112b2922"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("cb54671a-333e-c7a0-84a1-092b99842abb"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("dd29f8b7-762b-9de1-40a0-2ae0cfcb1fa2"));

            migrationBuilder.DeleteData(
                table: "DisabilityCategories",
                keyColumn: "Id",
                keyValue: new Guid("fe795048-7dfd-8018-4cd0-b0179b816aa8"));

            migrationBuilder.DeleteData(
                table: "EmploymentTypes",
                keyColumn: "Id",
                keyValue: new Guid("8d13bc0f-9a61-6142-54c7-45e3ec18a311"));

            migrationBuilder.DeleteData(
                table: "EmploymentTypes",
                keyColumn: "Id",
                keyValue: new Guid("dd879bbb-34cb-f1ae-e3af-2941010bfa9b"));

            migrationBuilder.DeleteData(
                table: "ReferenceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1df36615-39e6-2ea0-e74e-9ac219d70f7a"));

            migrationBuilder.DeleteData(
                table: "ReferenceTypes",
                keyColumn: "Id",
                keyValue: new Guid("342957c1-4d1a-6487-ea23-b7c9acfe6c3a"));

            migrationBuilder.DeleteData(
                table: "ReferenceTypes",
                keyColumn: "Id",
                keyValue: new Guid("661bb0cf-c646-d837-c7be-115bd197c543"));

            migrationBuilder.DeleteData(
                table: "ReferenceTypes",
                keyColumn: "Id",
                keyValue: new Guid("983e38f6-800f-34ae-9270-cd44ac06164f"));

            migrationBuilder.DeleteData(
                table: "ReferenceTypes",
                keyColumn: "Id",
                keyValue: new Guid("d0506695-b6de-2242-0100-c5b6f22da422"));

            migrationBuilder.DeleteData(
                table: "SchoolCategories",
                keyColumn: "Id",
                keyValue: new Guid("10ce2afc-9383-71d7-ba49-3b7bf451cf6e"));

            migrationBuilder.DeleteData(
                table: "SchoolCategories",
                keyColumn: "Id",
                keyValue: new Guid("a6f3639a-7f1b-f965-28bd-11ef6cb2e3b1"));

            migrationBuilder.DeleteData(
                table: "SchoolCategories",
                keyColumn: "Id",
                keyValue: new Guid("a8cae304-9188-dbdd-8351-e4ca2039c057"));

            migrationBuilder.DeleteData(
                table: "SchoolCategories",
                keyColumn: "Id",
                keyValue: new Guid("f354dfdf-42a8-8607-2dbd-071d119fe962"));

            migrationBuilder.DeleteData(
                table: "WorkLocationTypes",
                keyColumn: "Id",
                keyValue: new Guid("8417e5e9-9e50-c7ee-b250-30b12578c006"));

            migrationBuilder.DeleteData(
                table: "WorkLocationTypes",
                keyColumn: "Id",
                keyValue: new Guid("8afd22ec-de87-97dd-6a88-8f82e71f838c"));

            migrationBuilder.DeleteData(
                table: "WorkLocationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c373ad42-02cc-bb44-fe5e-ebe6405d947d"));
        }
    }
}
