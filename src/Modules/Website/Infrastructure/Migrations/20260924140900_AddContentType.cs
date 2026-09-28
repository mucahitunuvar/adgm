using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.Website.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListTemplate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetailTemplate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SupportsHierarchy = table.Column<bool>(type: "bit", nullable: false),
                    SupportsCategories = table.Column<bool>(type: "bit", nullable: false),
                    SupportsTags = table.Column<bool>(type: "bit", nullable: false),
                    SupportsDetailImage = table.Column<bool>(type: "bit", nullable: false),
                    SupportsGallery = table.Column<bool>(type: "bit", nullable: false),
                    SupportsVideos = table.Column<bool>(type: "bit", nullable: false),
                    SupportsAttachments = table.Column<bool>(type: "bit", nullable: false),
                    SupportsEvent = table.Column<bool>(type: "bit", nullable: false),
                    SupportsBlockLayout = table.Column<bool>(type: "bit", nullable: false),
                    SupportsForm = table.Column<bool>(type: "bit", nullable: false),
                    SupportsRelatedContent = table.Column<bool>(type: "bit", nullable: false),
                    HasDetailPage = table.Column<bool>(type: "bit", nullable: false),
                    HasListingPage = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    RequiresReview = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoutePrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeoMetaTitle = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    SeoMetaDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SeoMetaKeywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SeoOgTitle = table.Column<string>(type: "nvarchar(95)", maxLength: 95, nullable: false),
                    SeoOgDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeoOgImageMediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeoCanonicalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SeoNoIndex = table.Column<bool>(type: "bit", nullable: false),
                    ContentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentTypeTranslations_ContentTypes_ContentTypeId",
                        column: x => x.ContentTypeId,
                        principalTable: "ContentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ContentTypes",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedByUserId", "DetailTemplate", "HasDetailPage", "HasListingPage", "IsActive", "IsSearchable", "Key", "ListTemplate", "RequiresReview", "RowVersion", "SortMode", "SortOrder", "SupportsAttachments", "SupportsBlockLayout", "SupportsCategories", "SupportsDetailImage", "SupportsEvent", "SupportsForm", "SupportsGallery", "SupportsHierarchy", "SupportsRelatedContent", "SupportsTags", "SupportsVideos", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("1e80f1f3-1ed3-e8f6-42ae-e89322662343"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "document", false, true, true, true, "document", "document-list", false, new byte[] { 235, 118, 237, 228, 24, 216, 76, 81, 227, 193, 41, 122, 245, 149, 98, 67 }, "PublishDateDesc", 12, true, false, true, false, false, false, false, false, false, false, false, null, null },
                    { new Guid("21d083a6-cbf8-d3ff-aea8-1c0fddae5ed0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "event", true, true, true, true, "training", "cards", false, new byte[] { 105, 219, 9, 255, 165, 130, 55, 227, 136, 226, 137, 54, 70, 87, 114, 59 }, "EventDateAsc", 8, true, false, true, true, true, false, true, false, true, true, true, null, null },
                    { new Guid("27800450-ec5d-46eb-9628-0259f12d4ade"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "faq", false, true, true, true, "faq", "faq-accordion", false, new byte[] { 78, 86, 9, 144, 137, 60, 119, 63, 225, 9, 74, 55, 27, 137, 30, 180 }, "Manual", 10, false, false, true, false, false, false, false, false, false, false, false, null, null },
                    { new Guid("2e76848a-82a5-0370-a632-de4fc1d4e97a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "event", true, true, true, true, "event", "cards", false, new byte[] { 24, 47, 180, 79, 109, 169, 170, 120, 68, 126, 115, 116, 250, 146, 189, 155 }, "EventDateAsc", 7, true, false, true, true, true, false, true, false, true, true, true, null, null },
                    { new Guid("556494b9-fe9a-55fe-1dc7-d22cdebc8469"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "article", true, true, true, true, "news", "cards", false, new byte[] { 110, 162, 123, 88, 135, 57, 212, 244, 30, 89, 199, 2, 153, 244, 145, 33 }, "PublishDateDesc", 2, true, false, true, true, false, false, true, false, true, true, true, null, null },
                    { new Guid("5ada11b7-3892-d01a-3a9d-06cf9fb7a779"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "opportunity", true, true, true, true, "volunteer-opportunity", "cards", false, new byte[] { 224, 14, 179, 97, 26, 175, 157, 184, 43, 182, 197, 180, 186, 233, 116, 29 }, "PublishDateDesc", 9, false, false, true, true, false, true, false, false, true, false, false, null, null },
                    { new Guid("97549a0c-9911-a5b4-3c2b-8c14220ed6e6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "article", true, true, true, true, "press-release", "list", false, new byte[] { 52, 95, 197, 61, 146, 152, 220, 13, 36, 217, 113, 249, 46, 40, 137, 38 }, "PublishDateDesc", 13, true, false, false, true, false, false, false, false, true, false, false, null, null },
                    { new Guid("9c144b0e-3872-3279-cb81-d767b94cce44"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "article", true, true, true, true, "activity", "cards", false, new byte[] { 86, 25, 223, 35, 58, 101, 123, 86, 24, 14, 79, 5, 89, 8, 99, 32 }, "PublishDateDesc", 5, false, false, true, true, false, false, true, false, true, true, true, null, null },
                    { new Guid("a1924238-fa63-32bd-dcd5-3f8c1d477a72"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "article", true, true, true, true, "announcement", "list", false, new byte[] { 168, 132, 214, 88, 172, 239, 253, 245, 11, 6, 54, 151, 212, 102, 132, 129 }, "PublishDateDesc", 3, true, false, true, false, false, false, false, false, true, false, false, null, null },
                    { new Guid("d24c0018-525a-9148-c09d-5cb607d3b387"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "story", true, true, true, true, "success-story", "cards", false, new byte[] { 224, 7, 208, 212, 162, 53, 121, 210, 19, 152, 178, 175, 41, 111, 174, 92 }, "PublishDateDesc", 6, false, false, false, true, false, false, true, false, true, true, true, null, null },
                    { new Guid("d30550f5-b105-d0cf-4bed-ad582a2460bf"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "page", true, false, true, true, "page", "list", false, new byte[] { 123, 33, 37, 23, 218, 189, 58, 116, 19, 177, 198, 93, 28, 186, 233, 205 }, "Manual", 1, true, true, false, true, false, true, true, true, false, false, true, null, null },
                    { new Guid("db1c7e26-fc6e-0178-fb5a-10bec802139c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "project", true, true, true, true, "project", "cards", false, new byte[] { 150, 4, 3, 93, 167, 47, 239, 240, 86, 62, 210, 38, 206, 237, 213, 185 }, "Manual", 4, true, false, true, true, false, true, true, false, true, true, true, null, null },
                    { new Guid("f3c481f9-e264-bab5-9d1c-b08a5735cac9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), "team", false, true, true, false, "team", "team-grid", false, new byte[] { 11, 17, 31, 85, 255, 10, 226, 142, 13, 105, 116, 184, 58, 56, 213, 32 }, "Manual", 11, false, false, true, false, false, false, false, false, false, false, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "ContentTypeTranslations",
                columns: new[] { "Id", "ContentTypeId", "LanguageCode", "Name", "RoutePrefix", "SeoCanonicalUrl", "SeoMetaDescription", "SeoMetaKeywords", "SeoMetaTitle", "SeoNoIndex", "SeoOgDescription", "SeoOgImageMediaId", "SeoOgTitle" },
                values: new object[,]
                {
                    { new Guid("02fecd7f-6d9b-6d11-7598-c333a3e86015"), new Guid("97549a0c-9911-a5b4-3c2b-8c14220ed6e6"), "tr", "Basın Bülteni", "basin-bultenleri", null, "", "", "", false, "", null, "" },
                    { new Guid("041f2dce-70ea-1043-8ecc-83fe7baa0956"), new Guid("f3c481f9-e264-bab5-9d1c-b08a5735cac9"), "tr", "Ekip", "ekip", null, "", "", "", false, "", null, "" },
                    { new Guid("0ac62cc5-9f45-5ee6-7e39-fe5e76fafa41"), new Guid("5ada11b7-3892-d01a-3a9d-06cf9fb7a779"), "tr", "Gönüllülük Fırsatı", "gonulluluk-firsatlari", null, "", "", "", false, "", null, "" },
                    { new Guid("0ec8b82d-3480-58d9-8f4c-c343c52e6f50"), new Guid("db1c7e26-fc6e-0178-fb5a-10bec802139c"), "en", "Project", "projects", null, "", "", "", false, "", null, "" },
                    { new Guid("11bbf90a-99c1-bfce-05a4-83035eec803e"), new Guid("5ada11b7-3892-d01a-3a9d-06cf9fb7a779"), "en", "Volunteer Opportunity", "volunteer-opportunities", null, "", "", "", false, "", null, "" },
                    { new Guid("24642175-5be7-a399-2fb1-6b7aef8cb1ea"), new Guid("556494b9-fe9a-55fe-1dc7-d22cdebc8469"), "tr", "Haber", "haberler", null, "", "", "", false, "", null, "" },
                    { new Guid("363cd089-6a62-1225-f84c-4d7b58cdeaaf"), new Guid("9c144b0e-3872-3279-cb81-d767b94cce44"), "tr", "Faaliyet", "faaliyetler", null, "", "", "", false, "", null, "" },
                    { new Guid("366c984d-a2a8-87a1-3fc5-c3dde6d92725"), new Guid("97549a0c-9911-a5b4-3c2b-8c14220ed6e6"), "en", "Press Release", "press-releases", null, "", "", "", false, "", null, "" },
                    { new Guid("39cda1ea-fdfd-c233-1189-dca09380002b"), new Guid("d30550f5-b105-d0cf-4bed-ad582a2460bf"), "tr", "Sayfa", "", null, "", "", "", false, "", null, "" },
                    { new Guid("3d8e2770-daac-31a8-ceb8-63ea23e109dc"), new Guid("d24c0018-525a-9148-c09d-5cb607d3b387"), "en", "Success Story", "success-stories", null, "", "", "", false, "", null, "" },
                    { new Guid("47be5eb8-ea9f-e825-7e44-0b54a29b2dcd"), new Guid("9c144b0e-3872-3279-cb81-d767b94cce44"), "en", "Activity", "activities", null, "", "", "", false, "", null, "" },
                    { new Guid("48dbdb87-336c-5e85-2c24-3ea8a6e2fe78"), new Guid("f3c481f9-e264-bab5-9d1c-b08a5735cac9"), "en", "Team", "team", null, "", "", "", false, "", null, "" },
                    { new Guid("594fc47d-1b77-8663-d499-6c82e2b077bf"), new Guid("1e80f1f3-1ed3-e8f6-42ae-e89322662343"), "tr", "Belge", "belgeler", null, "", "", "", false, "", null, "" },
                    { new Guid("5f4d9ace-e282-c92f-a6d8-2d8bcc106369"), new Guid("d24c0018-525a-9148-c09d-5cb607d3b387"), "tr", "Başarı Hikayesi", "basari-hikayeleri", null, "", "", "", false, "", null, "" },
                    { new Guid("68640320-af74-976d-5fb0-6f43b79cd2f3"), new Guid("db1c7e26-fc6e-0178-fb5a-10bec802139c"), "tr", "Proje", "projeler", null, "", "", "", false, "", null, "" },
                    { new Guid("8e19bfa8-6a78-36ae-f813-466b7e51eedc"), new Guid("a1924238-fa63-32bd-dcd5-3f8c1d477a72"), "tr", "Duyuru", "duyurular", null, "", "", "", false, "", null, "" },
                    { new Guid("a436b29d-8976-1124-5c0e-eef5f7dac177"), new Guid("27800450-ec5d-46eb-9628-0259f12d4ade"), "tr", "SSS", "sss", null, "", "", "", false, "", null, "" },
                    { new Guid("a669753f-f6f6-78e6-1ae2-48cb0ac650c4"), new Guid("2e76848a-82a5-0370-a632-de4fc1d4e97a"), "en", "Event", "events", null, "", "", "", false, "", null, "" },
                    { new Guid("a71a48a0-8778-b105-d26e-719e20c87b7e"), new Guid("d30550f5-b105-d0cf-4bed-ad582a2460bf"), "en", "Page", "", null, "", "", "", false, "", null, "" },
                    { new Guid("ac31f9e0-cdbf-27ee-df86-96a68aa63d06"), new Guid("a1924238-fa63-32bd-dcd5-3f8c1d477a72"), "en", "Announcement", "announcements", null, "", "", "", false, "", null, "" },
                    { new Guid("ad164ee1-54c2-91a8-f673-b1597163c550"), new Guid("21d083a6-cbf8-d3ff-aea8-1c0fddae5ed0"), "en", "Training & Workshop", "trainings", null, "", "", "", false, "", null, "" },
                    { new Guid("b0b1c10e-873f-46b6-50a5-99463081b3a1"), new Guid("556494b9-fe9a-55fe-1dc7-d22cdebc8469"), "en", "News", "news", null, "", "", "", false, "", null, "" },
                    { new Guid("c4a3ec16-64c0-daad-a70a-7d7640d86582"), new Guid("1e80f1f3-1ed3-e8f6-42ae-e89322662343"), "en", "Document", "documents", null, "", "", "", false, "", null, "" },
                    { new Guid("c957777b-a964-9a3a-4503-f87497572823"), new Guid("21d083a6-cbf8-d3ff-aea8-1c0fddae5ed0"), "tr", "Eğitim ve Atölye", "egitimler", null, "", "", "", false, "", null, "" },
                    { new Guid("cba3280f-4ca1-4c7f-1aa5-d737bbaa7d4d"), new Guid("2e76848a-82a5-0370-a632-de4fc1d4e97a"), "tr", "Etkinlik", "etkinlikler", null, "", "", "", false, "", null, "" },
                    { new Guid("e0d63cb0-bfd2-3e8f-3e37-9fa8f6e88ca4"), new Guid("27800450-ec5d-46eb-9628-0259f12d4ade"), "en", "FAQ", "faq", null, "", "", "", false, "", null, "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypes_Key",
                table: "ContentTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeTranslations_ContentTypeId_LanguageCode",
                table: "ContentTypeTranslations",
                columns: new[] { "ContentTypeId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentTypeTranslations");

            migrationBuilder.DropTable(
                name: "ContentTypes");
        }
    }
}
