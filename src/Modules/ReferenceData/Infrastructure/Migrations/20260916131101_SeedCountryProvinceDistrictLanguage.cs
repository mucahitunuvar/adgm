using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCountryProvinceDistrictLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("19dd32b6-c88b-38ca-a652-2f7684b66c9b"), "DK", "Danimarka", true, 29 },
                    { new Guid("310defc2-7751-249b-e14c-714d111c3bf9"), "UA", "Ukrayna", true, 22 },
                    { new Guid("334dfa42-b719-48bb-1c40-dcc29142a55f"), "CA", "Kanada", true, 30 },
                    { new Guid("339ec35b-b804-b34e-1411-177ebf460af6"), "KR", "Güney Kore", true, 34 },
                    { new Guid("3428e0d1-3bb5-9d44-fabb-154ca81d52cd"), "BE", "Belçika", true, 6 },
                    { new Guid("3ccbbfe4-5a69-93c7-8fc9-7c9dd351af4f"), "KW", "Kuveyt", true, 20 },
                    { new Guid("3ec7b43a-80ce-3386-358b-2be79299ca2b"), "CH", "İsviçre", true, 8 },
                    { new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "TR", "Türkiye", true, 0 },
                    { new Guid("4805face-e426-2c07-87df-a89b47709c3f"), "RO", "Romanya", true, 25 },
                    { new Guid("52b6c985-f549-8e2c-2086-98470d74326c"), "FR", "Fransa", true, 4 },
                    { new Guid("5a044f5e-d300-13b6-eef3-93ee19b91721"), "BG", "Bulgaristan", true, 23 },
                    { new Guid("67bc4e14-dbe5-c887-d1db-9ed24c18e55e"), "US", "Amerika Birleşik Devletleri", true, 2 },
                    { new Guid("85294520-2815-4b48-1983-ec3abac2a408"), "EG", "Mısır", true, 21 },
                    { new Guid("96f906d9-d34b-b5c6-19d6-3b8c0bf26aa3"), "QA", "Katar", true, 19 },
                    { new Guid("9ad1b2ee-8f01-1148-4d3c-54a4aba56961"), "SA", "Suudi Arabistan", true, 17 },
                    { new Guid("9cfafafa-8acd-9521-f4cd-e2fb1959ef16"), "GE", "Gürcistan", true, 13 },
                    { new Guid("9e4bbd94-33ec-29d3-19b4-72a71d3d39f8"), "AE", "Birleşik Arap Emirlikleri", true, 18 },
                    { new Guid("a1dcfeb2-e5b4-792b-8c6c-8007fffd2abc"), "SE", "İsveç", true, 27 },
                    { new Guid("a3334d2c-4306-a771-dbb5-36426513db1a"), "SY", "Suriye", true, 16 },
                    { new Guid("a54d73d4-006f-5863-c6b7-c6b0597e1406"), "IR", "İran", true, 14 },
                    { new Guid("abf7428b-dd51-fa6c-1c62-08ee4fe937b9"), "AZ", "Azerbaycan", true, 12 },
                    { new Guid("addb5ad9-e8e8-9a93-401a-1d1448309ab7"), "NO", "Norveç", true, 28 },
                    { new Guid("afa01412-a075-56e4-2869-4d57b6d09429"), "RU", "Rusya", true, 11 },
                    { new Guid("bbe4c8c1-5c41-fff1-beeb-3bb273bc1a74"), "AT", "Avusturya", true, 7 },
                    { new Guid("bebe0683-3fad-a9cf-4d1f-c67d8500d359"), "GR", "Yunanistan", true, 24 },
                    { new Guid("c0031388-a2a2-4e8d-954a-c464181c04b6"), "ES", "İspanya", true, 10 },
                    { new Guid("c15ace5e-ffd1-e75b-cf4b-b7a2139fc0a5"), "GB", "Birleşik Krallık", true, 3 },
                    { new Guid("c3657d85-8158-bf6e-25e2-f4d4d5bde7ca"), "DE", "Almanya", true, 1 },
                    { new Guid("cb14515a-62e1-60c4-4088-e64010ec9e29"), "AU", "Avustralya", true, 31 },
                    { new Guid("cdfc042c-436f-ba43-f7b8-104f0c739317"), "PL", "Polonya", true, 26 },
                    { new Guid("d2ab7e74-ff1e-4f61-af87-53160afab9a3"), "IQ", "Irak", true, 15 },
                    { new Guid("d3c7161e-6bc5-f484-8ba3-b68b985b0be9"), "NL", "Hollanda", true, 5 },
                    { new Guid("de096156-7c0a-d247-584c-0b38e2a3451a"), "JP", "Japonya", true, 32 },
                    { new Guid("ecf10f2a-7ce5-a914-586a-1813478d2cc8"), "CN", "Çin", true, 33 },
                    { new Guid("fb8f7265-b628-4534-335a-17853e1b9729"), "IT", "İtalya", true, 9 }
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "ProvinceId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("00179d17-0dd5-fb8c-5f9e-7786b5daf27e"), "3427", "Maltepe", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 448 },
                    { new Guid("007cac78-e8d3-3a9a-9bc2-97bb513656c0"), "5311", "Merkez", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 723 },
                    { new Guid("007d6d54-6c36-5eae-5cfb-0bb6218113eb"), "2116", "Sur", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 281 },
                    { new Guid("00938280-24b4-b33c-a20f-4eb8974b1e9a"), "0315", "Sandıklı", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 38 },
                    { new Guid("0187d27d-745b-4486-9547-799ac39f006d"), "6008", "Reşadiye", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 809 },
                    { new Guid("0194d002-8576-e881-10fc-3489af96aa01"), "2503", "Çat", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 314 },
                    { new Guid("01de1dca-842a-47d9-ebd0-005bd581fcf0"), "0709", "Gündoğmuş", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 90 },
                    { new Guid("0239e566-f2ab-5cc5-12ab-b800ec1fb68d"), "5209", "Gülyalı", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 702 },
                    { new Guid("024a1e0b-9067-cd58-55a7-fbc665ab9f19"), "0712", "Kemer", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 93 },
                    { new Guid("028a6c5d-b017-4675-515b-88bcb94dd082"), "3605", "Merkez", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 495 },
                    { new Guid("0293c270-d024-48fe-d4c3-161f6a4805cf"), "3439", "Zeytinburnu", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 460 },
                    { new Guid("029a70c5-305b-59f6-da37-b22e471f08dc"), "3418", "Esenyurt", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 439 },
                    { new Guid("02abbd5f-7839-c08f-20d4-fc7087e8e53b"), "7603", "Merkez", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 942 },
                    { new Guid("03687df6-c493-bff5-da5d-a5671fc23edf"), "0901", "Bozdoğan", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 109 },
                    { new Guid("03d4ae60-6bc0-64f5-57bd-86f3aee0b681"), "6307", "Haliliye", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 846 },
                    { new Guid("03eba2ac-e4bf-8ad6-631e-c3e35fd69b0a"), "5307", "Hemşin", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 719 },
                    { new Guid("0442b458-8f1b-6615-68cf-6b679b888dc7"), "3516", "Karaburun", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 476 },
                    { new Guid("04c8fe07-7195-da88-bc0d-9eab02f09d09"), "4507", "Kırkağaç", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 626 },
                    { new Guid("0503fcd5-f2a1-e94e-ad52-96f7eac69df0"), "0115", "Yüreğir", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 14 },
                    { new Guid("052ae632-0193-1b03-023a-95a142c3cde0"), "4224", "Meram", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 585 },
                    { new Guid("0531019f-e64d-8c11-d451-cf3000a022b7"), "5811", "Koyulhisar", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 784 },
                    { new Guid("0550db70-aa37-2f47-e202-b66354e7b128"), "1019", "Savaştepe", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 145 },
                    { new Guid("05996af4-b947-fba1-427e-fdb3cbb3d8b9"), "0112", "Seyhan", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 11 },
                    { new Guid("059a9f00-77af-65aa-22d3-add55034f2d9"), "0506", "Suluova", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 55 },
                    { new Guid("05aedf14-e473-7209-0d92-88f50de89f36"), "1407", "Mudurnu", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 177 },
                    { new Guid("05d549c9-c24a-cf87-fd36-d8b0af86f5f8"), "3530", "Urla", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 490 },
                    { new Guid("0668f442-452a-1f4d-0b41-7c791810227e"), "1912", "Osmancık", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 243 },
                    { new Guid("06a35f89-9d7e-29e2-d1c7-5f2c6507ab04"), "6302", "Birecik", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 841 },
                    { new Guid("06e216da-242c-b84b-2d9f-aabbdbb7be72"), "7804", "Ovacık", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 953 },
                    { new Guid("07353e33-cfcd-8e1a-8452-b2ad60d8ce8c"), "2201", "Enez", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 283 },
                    { new Guid("0755047d-f405-7659-c8a2-74c9ddfabb74"), "0403", "Eleşkirt", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 44 },
                    { new Guid("07a87907-4e5f-ff3f-f63e-706690babc45"), "3419", "Eyüp", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 440 },
                    { new Guid("07ba810e-d562-ca0f-077a-705a22db1b51"), "4506", "Gördes", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 625 },
                    { new Guid("08c64f89-122a-d034-4938-be9e14e5a672"), "2501", "Aşkale", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 312 },
                    { new Guid("08e56a66-26c2-7160-00b0-124371292a7a"), "7902", "Merkez", true, new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), 957 },
                    { new Guid("08f34c0a-4c7a-09a6-f3fe-916f8ee818ac"), "3413", "Beyoğlu", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 434 },
                    { new Guid("0913b88a-9708-9e80-6623-1c1a8f7b7bec"), "6104", "Beşikdüzü", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 817 },
                    { new Guid("091ae9de-794b-4575-bc71-77456e4e3207"), "0609", "Elmadağ", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 65 },
                    { new Guid("09903b91-f419-0a1d-069a-049c90af8c4a"), "8006", "Sumbas", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 965 },
                    { new Guid("0abdfab9-a9bd-b7e9-89fa-468edd5cfc5d"), "0717", "Manavgat", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 98 },
                    { new Guid("0b2b8885-5ac8-d0f0-2753-96afb63151b4"), "2605", "Han", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 336 },
                    { new Guid("0b467da7-7a95-9ddf-172a-3e3447669c06"), "5215", "Kumru", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 708 },
                    { new Guid("0b9c2e23-bec4-74aa-5a7f-9506a5b71b1d"), "1406", "Merkez", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 176 },
                    { new Guid("0d585c4e-e5db-f294-a1de-b7588f79a80e"), "2803", "Çamoluk", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 357 },
                    { new Guid("0d6e601c-f126-b36e-3ff3-bb0bace6f9f7"), "3905", "Merkez", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 539 },
                    { new Guid("0e020603-4620-1086-e855-7539e7c9d8a1"), "1704", "Bozcaada", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 211 },
                    { new Guid("0e86b451-f60f-aa64-b281-1315e21cec90"), "6508", "Gürpınar", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 866 },
                    { new Guid("0e937d62-de7f-60cb-b1ab-1a25fd666fd4"), "5801", "Akıncılar", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 774 },
                    { new Guid("0ebe869f-1b1d-aff2-75a9-5cbb4ffd7f75"), "3113", "Reyhanlı", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 393 },
                    { new Guid("0ec358f9-7e22-b1b0-3b4e-f3bb6b76e3f2"), "3528", "Tire", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 488 },
                    { new Guid("0ec5f580-3149-be00-b549-fc5a2f4cb298"), "5005", "Hacıbektaş", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 684 },
                    { new Guid("0f2ca77c-c661-bf70-1679-57621a89a038"), "1709", "Gökçeada", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 216 },
                    { new Guid("0f36076e-5cf8-89f8-394d-9ad0006612bd"), "3806", "İncesu", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 524 },
                    { new Guid("0f4e833e-0a96-ae6c-64f4-f719c335731e"), "3407", "Bakırköy", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 428 },
                    { new Guid("0f65aa7f-704e-b715-dbf2-e718dcfc60da"), "4606", "Elbistan", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 643 },
                    { new Guid("0f9356d6-6781-ad03-9c50-fc0ceeb3517f"), "5704", "Durağan", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 768 },
                    { new Guid("0f96c21b-5f2c-d9c1-0dc5-13fdaa40d7ed"), "2811", "Keşap", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 365 },
                    { new Guid("0fb15df1-15ca-9eea-aaa7-732156372753"), "1307", "Tatvan", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 170 },
                    { new Guid("0fdefc50-f43d-69c2-09c3-989733d46f22"), "2208", "Süloğlu", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 290 },
                    { new Guid("0fe2eb45-7d70-2a39-168d-e36be67bc671"), "5208", "Gölköy", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 701 },
                    { new Guid("1000a622-99f6-16cb-ebeb-c6be947ce042"), "6802", "Eskil", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 893 },
                    { new Guid("1023f337-64f9-350f-2a4e-acd532b9d7b9"), "5402", "Akyazı", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 726 },
                    { new Guid("104bac4b-ee1e-a5fd-79d7-dccdd0731b25"), "6305", "Eyyübiye", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 844 },
                    { new Guid("104c8461-b927-6f1b-4790-30452489b3e5"), "1612", "Nilüfer", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 202 },
                    { new Guid("107600e0-342f-be51-e9d7-4210d3565fb7"), "0209", "Tut", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 23 },
                    { new Guid("1083eab5-621f-1405-84f0-b7be8c6206ac"), "6009", "Sulusaray", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 810 },
                    { new Guid("10916687-0fc9-cc3b-9ac9-98f573435297"), "0708", "Gazipaşa", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 89 },
                    { new Guid("10ca8f53-593b-47a2-d505-199f5de6ad3a"), "5908", "Muratlı", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 798 },
                    { new Guid("10d98c90-0c56-1d89-25ef-123394b6e3dc"), "3101", "Altınözü", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 381 },
                    { new Guid("10eb6b4d-ed48-350b-1cde-eeeb51842a96"), "4213", "Emirgazi", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 574 },
                    { new Guid("110521e7-e46b-4e2a-8b16-36e69411270d"), "3303", "Aydıncık", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 411 },
                    { new Guid("114cd08c-e70b-07a1-0e50-6b0893381a8e"), "3529", "Torbalı", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 489 },
                    { new Guid("116fc35a-a1ed-4c13-5ee7-81fb2d508882"), "6902", "Demirözü", true, new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), 900 },
                    { new Guid("11e9c93d-1e8f-485b-1ea2-57c094e620e5"), "7103", "Çelebi", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 910 },
                    { new Guid("120bb334-f020-adee-b014-a03e09832f37"), "4311", "Simav", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 603 },
                    { new Guid("12126022-ffa2-2c78-15e9-5e0bc2521f6d"), "0907", "İncirliova", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 115 },
                    { new Guid("12d6ebcf-977b-c65e-9530-32e784789078"), "1004", "Bandırma", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 130 },
                    { new Guid("1331d1dd-29f4-ffe7-27dd-74d55fc75515"), "7703", "Çınarcık", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 946 },
                    { new Guid("13c5415c-8335-3876-d99a-6b2948879514"), "3210", "Şarkikaraağaç", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 405 },
                    { new Guid("13f26922-d129-2bec-3366-f1ee15c058f4"), "5508", "Havza", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 748 },
                    { new Guid("140afadd-858c-d63f-3f21-1e418fe1f6d3"), "4501", "Ahmetli", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 620 },
                    { new Guid("140b31b6-f0c7-a416-1ad8-3d2589e5a548"), "7307", "Uludere", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 929 },
                    { new Guid("14537dcf-35f4-46a5-3f31-d916f6ee7be1"), "7705", "Merkez", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 948 },
                    { new Guid("14585b38-dcf3-f001-7869-c19c9a343345"), "1008", "Edremit", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 134 },
                    { new Guid("1473dcef-125f-7de4-3693-e4e401bc4d31"), "0719", "Serik", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 100 },
                    { new Guid("14a580c2-c7dd-6609-2286-bb847e6192d4"), "2020", "Tavas", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 265 },
                    { new Guid("15052912-50d4-0b28-4750-e54044297b04"), "3503", "Bayındır", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 463 },
                    { new Guid("1594b928-b81f-e2e4-7029-83d46972bd2e"), "5702", "Boyabat", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 766 },
                    { new Guid("16deba2d-4129-eef5-030a-4d2020a74d56"), "2406", "Otlukbeli", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 308 },
                    { new Guid("1743cc59-40b4-63f0-444c-5baa4331b866"), "0406", "Patnos", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 47 },
                    { new Guid("1748d4d6-719a-e6b8-b6cb-810603fd10ab"), "6113", "Ortahisar", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 826 },
                    { new Guid("1753c64c-1070-0faa-e028-d98164e67966"), "5303", "Çayeli", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 715 },
                    { new Guid("17b14cf8-241c-4c0a-d5ee-8b4c7cde8ef5"), "4708", "Nusaybin", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 657 },
                    { new Guid("17c75370-80a9-88d8-5be1-e11f929089d2"), "4612", "Türkoğlu", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 649 },
                    { new Guid("17d23cba-e25b-cda5-08d0-b9036e331173"), "5004", "Gülşehir", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 683 },
                    { new Guid("17e9548b-f2d2-3353-7b85-7a848b6b6844"), "2409", "Üzümlü", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 311 },
                    { new Guid("1811465b-d0aa-b3fe-9a32-8a817274838e"), "6205", "Nazımiye", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 836 },
                    { new Guid("18360516-134f-f65c-7646-c95f06c36065"), "6704", "Ereğli", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 889 },
                    { new Guid("18eb94b9-ce15-8c6a-86e0-f6b587b0de94"), "5606", "Şirvan", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 763 },
                    { new Guid("1914252c-eb75-7cd2-9073-01862d596382"), "6003", "Başçiftlik", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 804 },
                    { new Guid("194293db-0e83-67df-1dbf-d96a3317056d"), "4517", "Turgutlu", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 636 },
                    { new Guid("1962f24e-b773-a4c5-8aaa-5c71a1053306"), "0604", "Bala", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 60 },
                    { new Guid("197561ea-a138-81b5-cc1d-5fa99815a3bc"), "7003", "Ermenek", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 904 },
                    { new Guid("19a30190-f1e4-dc90-4205-ea0b8559c40f"), "3425", "Kartal", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 446 },
                    { new Guid("19ba79da-8c36-dfdf-ba81-2527ca23bd50"), "4711", "Yeşilli", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 660 },
                    { new Guid("19ffdffd-93ca-3244-4dee-2be7e91ba8e3"), "4109", "Kandıra", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 558 },
                    { new Guid("1a09e524-6107-a8e5-42b7-7aa61eb02205"), "1708", "Gelibolu", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 215 },
                    { new Guid("1a10dad0-937b-5ddb-bcad-167b6178cfc2"), "6006", "Niksar", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 807 },
                    { new Guid("1a3627f4-760a-108d-dee6-09efa4469133"), "2507", "Karaçoban", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 318 },
                    { new Guid("1a4bfb2f-b75a-fa7f-d36f-9329ecfbce5d"), "1609", "Kestel", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 199 },
                    { new Guid("1a87bd79-318a-2aba-4acb-0cdc22661d2e"), "2401", "Çayırlı", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 303 },
                    { new Guid("1b7aa360-8fcf-8c15-2e5e-274220bd1fed"), "3206", "Keçiborlu", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 401 },
                    { new Guid("1b97799c-3e9c-cd0e-23e6-94d7c203f79b"), "3606", "Sarıkamış", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 496 },
                    { new Guid("1bab9c53-58c6-fe3f-7e70-cc48db236aba"), "7305", "Merkez", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 927 },
                    { new Guid("1c7748d5-0379-4ff8-b7ac-3db38ccd1d85"), "5604", "Merkez", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 761 },
                    { new Guid("1c80889c-e1cb-bec6-28ee-1b8d1c3da26e"), "6005", "Merkez", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 806 },
                    { new Guid("1c9d5d98-2026-a08e-9ceb-01f3be8c0ab3"), "3432", "Sultanbeyli", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 453 },
                    { new Guid("1c9d6663-9413-a1c5-10c0-a5f9c8ff24ff"), "3115", "Yayladağı", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 395 },
                    { new Guid("1d14777c-993a-4d47-fc5f-a2b659c594b3"), "4307", "Gediz", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 599 },
                    { new Guid("1d3d7282-ae7b-991c-1364-34bf4de71c06"), "2307", "Kovancılar", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 298 },
                    { new Guid("1d50701d-137a-7cbc-274b-87611fa00bfe"), "0505", "Merzifon", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 54 },
                    { new Guid("1d9bab36-32a6-fc54-5117-94a2546f1de1"), "4710", "Savur", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 659 },
                    { new Guid("1dade086-c903-d14c-a2a8-9b99561d9db9"), "5006", "Kozaklı", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 685 },
                    { new Guid("1dec1ac8-e49c-0fb5-4ad9-d9265ec1ef20"), "3105", "Defne", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 385 },
                    { new Guid("1ebbae63-365b-25ce-d066-d934de24a8b8"), "2703", "Karkamış", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 348 },
                    { new Guid("1f34fdf4-8bf1-8195-1198-4b161232e2a2"), "4811", "Seydikemer", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 671 },
                    { new Guid("1f43ebb2-c0ff-f614-2ec1-c35bd13cab5c"), "4221", "Karapınar", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 582 },
                    { new Guid("1f8ef7d0-a55e-4b73-271a-eabd7351b113"), "4313", "Tavşanlı", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 605 },
                    { new Guid("1fef895c-9415-5b32-4a00-e4faaba1f844"), "0408", "Tutak", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 49 },
                    { new Guid("201ce73a-b717-deff-6ff2-6ea18a0c3f2e"), "6805", "Merkez", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 896 },
                    { new Guid("2024f47e-4cfa-7841-8882-4f8701bd75f4"), "2901", "Kelkit", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 371 },
                    { new Guid("202711b3-b50e-e3fa-253f-e5165d54c503"), "5810", "Kangal", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 783 },
                    { new Guid("2051f7c8-fc72-0f91-9eca-a247360b8ccc"), "0917", "Sultanhisar", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 125 },
                    { new Guid("20524eaa-6129-6a48-b221-94cda08eee98"), "2602", "Beylikova", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 333 },
                    { new Guid("20920848-c1c8-046c-8c3c-020d2f6f0256"), "5502", "Asarcık", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 742 },
                    { new Guid("20a1d7f3-b6b3-5e45-e53e-af819c18137d"), "2114", "Lice", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 279 },
                    { new Guid("20f7e17a-8df6-68e9-c926-506badcc2d07"), "3904", "Lüleburgaz", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 538 },
                    { new Guid("2172c52c-2c82-9d93-c9f2-355f0e323452"), "1506", "Gölhisar", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 185 },
                    { new Guid("22259c42-62e7-a209-c5f8-8535ac213d67"), "3718", "Şenpazar", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 516 },
                    { new Guid("222a3a67-4c8c-bc4f-5e4e-47dc66c73631"), "4202", "Akören", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 563 },
                    { new Guid("22d70a45-87c8-b160-3782-7eaa11c89077"), "5007", "Merkez", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 686 },
                    { new Guid("2349c4e6-4cba-6ffb-39c1-d7801b78c8fc"), "3712", "İhsangazi", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 510 },
                    { new Guid("2376b198-d420-9b69-c5e8-b7a0639a17de"), "1703", "Biga", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 210 },
                    { new Guid("240941c8-fd1e-7a98-a6e4-3cddbf03e48d"), "6117", "Vakfıkebir", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 830 },
                    { new Guid("24296299-a56d-1f13-24fe-ec362dd46b24"), "3403", "Ataşehir", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 424 },
                    { new Guid("24796970-0a55-d380-08a8-2d455b88d312"), "6603", "Boğazlıyan", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 874 },
                    { new Guid("248d16d4-5cfe-dc93-e2bc-0106ed90275a"), "1701", "Ayvacık", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 208 },
                    { new Guid("24c24367-69e2-4a4b-3e0c-9f86a22c38a1"), "1305", "Merkez", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 168 },
                    { new Guid("24d52fb0-0b73-b292-6f9f-3e2e82ce94af"), "5003", "Derinkuyu", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 682 },
                    { new Guid("255a8a63-5530-0b32-dd78-d903f5b4f3bc"), "2706", "Oğuzeli", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 351 },
                    { new Guid("25833b0b-87d4-f9b3-45b3-feef3d1007ad"), "3802", "Bünyan", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 520 },
                    { new Guid("25b51f1a-c709-236e-9c5f-91fa48cfe4ac"), "7108", "Sulakyurt", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 915 },
                    { new Guid("25ff5de0-1042-2509-f63c-2a49d63e73aa"), "1107", "Söğüt", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 154 },
                    { new Guid("260c30e7-74dc-dcd9-78fb-1b74cfc82437"), "3414", "Büyükçekmece", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 435 },
                    { new Guid("279c5bd1-ad87-3b09-09b1-ce30b5809c2a"), "3434", "Şile", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 455 },
                    { new Guid("27b4e054-f362-2a00-ca5e-0829a407d0fe"), "4801", "Bodrum", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 661 },
                    { new Guid("280608b6-ac57-b205-d2df-0abb4768e25a"), "3810", "Pınarbaşı", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 528 },
                    { new Guid("287f1302-c9cc-1d6b-7d36-da25e3e8046b"), "1011", "Gönen", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 137 },
                    { new Guid("28f1f7bf-47ed-f12d-f6ff-2ed3055d171a"), "0608", "Çubuk", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 64 },
                    { new Guid("2975d9b0-7ee7-9bb1-2c9c-566e015a6f0f"), "4230", "Yalıhüyük", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 591 },
                    { new Guid("2a4d25db-20ea-37d1-99a5-82c70574f657"), "4515", "Soma", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 634 },
                    { new Guid("2ab2663f-a3f5-c431-640a-84dbeade7a30"), "3108", "Hassa", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 388 },
                    { new Guid("2ad67f83-8435-33b5-9210-a8974e175cfc"), "3409", "Bayrampaşa", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 430 },
                    { new Guid("2b3823de-32ad-cf4f-7de7-906a91b9413c"), "3004", "Yüksekova", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 380 },
                    { new Guid("2ba3acdc-cad2-452b-0307-5d0affa8e8a1"), "4214", "Ereğli", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 575 },
                    { new Guid("2bcf5464-96d0-fc55-54e9-fb897882a4a4"), "2709", "Yavuzeli", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 354 },
                    { new Guid("2bea3b5d-41c1-b28b-cd35-0e4b71662f8b"), "0105", "İmamoğlu", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 4 },
                    { new Guid("2bf81188-78fc-f902-452f-b56b223294ba"), "5207", "Fatsa", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 700 },
                    { new Guid("2c1718d5-250e-e6b1-6557-9702f40222b0"), "4308", "Hisarcık", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 600 },
                    { new Guid("2c3d3eb5-492c-e753-9d84-b4f120126de8"), "1601", "Büyükorhan", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 191 },
                    { new Guid("2c913fca-4fb5-b2b2-e73b-37db01b0ceae"), "2801", "Alucra", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 355 },
                    { new Guid("2cb1c187-1a75-2113-faf2-63a8920e7ff4"), "3437", "Ümraniye", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 458 },
                    { new Guid("2cd71f8a-3c50-8b73-6ae5-b6b8b4fe40fa"), "5708", "Saraydüzü", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 772 },
                    { new Guid("2cf912d9-a16f-ab30-ff36-1d13b3b1c848"), "2016", "Merkezefendi", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 261 },
                    { new Guid("2d76347c-fbc7-e85d-df35-d2480068a0ef"), "3608", "Susuz", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 498 },
                    { new Guid("2d97204b-f8a9-efdd-d880-521a584f92d1"), "1914", "Uğurludağ", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 245 },
                    { new Guid("2dd895fd-69b9-d489-9df9-31a6b834c4f7"), "3106", "Dörtyol", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 386 },
                    { new Guid("2ed65f36-ef33-8208-0478-d5bace00742a"), "1909", "Merkez", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 240 },
                    { new Guid("2f0b92e1-2fd9-0ebe-353a-83e6d20f2b31"), "0318", "Şuhut", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 41 },
                    { new Guid("3030ffdf-7397-932e-c350-777414b6faa8"), "5404", "Erenler", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 728 },
                    { new Guid("30379c7c-80a2-7ba7-9eeb-676a8142cd62"), "4309", "Merkez", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 601 },
                    { new Guid("30a63a6b-7b7a-3498-7e2a-5282ca34d78c"), "3109", "İskenderun", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 389 },
                    { new Guid("31462072-705a-b329-599e-af0ff81b31b0"), "7904", "Polateli", true, new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), 959 },
                    { new Guid("31470dd3-a622-09b7-0c1b-1c5a77789743"), "7403", "Merkez", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 932 },
                    { new Guid("315259be-1820-fc1b-47cf-7d2579d428d9"), "0911", "Köşk", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 119 },
                    { new Guid("31adcf3c-1dfe-a2b7-424f-d74612714e9c"), "7806", "Yenice", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 955 },
                    { new Guid("31e08767-acb8-e9dc-1996-c7869bb41db7"), "2305", "Karakoçan", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 296 },
                    { new Guid("31e1433c-97d0-858b-35a1-390fb04fabb2"), "4216", "Hadim", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 577 },
                    { new Guid("322ab3f2-81e4-41cf-0a2e-c2b9e387f4a9"), "4810", "Ortaca", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 670 },
                    { new Guid("32672a10-a256-459c-0929-c18ff5072db5"), "1005", "Bigadiç", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 131 },
                    { new Guid("3287733f-d933-c666-5373-2a15a5dae76c"), "0103", "Çukurova", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 2 },
                    { new Guid("32a5ac19-e4b0-58b1-00f0-188e48240638"), "4105", "Dilovası", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 554 },
                    { new Guid("32e8a6dc-362c-ca71-5b9b-6a39d4720e97"), "4813", "Yatağan", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 673 },
                    { new Guid("3304e2e7-e903-c76b-c352-af470f7e5fdc"), "0207", "Samsat", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 21 },
                    { new Guid("336129ca-df72-0d2e-6f41-1ffa0517ad14"), "8003", "Hasanbeyli", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 962 },
                    { new Guid("3399132e-2b16-805f-2ab6-aecdefb761d6"), "6114", "Sürmene", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 827 },
                    { new Guid("33d5da1f-bc58-30b9-4d38-2edc5d3ee418"), "5607", "Tillo", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 764 },
                    { new Guid("343c0ba2-ebcd-5664-3c98-54f48b68d726"), "7805", "Safranbolu", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 954 },
                    { new Guid("35120efb-782d-3d98-66ba-44d8a3a3d174"), "4201", "Ahırlı", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 562 },
                    { new Guid("3562c94d-7e3b-d259-6165-783ef8eae7ef"), "5312", "Pazar", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 724 },
                    { new Guid("3566e03a-bf2c-bb4a-3300-23ac2da95e52"), "3201", "Aksu", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 396 },
                    { new Guid("3589cfa5-79af-9a62-c117-2e9b8dd878cf"), "1505", "Çeltikçi", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 184 },
                    { new Guid("35d1a642-e8d8-ca3a-7749-7c5ef666493a"), "0107", "Karataş", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 6 },
                    { new Guid("35df9d65-a829-c85a-f238-0db0e58c77cd"), "2608", "Mihalgazi", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 339 },
                    { new Guid("3626eb68-d999-6677-3140-55be0c3fa5b6"), "5101", "Altunhisar", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 688 },
                    { new Guid("36693398-e8f9-677a-9071-6ec1ca67a911"), "1812", "Yapraklı", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 231 },
                    { new Guid("36702f58-b2fb-071f-c840-418f38e5a30f"), "6503", "Çaldıran", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 861 },
                    { new Guid("36ee8166-466d-588d-afdd-5da7846e22f3"), "2107", "Eğil", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 272 },
                    { new Guid("371bad61-1f00-ced1-8f07-cacd0708dfa1"), "3715", "Merkez", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 513 },
                    { new Guid("37bd4683-01c1-7080-52fb-e9d6841dc4e1"), "3803", "Develi", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 521 },
                    { new Guid("37cf1b37-0ab6-a665-f24c-f9cb545cbc60"), "0617", "Keçiören", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 73 },
                    { new Guid("37d17b43-eabc-52ce-3ccc-4956b3ed57d9"), "4702", "Dargeçit", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 651 },
                    { new Guid("3812a3c0-3d70-95a9-2c08-96a3525fbf1b"), "6109", "Hayrat", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 822 },
                    { new Guid("390739cc-a48f-7ce7-c5ee-f86156d606d6"), "3804", "Felahiye", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 522 },
                    { new Guid("396c06c0-77a8-cee3-ee1e-8c4ce84bd337"), "0616", "Kazan", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 72 },
                    { new Guid("39782ce8-8b89-7844-bc47-6756754589a2"), "3211", "Uluborlu", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 406 },
                    { new Guid("397a5a74-0efa-3bae-23d7-1dc957559735"), "6201", "Çemişgezek", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 832 },
                    { new Guid("39905b8f-17bf-27b8-c46d-d789cbfbc412"), "0405", "Merkez", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 46 },
                    { new Guid("3a1e69f7-0924-6dbd-0007-66c80ffbc7fc"), "2005", "Beyağaç", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 250 },
                    { new Guid("3a35a340-950c-a049-a8ad-231ad774b27f"), "2306", "Keban", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 297 },
                    { new Guid("3a514dbb-205e-aea2-cf29-651bdaf6bd8d"), "2519", "Uzundere", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 330 },
                    { new Guid("3aac1f68-41df-5d3e-0889-a1a44c45c204"), "4305", "Dumlupınar", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 597 },
                    { new Guid("3abda89b-a277-11a7-54c2-369486f30e84"), "0905", "Efeler", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 113 },
                    { new Guid("3ac32fcf-ed50-9b79-624b-c53da0367bf4"), "5507", "Çarşamba", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 747 },
                    { new Guid("3af68b9e-a194-9041-8d11-2a9b501f424d"), "8007", "Toprakkale", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 966 },
                    { new Guid("3b6a32e4-042a-04cb-262a-6ec2a7ef1dd1"), "3505", "Bergama", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 465 },
                    { new Guid("3bc71e84-d44f-b731-fe97-d387be20217d"), "2516", "Şenkaya", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 327 },
                    { new Guid("3c0e4855-3c11-b4f5-692c-410ac571f62c"), "3430", "Sarıyer", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 451 },
                    { new Guid("3c108f54-ca49-7548-1b61-8e2fcfc846fe"), "3703", "Araç", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 501 },
                    { new Guid("3c74a279-420a-58e8-bed6-217fb9c4351e"), "5602", "Eruh", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 759 },
                    { new Guid("3cfa699f-f65e-c45a-10cb-ed514e163ccb"), "8102", "Cumayeri", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 968 },
                    { new Guid("3d3808fd-d2ba-3ddf-ed64-42ce90af5582"), "4206", "Bozkır", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 567 },
                    { new Guid("3d6328a4-1c36-3bbf-1313-db9cf13a2caa"), "3720", "Tosya", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 518 },
                    { new Guid("3e864610-98fc-206a-9c8d-1e74ef7a8225"), "2203", "İpsala", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 285 },
                    { new Guid("3fcbada4-a612-a27d-ad92-90bc2a96fbff"), "4107", "Gölcük", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 556 },
                    { new Guid("3fd1d7d2-d891-d625-c8ad-aaf946a0f51a"), "7602", "Karakoyunlu", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 941 },
                    { new Guid("3fff030e-0e0c-48f3-ae86-7d4808a388ac"), "1205", "Merkez", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 160 },
                    { new Guid("401029db-4922-414b-6f0e-3811d7921046"), "7303", "Güçlükonak", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 925 },
                    { new Guid("404cad8f-b557-f3e6-ab4e-22d0c25bfbb7"), "6115", "Şalpazarı", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 828 },
                    { new Guid("4064ab68-696d-ac98-5aa5-ae1d0094840b"), "6110", "Köprübaşı", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 823 },
                    { new Guid("40706ab3-1638-8195-ccac-7645219d54b6"), "7101", "Bahşili", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 908 },
                    { new Guid("407d5679-81da-a74a-d408-38b0cf9de4a8"), "6807", "Sarıyahşi", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 898 },
                    { new Guid("40a4efb7-8945-1903-e087-4c467c1739e1"), "1002", "Ayvalık", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 128 },
                    { new Guid("40ba322e-a054-dc50-709f-47177d2fc4c7"), "2309", "Merkez", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 300 },
                    { new Guid("40c8547b-30ab-4491-6d8c-321992cd620d"), "4807", "Marmaris", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 667 },
                    { new Guid("416965f4-4c25-b3d0-e8e7-70fe337dbc55"), "4002", "Akpınar", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 544 },
                    { new Guid("41748864-79db-cd36-c4c0-90e854420edb"), "4709", "Ömerli", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 658 },
                    { new Guid("41cf7f8a-0f19-5529-aecc-78c217944418"), "4007", "Mucur", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 549 },
                    { new Guid("4227f803-c228-6f6d-0127-009c94c5f9f1"), "4231", "Yunak", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 592 },
                    { new Guid("429eccf2-2383-d704-cdc1-c5dd33437003"), "3203", "Eğirdir", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 398 },
                    { new Guid("42ea4d04-9b0a-a23c-7bd0-0efeaf0aed06"), "0502", "Gümüşhacıköy", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 51 },
                    { new Guid("432f6ce4-e658-2cc9-a596-428f5926f78f"), "1017", "Marmara", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 143 },
                    { new Guid("43646202-f1bd-d15f-fb5e-471beb122134"), "5103", "Çamardı", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 690 },
                    { new Guid("43819ac2-685c-8a93-afbf-ac113bd6c571"), "7104", "Delice", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 911 },
                    { new Guid("439015ef-ae0a-a972-13d8-49afc34506c9"), "2404", "Kemaliye", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 306 },
                    { new Guid("43a64314-5720-45af-522b-6cee1ac5dc21"), "1801", "Atkaracalar", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 220 },
                    { new Guid("43b7a6ac-7be5-df9e-dfa9-00a9ceb8c7ea"), "3003", "Şemdinli", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 379 },
                    { new Guid("43c6cea5-5ad3-5d6d-4ad0-4a625b07bbac"), "0913", "Kuyucak", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 121 },
                    { new Guid("43f89a00-9e19-88c8-b088-606aacd2e39e"), "3509", "Çeşme", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 469 },
                    { new Guid("44656a4c-615a-12d4-1f13-6d7b88b52259"), "5205", "Çatalpınar", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 698 },
                    { new Guid("4470e27e-f003-f101-bc89-00722d61bc4e"), "6007", "Pazar", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 808 },
                    { new Guid("44d78bc9-8a32-bf9a-8aab-958559f52163"), "4903", "Korkut", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 676 },
                    { new Guid("44e2409d-dc06-5d7f-3a6c-6f997a9e77fd"), "2407", "Refahiye", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 309 },
                    { new Guid("4514203f-2a80-9aed-9d89-b32e7fb84bf0"), "3604", "Kağızman", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 494 },
                    { new Guid("454694eb-de3f-78e1-a526-6f5d76fa4290"), "1707", "Ezine", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 214 },
                    { new Guid("454aa006-7c2d-2f2f-103d-46e75fa9a39e"), "2013", "Honaz", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 258 },
                    { new Guid("45784aee-689f-4867-4a84-7496b67fb8a6"), "5409", "Karasu", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 733 },
                    { new Guid("457a4421-5276-55bb-956e-b6341c93e495"), "1608", "Keles", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 198 },
                    { new Guid("45c2365b-db9d-0cf1-8378-58cb889bac8b"), "3502", "Balçova", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 462 },
                    { new Guid("45d271ba-7f0a-fafa-76fd-c22f61278f58"), "3428", "Pendik", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 449 },
                    { new Guid("462128c1-9355-4322-8fdf-4de7beb31585"), "1606", "İznik", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 196 },
                    { new Guid("467913ad-dbb8-87b3-281c-d661736acfda"), "1404", "Kıbrıscık", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 174 },
                    { new Guid("468ed254-c6b4-74b4-37df-1a03dd1dc4ad"), "7004", "Kazımkarabekir", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 905 },
                    { new Guid("46b36ee7-0afc-6426-a3e5-bfe2a1a8ff49"), "6001", "Almus", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 802 },
                    { new Guid("46eb7293-6b87-b464-d227-ad70a9e45f00"), "4207", "Cihanbeyli", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 568 },
                    { new Guid("47446bb4-8b68-8830-9e5d-4db5125ed0e2"), "7903", "Musabeyli", true, new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), 958 },
                    { new Guid("4764820c-e9f4-0a9c-8963-4fc597f80e64"), "6310", "Karaköprü", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 849 },
                    { new Guid("4776fca5-bf25-78c1-2296-bf43b8cbaf91"), "4704", "Kızıltepe", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 653 },
                    { new Guid("47821e95-ff26-19fd-cb88-259ed32c1825"), "3102", "Antakya", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 382 },
                    { new Guid("4789c4f2-e5ff-5122-03ae-b7262e43af55"), "5401", "Adapazarı", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 725 },
                    { new Guid("47afd20d-a183-d02c-43dd-567ec36783ea"), "1511", "Yeşilova", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 190 },
                    { new Guid("47b555ae-c29a-5808-0371-584418d43813"), "3815", "Yahyalı", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 533 },
                    { new Guid("48365ce2-fcc1-6622-76df-51d9ad3d5e53"), "1401", "Dörtdivan", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 171 },
                    { new Guid("4859093b-db5a-6bc7-d933-03f2896fbb95"), "1306", "Mutki", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 169 },
                    { new Guid("486f19c2-d2a8-1805-91fd-b8d3725f6c70"), "2202", "Havsa", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 284 },
                    { new Guid("48ace69f-8ee1-ee1e-025f-263e82636e72"), "7206", "Sason", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 922 },
                    { new Guid("4924adeb-da83-7468-7281-10ff918db508"), "3501", "Aliağa", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 461 },
                    { new Guid("4938c3c7-cf3c-a5e4-d2ff-c225dd09d85d"), "5910", "Süleymanpaşa", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 800 },
                    { new Guid("4993a8f5-22ab-7936-a6e2-4540824d0895"), "7306", "Silopi", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 928 },
                    { new Guid("4995ad5e-b1db-d7f8-e8a3-9047351fc792"), "0304", "Çay", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 27 },
                    { new Guid("49a39b93-c1e2-6b50-5c00-bd5d7df237ba"), "6613", "Yenifakılı", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 884 },
                    { new Guid("49bb282f-c3e9-d656-61f1-fc7b19fa8063"), "2012", "Güney", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 257 },
                    { new Guid("49e569ae-de67-5636-96f6-7096855afc9c"), "2517", "Tekman", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 328 },
                    { new Guid("4a5cd184-2b23-c8fc-7467-6db6989a0a69"), "0718", "Muratpaşa", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 99 },
                    { new Guid("4a764bce-f68a-a013-7c25-d8263254aa58"), "2004", "Bekilli", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 249 },
                    { new Guid("4a8cb38e-5ccd-260b-6381-762c3be84e32"), "2405", "Merkez", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 307 },
                    { new Guid("4ac48d81-596d-1ec4-e11f-3964e606f04c"), "4223", "Kulu", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 584 },
                    { new Guid("4acc8665-acc6-dc9b-d24a-43da843628b8"), "5302", "Çamlıhemşin", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 714 },
                    { new Guid("4ad9d0eb-a114-7290-80de-2ec1ca4f846e"), "4504", "Demirci", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 623 },
                    { new Guid("4b605dc7-dcc0-ffdd-070f-06e555b9cc84"), "4405", "Darende", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 610 },
                    { new Guid("4b8f9da7-955d-bdc7-64e5-bf114feb22df"), "6804", "Güzelyurt", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 895 },
                    { new Guid("4bb1b3b6-8524-e446-79fc-c35cd11e3995"), "8001", "Bahçe", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 960 },
                    { new Guid("4bc2028b-71c8-2d79-82cc-5eb655a83b3c"), "3811", "Sarıoğlan", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 529 },
                    { new Guid("4c9cb824-16a0-e28d-27c2-6299b55a0e99"), "2508", "Karayazı", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 319 },
                    { new Guid("4cc3c3b9-230c-6339-46d0-ae8974ff8f51"), "3309", "Mut", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 417 },
                    { new Guid("4d06578b-fc77-4e3e-b999-c32e9855a4ab"), "7704", "Çiftlikköy", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 947 },
                    { new Guid("4dedd4a8-c18c-d145-ba42-82e9cdb31d7d"), "5705", "Erfelek", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 769 },
                    { new Guid("4e08f818-0a06-e7cf-36bc-4d61e85925ab"), "5415", "Söğütlü", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 739 },
                    { new Guid("4e6f7bcf-06ac-50c9-1c2a-f98792b86a4b"), "2702", "İslahiye", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 347 },
                    { new Guid("4e8ff246-96ae-5025-217d-6b303afcad41"), "0714", "Konyaaltı", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 95 },
                    { new Guid("4f47e290-4e25-e34b-da9a-5924eba0fe19"), "4513", "Saruhanlı", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 632 },
                    { new Guid("4fa69017-36e2-ff67-42ff-19eae6edd31f"), "6803", "Gülağaç", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 894 },
                    { new Guid("4ffddaf3-d9ce-2832-801c-8ce1902bffb4"), "4706", "Merkez", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 655 },
                    { new Guid("5028724a-5f9c-1fe4-dffe-331457db06f2"), "3807", "Kocasinan", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 525 },
                    { new Guid("5032aff0-8c8c-9123-d825-6348c5acd9aa"), "1901", "Alaca", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 232 },
                    { new Guid("515eab35-40d1-e53c-4f0a-b9e936bbfccb"), "4608", "Merkez", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 645 },
                    { new Guid("527dc86f-fac5-9464-48bf-45442e666b65"), "4607", "Göksun", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 644 },
                    { new Guid("52a76a87-3d8c-5fcb-b60f-40b98692b3ee"), "3114", "Samandağ", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 394 },
                    { new Guid("538738d1-822e-906f-0453-06fab8c9bed0"), "4212", "Doğanhisar", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 573 },
                    { new Guid("5388a04f-3e4f-d253-5633-99c26d1a7818"), "5805", "Gemerek", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 778 },
                    { new Guid("5397662e-3348-bf6c-61dd-1c8ba5ddd1d2"), "4806", "Köyceğiz", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 666 },
                    { new Guid("53a1e9ad-9ac2-94a8-4fcd-467a9265dd61"), "2304", "Baskil", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 295 },
                    { new Guid("53e1d86c-4a27-f598-0c64-ad6db24a5270"), "3901", "Babaeski", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 535 },
                    { new Guid("53f9b317-03c2-b92b-7969-bbaeea51cacb"), "2807", "Espiye", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 361 },
                    { new Guid("5463bd87-2fab-e2f8-3e47-04666c708221"), "2806", "Doğankent", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 360 },
                    { new Guid("5481d3b6-a7c5-97dd-c49b-1dbf721bfaf2"), "0915", "Nazilli", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 123 },
                    { new Guid("54cb9217-7532-36ba-ac0b-af46370d183f"), "6202", "Hozat", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 833 },
                    { new Guid("54f04cee-2d44-beed-189a-bcc8865d9d96"), "3517", "Karşıyaka", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 477 },
                    { new Guid("550a8669-3d66-227a-c2d5-ef09931b7e73"), "1509", "Merkez", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 188 },
                    { new Guid("55750b7f-cc76-0d51-a350-986eb88bfb76"), "4208", "Çeltik", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 569 },
                    { new Guid("55c71578-b9ac-0e2b-5048-ec9c0bbdac99"), "5809", "İmranlı", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 782 },
                    { new Guid("55db2f5e-35a4-86b8-1bee-b7ebae146438"), "3415", "Çatalca", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 436 },
                    { new Guid("56da1c4e-18aa-1207-3781-8316e1f56ea3"), "5814", "Şarkışla", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 787 },
                    { new Guid("572153b4-cf8b-1bbb-4612-ee1d42313566"), "5516", "Vezirköprü", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 756 },
                    { new Guid("574bf7cd-e16d-bb75-e82b-d369ed278962"), "5512", "Ondokuzmayıs", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 752 },
                    { new Guid("5797e912-0106-46ec-3f4d-716926f6c0c1"), "6608", "Merkez", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 879 },
                    { new Guid("579e53c0-f214-6210-4ed8-344430354f49"), "4302", "Aslanapa", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 594 },
                    { new Guid("57d1a26c-47ba-4203-c09f-7d86eaee477a"), "5309", "İyidere", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 721 },
                    { new Guid("58782f6d-8cfc-8c58-4271-f74913de9ba0"), "6309", "Hilvan", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 848 },
                    { new Guid("58ab9b91-4ad7-f736-a03f-c721d1f277a4"), "1021", "Susurluk", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 147 },
                    { new Guid("58c8f933-4046-1c41-4f4d-632d37ee89e3"), "4306", "Emet", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 598 },
                    { new Guid("58ed6855-389a-18a9-3027-6cd756a56b50"), "6901", "Aydıntepe", true, new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), 899 },
                    { new Guid("594166f2-17d1-b8e1-a016-1781b5e6adde"), "2403", "Kemah", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 305 },
                    { new Guid("595b6156-eb1c-f8fc-96d1-dd25d222243f"), "4803", "Datça", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 663 },
                    { new Guid("59b69028-568c-106b-0c99-9f672ac64c3a"), "5213", "Kabataş", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 706 },
                    { new Guid("5a24d4f9-e9c3-67bd-8682-d5dc516392db"), "0205", "Kahta", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 19 },
                    { new Guid("5b167d78-b265-d2de-4b46-eddd18fa2f7e"), "2613", "Sivrihisar", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 344 },
                    { new Guid("5b43ea66-cf60-bab0-69fd-bef30993c907"), "5217", "Perşembe", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 710 },
                    { new Guid("5b5ef148-3d24-e970-cad9-3c2942abfb6c"), "6612", "Şefaatli", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 883 },
                    { new Guid("5b769350-3d45-9dde-4b2f-2997b8e05733"), "6601", "Akdağmadeni", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 872 },
                    { new Guid("5b782054-c364-0aba-70d8-7f86c2adb910"), "1203", "Karlıova", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 158 },
                    { new Guid("5b85f39b-5d74-5dd1-4dfc-eeaa4d5c0c54"), "5412", "Pamukova", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 736 },
                    { new Guid("5bd88228-0545-d37a-cad8-4a704e62dc7c"), "0314", "Merkez", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 37 },
                    { new Guid("5c090e57-3628-4786-7595-c489f2609c7b"), "3520", "Kiraz", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 480 },
                    { new Guid("5cadb949-bfb9-a679-7f60-c99f07f87428"), "3001", "Çukurca", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 377 },
                    { new Guid("5d541259-7a86-773c-f638-7fb7c985a75e"), "4412", "Pütürge", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 617 },
                    { new Guid("5d6108a9-50b4-b305-bf84-65e516a818d2"), "0625", "Yenimahalle", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 81 },
                    { new Guid("5da6ffb2-40e1-0378-3ab1-661bac9205e2"), "1705", "Çan", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 212 },
                    { new Guid("5db09ae6-444a-d4a5-3c16-60c347b8ac48"), "0603", "Ayaş", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 59 },
                    { new Guid("5dbec8c2-f316-e7a7-8063-a2b6ef6c8eb2"), "4402", "Arapgir", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 607 },
                    { new Guid("5e0b3ad5-929f-0c63-bc63-1cec522c245f"), "7205", "Merkez", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 921 },
                    { new Guid("5e213938-4ce6-4c2e-8c85-76a50be75621"), "5218", "Ulubey", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 711 },
                    { new Guid("5e37640d-e781-4164-c129-0caf9bc33176"), "3435", "Şişli", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 456 },
                    { new Guid("5e7638fd-ac3b-b3e9-528a-893b601d5124"), "6303", "Bozova", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 842 },
                    { new Guid("5e82613f-7810-7aa0-6b20-1ef20b09135c"), "1013", "İvrindi", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 139 },
                    { new Guid("5f0e3969-4300-6647-e8e7-b252f2939ab4"), "0803", "Borçka", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 103 },
                    { new Guid("5f1d3d4a-c05f-f4df-19fb-1522bad55b36"), "3301", "Akdeniz", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 409 },
                    { new Guid("5f29e889-7dca-4afb-41af-5ae77e0221b3"), "2014", "Kale", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 259 },
                    { new Guid("5f2ea40b-21d3-4b04-7655-dc3d9f582b50"), "5203", "Aybastı", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 696 },
                    { new Guid("5f5ae402-ea77-3ed2-09bd-98a2f14e0829"), "0507", "Taşova", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 56 },
                    { new Guid("5f64d6da-b6c9-7671-0680-ae916e90193f"), "3424", "Kağıthane", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 445 },
                    { new Guid("5fa96775-c1ff-3091-b676-6c8c4cdac058"), "5406", "Geyve", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 730 },
                    { new Guid("5fb2db04-fedd-78fd-be65-f8159ec54802"), "2310", "Palu", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 301 },
                    { new Guid("5fc0f2a5-9460-fb52-c633-30b858806a94"), "5807", "Gürün", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 780 },
                    { new Guid("5fe37211-5794-1239-095f-9688b73afbee"), "1501", "Ağlasun", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 180 },
                    { new Guid("6014ffab-296b-f782-0570-26b4dd1f86e3"), "1807", "Korgun", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 226 },
                    { new Guid("60abd7fa-ff96-79b1-862a-c410811636b9"), "3519", "Kınık", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 479 },
                    { new Guid("60efa5f9-1746-c6dc-c91c-22a3f49caf47"), "4227", "Seydişehir", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 588 },
                    { new Guid("6170460f-a3cf-9fba-daa7-ae8c92c71437"), "6101", "Akçaabat", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 814 },
                    { new Guid("61a76485-69ca-cc29-3598-d428add07406"), "3213", "Yenişarbademli", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 408 },
                    { new Guid("61de2ade-94ea-f45e-8510-a09531e96a97"), "1402", "Gerede", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 172 },
                    { new Guid("620faafb-cb6a-58bb-8c71-8b9f9427c5a6"), "6306", "Halfeti", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 845 },
                    { new Guid("6270433b-6353-4161-8925-bda9905afa5e"), "0501", "Göynücek", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 50 },
                    { new Guid("62ddf38b-2762-e442-681d-e6b4116b5667"), "2207", "Merkez", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 289 },
                    { new Guid("62e79b98-9d69-0e97-cf1c-406b69626290"), "5907", "Marmaraereğlisi", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 797 },
                    { new Guid("63a9decf-cad1-5777-0dd3-01216cbb1048"), "6304", "Ceylanpınar", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 843 },
                    { new Guid("63f3dffe-db98-90ea-b7d0-79ca8ece118d"), "6313", "Viranşehir", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 852 },
                    { new Guid("64146f0e-9866-3a29-c473-cdf5e1803e23"), "2905", "Şiran", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 375 },
                    { new Guid("64646a1d-dc04-df96-0dd0-ad5714d0a677"), "3902", "Demirköy", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 536 },
                    { new Guid("64a88e33-c968-6878-91db-118e77cff765"), "3421", "Gaziosmanpaşa", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 442 },
                    { new Guid("64a8a9c8-4317-0198-f68d-78d4165236fc"), "0902", "Buharkent", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 110 },
                    { new Guid("65286f80-d508-81c2-6635-a6d255ab013c"), "0504", "Merkez", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 53 },
                    { new Guid("654c27f3-c447-6bfe-1368-f71e862173ea"), "5202", "Altınordu", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 695 },
                    { new Guid("65785331-530d-77e6-e27b-47af62425228"), "1510", "Tefenni", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 189 },
                    { new Guid("66131569-c770-5a46-a365-f25b466e6691"), "7202", "Gercüş", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 918 },
                    { new Guid("6650717b-c5ca-32d8-89bd-855c8ba31bc4"), "5414", "Serdivan", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 738 },
                    { new Guid("66a7f653-e559-e13d-261d-71c9e16d580d"), "5804", "Doğanşar", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 777 },
                    { new Guid("66b3111a-6057-b068-a5d9-11041585774e"), "1804", "Eldivan", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 223 },
                    { new Guid("66c4e39a-ddc9-f740-631d-2602d4a9f123"), "1006", "Burhaniye", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 132 },
                    { new Guid("675159e5-7f1a-ace2-7d9b-1b996e98fe81"), "8108", "Yığılca", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 974 },
                    { new Guid("67947715-e087-b3e7-3adb-9e5249a4db3c"), "4904", "Malazgirt", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 677 },
                    { new Guid("67ed840d-8c0f-196b-7d92-8fd21fe913d7"), "1012", "Havran", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 138 },
                    { new Guid("680a5795-bceb-2192-4154-92898154c489"), "0715", "Korkuteli", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 96 },
                    { new Guid("681e8eca-5428-d598-f9fc-c29dfdab7978"), "1001", "Altıeylül", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 127 },
                    { new Guid("68b83ef9-eb66-91f6-0a5f-655d18557101"), "5513", "Salıpazarı", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 753 },
                    { new Guid("68c329fc-1889-cd62-9056-09d8b6e1145b"), "5206", "Çaybaşı", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 699 },
                    { new Guid("693632da-150a-60b1-f979-799db7767ee9"), "4905", "Merkez", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 678 },
                    { new Guid("69363856-a288-bd8b-c387-713341fdeeb6"), "2311", "Sivrice", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 302 },
                    { new Guid("69707d3e-6954-3fbd-54a1-ae0a30729a17"), "0114", "Yumurtalık", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 13 },
                    { new Guid("69e17305-9e1a-2892-0c58-c2c7d3bdf076"), "0615", "Kalecik", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 71 },
                    { new Guid("6a095501-0881-364b-70f2-3c1923697ebf"), "7701", "Altınova", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 944 },
                    { new Guid("6a28492a-d964-d1fa-a6c2-da35a60d7465"), "0301", "Başmakçı", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 24 },
                    { new Guid("6a80e275-90f3-eac4-82a1-01739171ca42"), "1702", "Bayramiç", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 209 },
                    { new Guid("6b05f4a6-d365-2c88-4209-e3ef99c5cd45"), "3205", "Gönen", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 400 },
                    { new Guid("6b858cdc-2013-3fbb-6cbe-0aa66415c688"), "4215", "Güneysınır", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 576 },
                    { new Guid("6b946810-e2a0-e40c-4244-e1df88e8f8a7"), "6401", "Banaz", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 853 },
                    { new Guid("6bc6a89f-ed5c-1abc-78e4-97436d2cdf68"), "3701", "Abana", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 499 },
                    { new Guid("6bf52d55-2ccc-e2a6-e4ea-8279757db070"), "4809", "Milas", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 669 },
                    { new Guid("6bffe4d9-fa11-2c6b-1f17-ad40f7ef6b51"), "0909", "Karpuzlu", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 117 },
                    { new Guid("6c150530-c6a1-a21d-544a-69bacd8f451c"), "3429", "Sancaktepe", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 450 },
                    { new Guid("6c21da57-8360-d69f-e58b-1e6b4595f302"), "4312", "Şaphane", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 604 },
                    { new Guid("6c3a5a49-da15-b6ab-8a92-45ed59f2d329"), "3906", "Pehlivanköy", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 540 },
                    { new Guid("6c44d397-8c01-94ee-05a9-a66210666be1"), "3208", "Senirkent", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 403 },
                    { new Guid("6ccde6f7-060b-c505-43ab-e951acb882e0"), "3707", "Çatalzeytin", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 505 },
                    { new Guid("6cd21196-6d75-16a3-effc-8666529ecc2b"), "6103", "Arsin", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 816 },
                    { new Guid("6d56a3d0-2d42-6506-8d71-761de33c5e0a"), "0607", "Çankaya", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 63 },
                    { new Guid("6d6a9d94-63e1-2530-f4cb-ecd03c57aa72"), "1809", "Merkez", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 228 },
                    { new Guid("6d7fe760-b8fa-ca22-1ae7-68cb9fae70b4"), "0621", "Polatlı", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 77 },
                    { new Guid("6d83ecd5-53bd-3b72-40b4-3ebce25f84a9"), "4228", "Taşkent", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 589 },
                    { new Guid("6d93d76c-f8c7-e2d0-a8f4-0ac8fb8f5731"), "0404", "Hamur", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 45 },
                    { new Guid("6e1ebaaa-3ffa-cb79-a4a5-7fd94fda8d37"), "2303", "Arıcak", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 294 },
                    { new Guid("6f406ca4-69c9-5000-a40d-f2ec0e5c0e72"), "0104", "Feke", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 3 },
                    { new Guid("6f709d57-e883-b4e2-0ea6-ae10c2e2173c"), "6507", "Gevaş", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 865 },
                    { new Guid("707d5933-1dfd-3e31-4214-ff24563f6654"), "2505", "Horasan", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 316 },
                    { new Guid("70aee598-75ca-8ab0-f7d0-ecd798bb7512"), "7604", "Tuzluca", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 943 },
                    { new Guid("70da7b4c-eaa8-1a66-e668-86d1d6a8717d"), "4101", "Başiskele", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 550 },
                    { new Guid("70f1428e-07df-f56f-358a-e40e9e5ee85f"), "6502", "Başkale", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 860 },
                    { new Guid("716aee55-c66b-fc49-ea8f-0a88871e3a88"), "4611", "Pazarcık", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 648 },
                    { new Guid("71a4dee5-443e-14a3-9486-8adcef53537d"), "7702", "Armutlu", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 945 },
                    { new Guid("71d4563e-c1ae-6189-8489-72f7be41f2b4"), "5510", "Kavak", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 750 },
                    { new Guid("721edc0c-4a65-cf41-20bd-0dc057db4576"), "1613", "Orhaneli", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 203 },
                    { new Guid("7295a940-0ea3-8b10-9ecd-83f7beb56b55"), "0305", "Çobanlar", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 28 },
                    { new Guid("7298969e-d165-471a-cd3b-65a492d042c9"), "2610", "Odunpazarı", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 341 },
                    { new Guid("72b7bb5a-1630-a95b-bfca-4744b0292039"), "3702", "Ağlı", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 500 },
                    { new Guid("731f9835-c900-a01b-0f8c-91f153d4cf47"), "0109", "Pozantı", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 8 },
                    { new Guid("73fb4bc9-9af7-9de8-fc5c-abf6ed007a24"), "0307", "Dinar", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 30 },
                    { new Guid("7455c696-0629-02e5-712a-425f5c696d37"), "2402", "İliç", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 304 },
                    { new Guid("7540edd8-ec91-5710-0ef5-913bc727d428"), "4111", "Kartepe", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 560 },
                    { new Guid("75578ea7-2ba4-f3d6-94e4-79a731ba1cbf"), "6002", "Artova", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 803 },
                    { new Guid("756b8e1c-4136-1d44-9acd-35bf2861b5be"), "3420", "Fatih", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 441 },
                    { new Guid("75842dc1-b74a-3f67-0a8d-83b757572b9f"), "6611", "Sorgun", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 882 },
                    { new Guid("75c30990-72d3-1df1-bf80-ec947ca19d92"), "0622", "Pursaklar", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 78 },
                    { new Guid("75c4361d-81af-6b93-977c-31d0cbc5bc41"), "6806", "Ortaköy", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 897 },
                    { new Guid("75f985ac-93af-0480-c033-20e87059660f"), "0711", "Kaş", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 92 },
                    { new Guid("7614eb83-84e6-4015-0cef-302b9197600c"), "1712", "Yenice", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 219 },
                    { new Guid("76677dbf-42bb-cf35-966c-1cba423a3e1d"), "0202", "Çelikhan", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 16 },
                    { new Guid("768ebeb6-a859-cf64-3ca6-7cd66756320f"), "0716", "Kumluca", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 97 },
                    { new Guid("76c82282-6ba6-8baa-fa02-24fa663c7f6f"), "4414", "Yeşilyurt", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 619 },
                    { new Guid("76fc5951-6c24-05f6-c7d1-39223998bca3"), "4407", "Doğanyol", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 612 },
                    { new Guid("778ee021-25c2-ec2c-c74c-ea1d4d27cc74"), "3603", "Digor", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 493 },
                    { new Guid("7798608b-a40f-5d01-dcf5-781dc3d1db8f"), "5304", "Derepazarı", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 716 },
                    { new Guid("7799f2ff-2300-4e1e-1016-ec97655c3d47"), "1403", "Göynük", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 173 },
                    { new Guid("793decc1-c90f-1145-fb7c-04a7f3c7dae6"), "3812", "Sarız", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 530 },
                    { new Guid("7a571c1b-161a-52c6-d9b1-d210fe3664e3"), "2006", "Bozkurt", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 251 },
                    { new Guid("7a77f453-0dc8-f3c9-53c6-f497d7aac453"), "4222", "Karatay", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 583 },
                    { new Guid("7aa863d5-60ce-4f43-bb8f-82a6216f7812"), "6403", "Karahallı", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 855 },
                    { new Guid("7ac77975-64af-360d-61a0-3259f0d8fb55"), "3422", "Güngören", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 443 },
                    { new Guid("7b33f287-ba83-8d17-227f-75216edce139"), "0602", "Altındağ", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 58 },
                    { new Guid("7b37b89b-0428-f099-122f-fc8b83f2632f"), "5212", "Kabadüz", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 705 },
                    { new Guid("7b7ca908-2413-fc9a-68e0-4551fd495d22"), "0704", "Demre", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 85 },
                    { new Guid("7bf5b221-0457-9eb0-0fd0-443e07e6a0b3"), "1602", "Gemlik", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 192 },
                    { new Guid("7c1a824e-9fa4-b585-5f81-84a0196224fc"), "3401", "Adalar", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 422 },
                    { new Guid("7c29a90b-e2ed-e581-17fe-837797e58c0a"), "4003", "Boztepe", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 545 },
                    { new Guid("7c55c12b-32ea-ee48-07cc-a019d306cdf6"), "5408", "Karapürçek", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 732 },
                    { new Guid("7c7db3bb-bf23-496b-7f6e-721d6058c4e9"), "6102", "Araklı", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 815 },
                    { new Guid("7ccabb50-7cf5-0b1c-d679-6cb22ada83fa"), "3525", "Ödemiş", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 485 },
                    { new Guid("7d23ad8f-9792-a9e8-6497-df59c44cc18e"), "4511", "Salihli", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 630 },
                    { new Guid("7d3212dd-dd9c-67f0-0434-8ed46a93ef8b"), "1610", "Mudanya", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 200 },
                    { new Guid("7d71b256-cd4b-2c41-e93b-6260acc883a2"), "1016", "Manyas", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 142 },
                    { new Guid("7d72500d-99fa-0258-7049-fe84e5bbef8b"), "2612", "Seyitgazi", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 343 },
                    { new Guid("7d7a695b-5d31-2bdc-6686-f3f7b75da558"), "7502", "Damal", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 935 },
                    { new Guid("7dcd0d2b-d076-5301-c37d-db71fd3f929d"), "0611", "Evren", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 67 },
                    { new Guid("7de5e5c6-8c6f-037a-3f54-6971cbf76663"), "2815", "Tirebolu", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 369 },
                    { new Guid("7e7af28a-dcc7-6ecd-5689-08e2caace578"), "7601", "Aralık", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 940 },
                    { new Guid("7e92f1eb-bc41-f3e8-2759-3e020d0a1661"), "0113", "Tufanbeyli", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 12 },
                    { new Guid("7f63eb6d-3745-2b24-c68a-3b17a0c30339"), "5201", "Akkuş", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 694 },
                    { new Guid("7f9ee06b-3387-5d43-02a9-ab5b6f3de62b"), "3705", "Bozkurt", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 503 },
                    { new Guid("7fcb5da2-a157-cb04-771e-affa6809a611"), "0601", "Akyurt", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 57 },
                    { new Guid("7fff01d0-ab62-5800-3df4-0970543ea2af"), "0906", "Germencik", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 114 },
                    { new Guid("8008ebd4-64db-a68e-221d-688f827a8cb1"), "3708", "Daday", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 506 },
                    { new Guid("8031cd42-7fee-11d7-7af2-2ddeab8bb79e"), "5911", "Şarköy", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 801 },
                    { new Guid("80890687-5b75-284d-807c-dbed62d6ae6a"), "4701", "Artuklu", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 650 },
                    { new Guid("8090e2cb-604b-ffaa-fdca-51d7886cc7fc"), "0713", "Kepez", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 94 },
                    { new Guid("80a32e56-065b-ab5f-ff39-2536218751bd"), "2701", "Araban", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 346 },
                    { new Guid("80b3bb04-fccc-ad56-9bc7-723aef635cdb"), "2814", "Şebinkarahisar", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 368 },
                    { new Guid("80e56c02-5989-c64f-8c1e-6bbecfceecb6"), "3513", "Gaziemir", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 473 },
                    { new Guid("81de7abf-fd6c-6a3a-efcd-e19db8f610f1"), "0108", "Kozan", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 7 },
                    { new Guid("825b27b7-904f-8d32-46c9-b3f09378cdfb"), "6604", "Çandır", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 875 },
                    { new Guid("825dbc34-4d00-75c8-0eb3-2b5d2eb6e3a9"), "0807", "Şavşat", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 107 },
                    { new Guid("82633a06-dac2-6015-38fa-a5d84e7b9c8e"), "3907", "Pınarhisar", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 541 },
                    { new Guid("826fc047-50a4-27c5-d916-f38fc94da30a"), "2113", "Kulp", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 278 },
                    { new Guid("82e85400-bbf7-3cdb-9675-2b5a3802042a"), "0806", "Murgul", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 106 },
                    { new Guid("8373b8a7-eac5-78b9-c090-4c38980fb32d"), "4609", "Nurhak", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 646 },
                    { new Guid("838b15c6-dc6c-05df-4cfb-0b857a6033b8"), "7304", "İdil", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 926 },
                    { new Guid("83b403a3-ce34-5bc0-7ea7-309adfb1ca27"), "1409", "Yeniçağa", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 179 },
                    { new Guid("84140825-959a-465c-661c-076fffa991d5"), "5211", "İkizce", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 704 },
                    { new Guid("8414f20d-df17-eb2a-ee34-2a17b1c2b4b4"), "5812", "Merkez", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 785 },
                    { new Guid("84208953-82fa-092a-65e5-7828b54273a8"), "2707", "Şahinbey", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 352 },
                    { new Guid("846d94df-68f2-9dc1-f040-1d4bbdf8dadb"), "0914", "Merkez", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 122 },
                    { new Guid("8484635e-b304-7605-0010-e0f3281506a2"), "6607", "Kadışehri", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 878 },
                    { new Guid("84f9e3e1-c5ed-6ac1-95f7-53fe76199bb8"), "4005", "Kaman", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 547 },
                    { new Guid("8504ece9-e987-16c4-8052-ef39634479b2"), "6509", "İpekyolu", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 867 },
                    { new Guid("85113758-7084-4ea6-8855-842f65bd8ed3"), "4502", "Akhisar", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 621 },
                    { new Guid("8527f38e-0373-397c-33fb-b489484006da"), "1201", "Adaklı", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 156 },
                    { new Guid("85416a9f-9b6b-a4d7-628b-bc2ef7427300"), "6116", "Tonya", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 829 },
                    { new Guid("85c964dc-3ca5-0703-a155-635297259fda"), "2112", "Kocaköy", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 277 },
                    { new Guid("85e64730-927e-7f83-36c0-a3ce3a436435"), "5219", "Ünye", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 712 },
                    { new Guid("8650ad30-ca3e-b4c3-adda-e5829e4a7cd2"), "1018", "Merkez", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 144 },
                    { new Guid("867bba55-e925-1b30-9634-343b91f5146b"), "3431", "Silivri", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 452 },
                    { new Guid("8688d48f-ee3a-f4db-a634-5b187cfa9bde"), "1604", "Harmancık", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 194 },
                    { new Guid("86a35804-0b7a-83d0-a723-1260208ef4d3"), "0208", "Sincik", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 22 },
                    { new Guid("875a92d4-ebea-3f22-1ae0-68cd600838f0"), "2903", "Kürtün", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 373 },
                    { new Guid("87745af4-ba91-c4bf-4ebb-3734e7a462dd"), "1605", "İnegöl", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 195 },
                    { new Guid("87b37628-19b4-2ea8-624f-d90271db89e0"), "3002", "Merkez", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 378 },
                    { new Guid("884d0b4b-7b1c-dd7d-fdba-bcdd0ebc08f9"), "5802", "Altınyayla", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 775 },
                    { new Guid("886db355-3c90-2122-c831-05302f948896"), "0801", "Ardanuç", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 101 },
                    { new Guid("88c916b3-fb44-4441-b058-ac0edb1e2a2e"), "1102", "Gölpazarı", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 149 },
                    { new Guid("890751ea-c24e-33ff-d259-c70bc6be4c19"), "3704", "Azdavay", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 502 },
                    { new Guid("89148880-f269-54cd-3750-eb087ee7a3d6"), "3212", "Yalvaç", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 407 },
                    { new Guid("893b4efe-c877-4681-7d0e-dc846c0e7df2"), "7503", "Göle", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 936 },
                    { new Guid("89a2cc4e-c9a8-26ef-0493-12b1e42fcba2"), "0303", "Bolvadin", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 26 },
                    { new Guid("89bb02a3-880a-ecad-0af4-8e679f61a62c"), "5909", "Saray", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 799 },
                    { new Guid("8a39012f-aeb6-7620-e434-2a3d21d25441"), "5901", "Çerkezköy", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 791 },
                    { new Guid("8a3b7c67-46ac-bef4-1da3-6e3c907289c9"), "5301", "Ardeşen", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 713 },
                    { new Guid("8aac744b-ce32-b16b-1dbf-983d836a6c84"), "2805", "Dereli", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 359 },
                    { new Guid("8ab577ef-c06a-0400-b518-865db7768c46"), "7901", "Elbeyli", true, new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), 956 },
                    { new Guid("8b01e8cd-73c2-0b01-2f49-2d9712d0518b"), "3404", "Avcılar", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 425 },
                    { new Guid("8b61109f-93b5-fd1c-4682-84a6f2b65ec4"), "6203", "Mazgirt", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 834 },
                    { new Guid("8ba4f467-2dd7-7a66-6058-024a53abbe14"), "4508", "Köprübaşı", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 627 },
                    { new Guid("8bc377e4-eeaa-6719-796e-459333f0f090"), "2009", "Çameli", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 254 },
                    { new Guid("8bc415da-ff43-6799-45ff-2ec8b040978f"), "2808", "Eynesil", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 362 },
                    { new Guid("8c4265c7-d845-736c-9f97-a1dbc874e3fd"), "1104", "Merkez", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 151 },
                    { new Guid("8c42cec4-2904-a0b6-9112-c31d03637b36"), "5601", "Baykan", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 758 },
                    { new Guid("8c947a40-05d4-1c42-dac5-88c802ee7bd1"), "0701", "Akseki", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 82 },
                    { new Guid("8ce4d199-3d0b-2d31-8262-390bbdb9bd5f"), "1810", "Orta", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 229 },
                    { new Guid("8cf40d7a-da54-dc1b-3580-e55d867fd302"), "8103", "Çilimli", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 969 },
                    { new Guid("8d5bb10e-3fdc-7ee7-c6bd-2393dbafe3c8"), "0308", "Emirdağ", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 31 },
                    { new Guid("8d5e138b-d183-8c21-1ccb-cf8dabb87e5c"), "3104", "Belen", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 384 },
                    { new Guid("8d63b22c-9434-6abf-1abb-9f6a346bcf5f"), "1207", "Yayladere", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 162 },
                    { new Guid("8ef38b58-8490-7762-c3ed-1d4807cbe1a2"), "3202", "Atabey", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 397 },
                    { new Guid("8eface48-f830-55fb-97b7-2297fa55cb66"), "0618", "Kızılcahamam", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 74 },
                    { new Guid("8eff5bd9-915a-31a8-9a45-6986c18f3002"), "0705", "Döşemealtı", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 86 },
                    { new Guid("8fa4932a-cf41-9e50-1bcb-916fba429c42"), "6010", "Turhal", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 811 },
                    { new Guid("8fde2518-f714-133a-19b7-086bbfd6581c"), "4209", "Çumra", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 570 },
                    { new Guid("90485d8d-10df-c011-01a2-b850e9af0c1c"), "4403", "Arguvan", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 608 },
                    { new Guid("904abd3b-f437-1d56-ba0a-fb575e1f05b3"), "0707", "Finike", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 88 },
                    { new Guid("907d0baa-6a42-f286-671a-a107e14bf6f1"), "0302", "Bayat", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 25 },
                    { new Guid("91144ec5-9581-4916-c426-224f164702b7"), "4410", "Kuluncak", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 615 },
                    { new Guid("915b235e-a1c0-daf1-e4a4-cd3113127bed"), "4304", "Domaniç", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 596 },
                    { new Guid("9182d202-a09f-d9b7-f105-6714a5fca87c"), "7501", "Çıldır", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 934 },
                    { new Guid("918bfaad-7f9b-c6fd-dfb0-0b8d89a8bdfc"), "5503", "Atakum", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 743 },
                    { new Guid("91f19438-2267-d423-38d3-3d5a4c206943"), "5413", "Sapanca", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 737 },
                    { new Guid("91feba61-bc38-95ed-4c7a-272d53b76f26"), "3423", "Kadıköy", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 444 },
                    { new Guid("924ddd80-9410-358d-08bb-87da78336a50"), "1507", "Karamanlı", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 186 },
                    { new Guid("926bbb94-0623-0e1b-e7f9-1c7a0d9ad315"), "0904", "Didim", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 112 },
                    { new Guid("9353ead3-2012-d83f-c1f3-c108882e0726"), "4106", "Gebze", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 555 },
                    { new Guid("935d8741-8187-fbac-87e8-01686d27fe62"), "7302", "Cizre", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 924 },
                    { new Guid("93849d88-9b27-fb57-0999-5f28fba76a51"), "6208", "Pülümür", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 839 },
                    { new Guid("938c102a-73b4-a041-e66b-b1da108da246"), "2511", "Oltu", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 322 },
                    { new Guid("94b3ea2d-7570-f546-ad0d-c46126924aa9"), "6312", "Suruç", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 851 },
                    { new Guid("94c2f857-b0ae-1690-3c2d-f79f1248ef18"), "2102", "Bismil", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 267 },
                    { new Guid("9563a2cb-627d-d867-b220-f57b82e92c1e"), "3416", "Çekmeköy", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 437 },
                    { new Guid("95896ff1-978b-4e1e-dd23-ad73eefc6d6a"), "1905", "İskilip", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 236 },
                    { new Guid("95d2610e-1284-c7bb-5009-4236742deac0"), "1902", "Bayat", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 233 },
                    { new Guid("95f4a3d8-d354-3560-ebf4-55fc9bcf1948"), "5308", "İkizdere", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 720 },
                    { new Guid("95fd376a-e70b-ed22-cc06-2847d42dba32"), "5214", "Korgan", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 707 },
                    { new Guid("9657d8a7-3cea-660c-e087-126bcf36fc4c"), "2110", "Hazro", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 275 },
                    { new Guid("969bf133-7696-2ce4-e921-fc72f4a83b7a"), "8107", "Merkez", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 973 },
                    { new Guid("969d314a-74f5-6175-4d16-160349c290b8"), "6402", "Eşme", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 854 },
                    { new Guid("96b552c6-5380-971a-32f0-1ec1ba9d46f2"), "0610", "Etimesgut", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 66 },
                    { new Guid("9716f0c5-6a55-d915-3091-dbfd3ad2f7c9"), "1615", "Osmangazi", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 205 },
                    { new Guid("97830230-79f8-d3b2-0f9a-34792547bfd1"), "2603", "Çifteler", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 334 },
                    { new Guid("9863eb76-d3ff-bbfe-10cf-3c8b8730c1cf"), "6606", "Çekerek", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 877 },
                    { new Guid("989a4319-d1f9-f42b-7cd5-164e5c7b333e"), "0110", "Saimbeyli", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 9 },
                    { new Guid("98baa8ac-5685-4471-5b89-4579d0fb17e8"), "5509", "İlkadım", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 749 },
                    { new Guid("98c25074-10fe-455a-1617-b1b50e69a531"), "5310", "Kalkandere", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 722 },
                    { new Guid("9936e3f3-0a7a-ae35-ef0c-273a250a60b5"), "5505", "Bafra", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 745 },
                    { new Guid("99c5f876-faea-c451-94ba-c8f459795554"), "2506", "İspir", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 317 },
                    { new Guid("99d54a9e-f003-3bed-554b-2a5efa9f62dd"), "3402", "Arnavutköy", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 423 },
                    { new Guid("99d95882-7ce0-2796-0eb5-469df3edc8af"), "0313", "Kızılören", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 36 },
                    { new Guid("99ffbb26-4d34-fae5-ccdb-fed92fc45b42"), "4509", "Kula", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 628 },
                    { new Guid("9a0065e1-c074-de1e-f6c4-91abbb046fe5"), "5815", "Ulaş", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 788 },
                    { new Guid("9a4c1f6d-4efb-b968-da19-1cd6623c728f"), "2205", "Lalapaşa", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 287 },
                    { new Guid("9ab36369-ba1e-fb0d-8b9c-a4014b2049dc"), "0614", "Haymana", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 70 },
                    { new Guid("9ac6bb65-7678-9938-9dec-969572371e1e"), "4604", "Dulkadiroğlu", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 641 },
                    { new Guid("9ade470a-2603-dafb-a0dc-a4955713a7f7"), "3426", "Küçükçekmece", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 447 },
                    { new Guid("9af133dd-14dd-1c52-cb0b-58ae9081c3d9"), "1607", "Karacabey", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 197 },
                    { new Guid("9aff0dab-3a7d-f91a-bee3-1be5c1a354fe"), "3204", "Gelendost", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 399 },
                    { new Guid("9b358a90-31da-24a0-c3eb-aa03ee0bff43"), "5511", "Ladik", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 751 },
                    { new Guid("9b810d9d-1755-ae65-63f6-563a5abeaf46"), "3908", "Vize", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 542 },
                    { new Guid("9c31b583-aa40-5874-98bd-3b6701bf0b6f"), "3710", "Doğanyurt", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 508 },
                    { new Guid("9c99ad0e-99bb-ffa9-071d-c00da016ea97"), "0624", "Şereflikoçhisar", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 80 },
                    { new Guid("9ccaea3d-a09e-a556-94d1-53217126d61c"), "4603", "Çağlayancerit", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 640 },
                    { new Guid("9d3904af-7def-4ca5-8bf7-5606cf4dfdd4"), "5808", "Hafik", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 781 },
                    { new Guid("9d4f1eaa-a299-c64e-83aa-eb2615db31b9"), "2601", "Alpu", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 332 },
                    { new Guid("9dc9f9d0-7713-ff21-3546-c2ae8433dd6c"), "2002", "Babadağ", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 247 },
                    { new Guid("9dcff53f-0506-1c39-0ab0-9bd080185eb3"), "2607", "Mahmudiye", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 338 },
                    { new Guid("9e7fee88-e48e-b111-bab6-a8effffe4eee"), "5603", "Kurtalan", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 760 },
                    { new Guid("9fdf0f67-01ba-2aec-4713-06a6398fd20c"), "3714", "Küre", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 512 },
                    { new Guid("a088f278-93a9-511d-1b31-539500da18ed"), "2008", "Çal", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 253 },
                    { new Guid("a09ca274-f187-3ce1-1c44-b9087fcbc4f4"), "2816", "Yağlıdere", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 370 },
                    { new Guid("a0b9d3e8-4ef1-d3b8-91db-089b4e5906d6"), "5806", "Gölova", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 779 },
                    { new Guid("a11bdb92-effb-e19c-e791-23874879c801"), "1802", "Bayramören", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 221 },
                    { new Guid("a1752844-b33a-1223-1f9b-5a402f2694b4"), "1913", "Sungurlu", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 244 },
                    { new Guid("a18a07c4-192f-ad22-ea6c-b573a651d711"), "1502", "Altınyayla", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 181 },
                    { new Guid("a1f850b4-59b1-596b-829d-beb7a3fa318b"), "2812", "Merkez", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 366 },
                    { new Guid("a1fa1768-8132-b257-9b75-3b8d21ecbbb5"), "2101", "Bağlar", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 266 },
                    { new Guid("a20b0d44-a1c2-0e99-2934-c32d82ab6f9c"), "1204", "Kiğı", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 159 },
                    { new Guid("a24b91ba-86e1-39ef-588d-762c8912b3c6"), "5216", "Mesudiye", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 709 },
                    { new Guid("a2ded523-9c45-232a-98d0-b6290589964b"), "4906", "Varto", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 679 },
                    { new Guid("a36d7635-5621-8d67-771a-9890d48b9c26"), "0620", "Nallıhan", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 76 },
                    { new Guid("a3acdc03-d800-f353-a371-04a1f7a39d46"), "4220", "Kadınhanı", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 581 },
                    { new Guid("a49146ef-65ac-18d7-a0dc-475f76ce8204"), "1009", "Erdek", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 135 },
                    { new Guid("a5173f45-76e6-e96e-9863-6030cb6fcca0"), "5204", "Çamaş", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 697 },
                    { new Guid("a576b209-c382-66f8-4c5e-f6ff7b3b8d08"), "4210", "Derbent", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 571 },
                    { new Guid("a5a470b5-ffa5-d863-bab9-a45097acdf7a"), "4310", "Pazarlar", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 602 },
                    { new Guid("a5c2a68e-184c-a52c-5f76-229a49014bcf"), "2408", "Tercan", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 310 },
                    { new Guid("a5dc233c-1fd0-e702-dd71-a2187897e242"), "4226", "Selçuklu", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 587 },
                    { new Guid("a61ab9e0-88f6-65b3-770e-1709f312e8c0"), "1617", "Yıldırım", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 207 },
                    { new Guid("a61b2920-f0c6-b5de-a59d-bd748bb96990"), "6609", "Saraykent", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 880 },
                    { new Guid("a6793949-a372-42e6-924a-0bad6d0e2e47"), "0910", "Koçarlı", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 118 },
                    { new Guid("a67d53ec-4872-b1ee-99cc-5c8dff094625"), "0309", "Evciler", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 32 },
                    { new Guid("a6810063-6b99-be61-28ba-1bf8b565f7be"), "1911", "Ortaköy", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 242 },
                    { new Guid("a6bc0c40-1c26-663b-4db1-898babc32f89"), "2204", "Keşan", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 286 },
                    { new Guid("a6cccd80-8ed9-f8a6-38a3-222a79f4b769"), "5001", "Acıgöl", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 680 },
                    { new Guid("a749ca4d-0767-054f-ac11-134677c45a2b"), "1904", "Dodurga", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 235 },
                    { new Guid("a74a20f2-0124-8a37-ec5e-debbbd49b146"), "3103", "Arsuz", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 383 },
                    { new Guid("a7799388-63ec-681c-8161-97bce01a730d"), "1806", "Kızılırmak", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 225 },
                    { new Guid("a78d93f8-a359-37f4-59ae-662241542c72"), "0203", "Gerger", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 17 },
                    { new Guid("a7e9c9cf-ad43-a075-ab79-af07de6e46d5"), "7504", "Hanak", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 937 },
                    { new Guid("a80500b3-2e15-086a-7458-7ac5e8d94356"), "6706", "Merkez", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 891 },
                    { new Guid("a85a4558-f31a-ac4b-1531-799ff3ae4da5"), "4601", "Afşin", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 638 },
                    { new Guid("a92f20ae-2208-a3f0-e3db-1f79a60c7de1"), "0106", "Karaisalı", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 5 },
                    { new Guid("a9b206a5-a6e1-3fb9-8b37-ce8b1896ea75"), "2018", "Sarayköy", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 263 },
                    { new Guid("a9bc7c5f-e42b-0d7e-971c-e383a5fab0cb"), "7706", "Termal", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 949 },
                    { new Guid("a9f9b263-81ec-e6f9-a600-67b4db2810e6"), "6108", "Düzköy", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 821 },
                    { new Guid("aa2b8de3-ec9e-751b-c312-f8f2b479facf"), "3507", "Bornova", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 467 },
                    { new Guid("aa6ff543-280a-d106-ca4d-11eda5280864"), "0606", "Çamlıdere", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 62 },
                    { new Guid("aae09c91-18a6-c265-9af1-e7791f84bf46"), "2606", "İnönü", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 337 },
                    { new Guid("ab2184aa-a391-03ce-0719-b5733be396ab"), "6204", "Merkez", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 835 },
                    { new Guid("ab3bec02-414b-b095-3d67-f8083c06053f"), "1906", "Kargı", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 237 },
                    { new Guid("ab4b330c-4cc2-234e-7f95-d01b5a39bf8e"), "2301", "Ağın", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 292 },
                    { new Guid("ab7d4767-37ea-c786-cefb-cb8c7b93c7b4"), "2604", "Günyüzü", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 335 },
                    { new Guid("abfa030c-da66-3739-1ad6-1cd5c74d4f23"), "4406", "Doğanşehir", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 611 },
                    { new Guid("ac63e3dd-14eb-aa72-8075-b61ac09f6547"), "4004", "Çiçekdağı", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 546 },
                    { new Guid("ac7ce36b-712d-eb47-c5dc-119b82be4bf0"), "0306", "Dazkırı", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 29 },
                    { new Guid("acbf7e14-65a6-0222-94e1-db319d5170d0"), "3305", "Çamlıyayla", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 413 },
                    { new Guid("acd05185-f828-07c0-b384-031702595d73"), "7006", "Sarıveliler", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 907 },
                    { new Guid("ad11cca0-1702-6a6c-80ad-7dff2663cc2d"), "3601", "Akyaka", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 491 },
                    { new Guid("ad2dc8f7-6376-eeb9-a36e-e83e33866687"), "3508", "Buca", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 468 },
                    { new Guid("ad734da8-7f1c-44f1-e66d-f98185061b39"), "5605", "Pervari", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 762 },
                    { new Guid("ad761760-02de-aff2-ebb0-77015ea33496"), "4205", "Beyşehir", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 566 },
                    { new Guid("adabea3c-dd11-ebc9-257b-5ab4bc9b4a4a"), "8004", "Kadirli", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 963 },
                    { new Guid("ae831c3b-06f5-219f-af32-dd1d78f689ff"), "3411", "Beykoz", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 432 },
                    { new Guid("ae90b22d-6fd0-eeca-17c3-f2e944996d38"), "5501", "Alaçam", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 741 },
                    { new Guid("aeb96c1a-2d50-441a-deaf-f298d960cdc7"), "2512", "Olur", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 323 },
                    { new Guid("aefc2e18-bd23-43a8-f32f-07ffaec66d74"), "0710", "İbradı", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 91 },
                    { new Guid("af574fea-c57f-1b1f-90b2-263aac4018b6"), "3816", "Yeşilhisar", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 534 },
                    { new Guid("afabb77a-6106-e9f7-9916-8e57e55b1381"), "3438", "Üsküdar", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 459 },
                    { new Guid("afc9a475-287b-000c-521c-05f193d76f36"), "5306", "Güneysu", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 718 },
                    { new Guid("afd6d0d3-5417-76f4-5890-400c52a17761"), "0805", "Merkez", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 105 },
                    { new Guid("afd81fe3-0756-740e-9898-accc9a66e154"), "5506", "Canik", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 746 },
                    { new Guid("b03388f3-f4b6-ba44-52af-851b89751f21"), "3813", "Talas", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 531 },
                    { new Guid("b06d0380-9f36-6f2a-4fc0-afae2d6c84dc"), "1206", "Solhan", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 161 },
                    { new Guid("b0988fea-d584-0f0f-450d-549707610697"), "6703", "Devrek", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 888 },
                    { new Guid("b0cdd709-0402-d6cd-a11d-da1fa443a10e"), "6012", "Zile", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 813 },
                    { new Guid("b0fdaab9-c8f7-0fb0-cb30-603e2e1dd9f0"), "4001", "Akçakent", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 543 },
                    { new Guid("b104f50d-3df5-a134-798d-51b27017c8de"), "5515", "Terme", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 755 },
                    { new Guid("b11505d3-efd1-f735-d3a6-80172b3b018e"), "1108", "Yenipazar", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 155 },
                    { new Guid("b1160eae-4d4e-35f3-051e-dde09f81a7d8"), "4411", "Merkez", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 616 },
                    { new Guid("b13a8a7c-7826-6ae1-4f48-f8535a1e7df4"), "4705", "Mazıdağı", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 654 },
                    { new Guid("b13ad327-df03-a5c6-ca66-4cee91b180e1"), "1303", "Güroymak", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 166 },
                    { new Guid("b14cfb62-d6d9-89bc-788b-7ece2ebaf614"), "6111", "Maçka", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 824 },
                    { new Guid("b187fbf0-fde2-9ee2-977f-0690417de13e"), "6511", "Özalp", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 869 },
                    { new Guid("b1c606a1-9463-5ace-4257-73cd91330688"), "4303", "Çavdarhisar", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 595 },
                    { new Guid("b2245aa8-870b-ef4f-85ce-d9a91801c312"), "8002", "Düziçi", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 961 },
                    { new Guid("b2383a9d-9ba2-9205-92b4-2a3b7a3d4e4d"), "0402", "Doğubayazıt", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 43 },
                    { new Guid("b249c11a-e155-2e29-a1d6-24f0a9b1902c"), "4211", "Derebucak", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 572 },
                    { new Guid("b25467a8-2b0a-d6a4-c876-c04fd5612114"), "2001", "Acıpayam", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 246 },
                    { new Guid("b25839ed-8eb9-b648-7964-58d8b89a121e"), "2902", "Köse", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 372 },
                    { new Guid("b34544eb-27e1-55e4-f5e4-c48b928cd230"), "7402", "Kurucaşile", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 931 },
                    { new Guid("b3d8d676-dd08-bb1d-e4d9-c559ab3b610f"), "2509", "Köprüköy", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 320 },
                    { new Guid("b527f509-4b4f-ba95-b007-440d255c0dab"), "3602", "Arpaçay", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 492 },
                    { new Guid("b568a95c-7395-f3ea-1a59-6116155529a5"), "3405", "Bağcılar", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 426 },
                    { new Guid("b5b138bf-29c6-98da-6e43-31b8f85efa86"), "4804", "Fethiye", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 664 },
                    { new Guid("b5cdc0df-fc8a-341a-71a3-8402ff5e7780"), "8101", "Akçakoca", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 967 },
                    { new Guid("b5e1ede3-0e26-96dc-b501-278842312b9b"), "5504", "Ayvacık", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 744 },
                    { new Guid("b666b106-a19b-3a1d-7f1a-a73e894b2fbd"), "5706", "Gerze", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 770 },
                    { new Guid("b6974040-0cf0-5ba1-0c43-135c2542bca5"), "4301", "Altıntaş", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 593 },
                    { new Guid("b7188feb-f99b-6607-b99e-6c00b02346a0"), "2510", "Narman", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 321 },
                    { new Guid("b8179f00-1d3d-2d74-102f-8199e75db618"), "5210", "Gürgentepe", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 703 },
                    { new Guid("b82994a0-3fbd-2134-c9be-8f29ecb59f6e"), "2502", "Aziziye", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 313 },
                    { new Guid("b8702f16-66c8-1621-f7aa-6c6b63a192a2"), "3711", "Hanönü", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 509 },
                    { new Guid("b8e4aed2-bf70-fe2b-0e94-12d25ba6047b"), "7109", "Yahşihan", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 916 },
                    { new Guid("b9009057-280e-9041-1156-cf94440b37b7"), "6602", "Aydıncık", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 873 },
                    { new Guid("b9413de8-4d7d-0105-029e-a5bda9e3ea57"), "5002", "Avanos", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 681 },
                    { new Guid("b9581aa9-3d1b-1cf8-9002-0010886f8e8c"), "6404", "Merkez", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 856 },
                    { new Guid("b97421cd-6121-3c96-3e84-c3af6f1f832b"), "2019", "Serinhisar", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 264 },
                    { new Guid("b97cc527-15ef-5c93-7f29-c6308fe38f89"), "3514", "Güzelbahçe", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 474 },
                    { new Guid("b9a779c1-adf2-e4bb-573e-2093ec00c6ac"), "3306", "Erdemli", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 414 },
                    { new Guid("b9e8e38e-f0f6-ccb1-66d1-406f454a03dc"), "5104", "Çiftlik", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 691 },
                    { new Guid("baaf75e7-c572-d7ca-90d6-ae42e51b9bd6"), "4409", "Kale", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 614 },
                    { new Guid("bb873d3b-79bf-2283-289b-473fb3ff87e8"), "3607", "Selim", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 497 },
                    { new Guid("bba78f97-21bb-b682-70dd-5f8cd949b563"), "1304", "Hizan", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 167 },
                    { new Guid("bc096f08-eb3a-fed4-7fdd-3464481e4563"), "4006", "Merkez", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 548 },
                    { new Guid("bc8003af-a50f-69f2-1f82-238eabf85882"), "7803", "Merkez", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 952 },
                    { new Guid("bc8008e3-bf9e-bc1e-1501-9903a1bb886c"), "6011", "Yeşilyurt", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 812 },
                    { new Guid("bd18f1a4-7c1a-933a-7fc8-7ef6c7fe0136"), "1103", "İnhisar", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 150 },
                    { new Guid("bd3a20bf-635c-023d-84c7-a5c77b907eba"), "2111", "Kayapınar", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 276 },
                    { new Guid("bd567a4e-7b76-d895-30e0-9bd036bb06dd"), "6505", "Edremit", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 863 },
                    { new Guid("bd5dd48e-897d-6fd0-ea66-fa092c179b23"), "1803", "Çerkeş", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 222 },
                    { new Guid("bdf147ef-0d69-ddff-f153-4da2ec3d7172"), "4401", "Akçadağ", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 606 },
                    { new Guid("befe171f-ae56-877c-5273-a309ea3bad51"), "7005", "Merkez", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 906 },
                    { new Guid("bf048e82-239d-4864-03ba-b179c42e396e"), "1508", "Kemer", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 187 },
                    { new Guid("bf274e90-f01d-0cb3-1520-c906daa9249d"), "2609", "Mihalıççık", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 340 },
                    { new Guid("bf582f74-80ae-6651-d802-9f4ab32f219d"), "2109", "Hani", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 274 },
                    { new Guid("bf677d76-08ba-8546-b5eb-fcaba56814cd"), "2514", "Pasinler", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 325 },
                    { new Guid("c055778c-f047-7cdd-df8e-975be17e2190"), "1603", "Gürsu", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 193 },
                    { new Guid("c0764943-739a-9463-716f-bd65c580b465"), "5410", "Kaynarca", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 734 },
                    { new Guid("c12730d4-a1a7-21fc-335b-1dc43032746d"), "6614", "Yerköy", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 885 },
                    { new Guid("c12a2b5b-32b4-853b-e9a6-6c18f4ffea71"), "4204", "Altınekin", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 565 },
                    { new Guid("c234755c-63b8-1958-8c4b-539e770dd68f"), "6702", "Çaycuma", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 887 },
                    { new Guid("c26a2e40-2ec4-5a92-469d-60a3f418b924"), "5905", "Kapaklı", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 795 },
                    { new Guid("c2728e97-4e4e-5784-2d5f-05c0253ea332"), "5703", "Dikmen", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 767 },
                    { new Guid("c273c978-0c9a-0cd1-18fb-a7e9e1b04b04"), "3313", "Yenişehir", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 421 },
                    { new Guid("c2b1b891-8412-3ddf-d427-db5e41efa2c6"), "3310", "Silifke", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 418 },
                    { new Guid("c2b884c9-ff5f-4b14-35cd-1675a955572f"), "7401", "Amasra", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 930 },
                    { new Guid("c2cc2247-6540-9dad-1746-a1919e1e8613"), "8104", "Gölyaka", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 970 },
                    { new Guid("c3474549-5420-8d8b-9c2d-f819b92f0687"), "1903", "Boğazkale", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 234 },
                    { new Guid("c365daa8-d520-d3dd-a79f-d990c0cb1565"), "5403", "Arifiye", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 727 },
                    { new Guid("c3f225d6-d695-e406-c3eb-4787ba074e23"), "2906", "Torul", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 376 },
                    { new Guid("c4025883-1bc7-6c1a-1482-e6fa1dcc2b12"), "0912", "Kuşadası", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 120 },
                    { new Guid("c42dffd2-6e11-bc81-2b25-176b1a2106c9"), "1908", "Mecitözü", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 239 },
                    { new Guid("c455d831-37a3-40d8-3d9e-501d6e9c6674"), "7201", "Beşiri", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 917 },
                    { new Guid("c541bc2e-0d82-09dd-1eb3-86b748c3993f"), "7105", "Karakeçili", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 912 },
                    { new Guid("c5932402-4f40-37e7-f472-3db128717d37"), "0317", "Sultandağı", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 40 },
                    { new Guid("c5a97f47-42b4-3034-9510-09d73883d345"), "8106", "Kaynaşlı", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 972 },
                    { new Guid("c5c801ce-da88-dc1f-5d3e-fd2694a5b442"), "2017", "Pamukkale", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 262 },
                    { new Guid("c5de61f5-b501-c06d-500a-41c763926099"), "3112", "Payas", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 392 },
                    { new Guid("c605e43a-18bb-94b6-e0f5-6f89a0223cb7"), "3311", "Tarsus", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 419 },
                    { new Guid("c69a04bc-f164-3a40-51ba-16329ecc1739"), "6605", "Çayıralan", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 876 },
                    { new Guid("c707b422-2baa-060d-0ea2-129ca5c8dbe4"), "3903", "Kofçaz", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 537 },
                    { new Guid("c7120222-24ae-e04a-f64f-89021ef6cb20"), "0804", "Hopa", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 104 },
                    { new Guid("c7648a89-8f37-1662-f3d6-655030aa840c"), "1302", "Ahlat", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 165 },
                    { new Guid("c820a344-b41a-dfe6-9eae-160dacb1bd96"), "2804", "Çanakçı", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 358 },
                    { new Guid("c8541c90-d189-4e7f-c287-a86b70473197"), "0623", "Sincan", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 79 },
                    { new Guid("c8570f49-c56a-7012-46c6-6b4ba9b22fb5"), "7506", "Posof", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 939 },
                    { new Guid("c89745fd-bfff-5529-b235-8787f0ed0cf1"), "3809", "Özvatan", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 527 },
                    { new Guid("c90fb872-2148-461f-2beb-169bb8c1f706"), "5817", "Zara", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 790 },
                    { new Guid("c943efc5-b983-fccb-9be2-ca5b699019da"), "1003", "Balya", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 129 },
                    { new Guid("ca29b1f1-a8c0-89d7-aa99-b95b07f17aa7"), "0808", "Yusufeli", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 108 },
                    { new Guid("ca4a23e6-b927-f056-9de4-dd5d321315bb"), "1105", "Osmaneli", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 152 },
                    { new Guid("ca5c73e5-a3da-e5c2-4891-83ac1f988827"), "2705", "Nurdağı", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 350 },
                    { new Guid("ca6ee9ac-33ad-b66f-98a9-313fb70ec172"), "2308", "Maden", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 299 },
                    { new Guid("ca927d55-b991-a4ac-a186-8a94263a24bc"), "3304", "Bozyazı", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 412 },
                    { new Guid("ca986e61-04de-0012-f16d-e38c754ffcc4"), "0312", "İscehisar", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 35 },
                    { new Guid("caab9b31-f78d-74fc-df15-1df50aa1da91"), "5813", "Suşehri", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 786 },
                    { new Guid("cad3e123-4c18-5f54-c1b8-30985cccfa62"), "4510", "Merkez", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 629 },
                    { new Guid("cb1c1b80-5dd9-536f-7e4e-088d80e35a3e"), "0311", "İhsaniye", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 34 },
                    { new Guid("cb2b417a-1e04-ac7d-b1c7-7fedd6f93fad"), "0908", "Karacasu", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 116 },
                    { new Guid("cb9575bb-e041-7745-b394-eb86f20e84b4"), "0613", "Güdül", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 69 },
                    { new Guid("cbc13f9d-fbf4-3fd9-200a-29102c89497d"), "3522", "Menderes", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 482 },
                    { new Guid("cbca2911-f75e-5c4f-9489-b177b3328388"), "2802", "Bulancak", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 356 },
                    { new Guid("cbda4d3f-0160-c597-c3d0-2fffd4d22c34"), "3504", "Bayraklı", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 464 },
                    { new Guid("cbe6fb0d-7911-96cd-8eb5-a9e2571e5d04"), "5707", "Merkez", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 771 },
                    { new Guid("cc0f1ed5-b61b-bd52-150f-e3e31b5034b0"), "0316", "Sinanpaşa", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 39 },
                    { new Guid("cd4626c3-d56c-6a9c-a6e9-491621cbe74c"), "8005", "Merkez", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 964 },
                    { new Guid("cd5fb58b-74a0-ed85-7d62-6539722a4202"), "0204", "Gölbaşı", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 18 },
                    { new Guid("cd850fa9-7bbe-1c90-366b-32a3c949c9db"), "2513", "Palandöken", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 324 },
                    { new Guid("cd91af4f-f1ec-cdd6-3ae5-bd9e6a6db1c7"), "3506", "Beydağ", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 466 },
                    { new Guid("cdc8e52a-5c37-e24d-9cf1-4dc0677e0cb4"), "3706", "Cide", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 504 },
                    { new Guid("ce42bc7f-f563-b75a-f242-e8b306184c1c"), "4408", "Hekimhan", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 613 },
                    { new Guid("ceaa8714-8e74-4b84-0252-4c9d74edb414"), "1611", "Mustafakemalpaşa", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 201 },
                    { new Guid("ceb8c579-1a9f-dc42-3c63-942d4c4d4663"), "0201", "Besni", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 15 },
                    { new Guid("cf56a574-4bf0-c4c2-b58c-6f5e7eefe65c"), "1408", "Seben", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 178 },
                    { new Guid("d0388482-e986-e848-a0b6-f8f14e034c68"), "0401", "Diyadin", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 42 },
                    { new Guid("d083ad91-0ef6-f2e2-6e53-a217f69e933f"), "3110", "Kırıkhan", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 390 },
                    { new Guid("d1337b8d-8882-53a6-ebb3-d8d36f71d36d"), "1007", "Dursunbey", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 133 },
                    { new Guid("d1362f2a-5fbe-f286-77b9-b3247609ca6b"), "2809", "Görele", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 363 },
                    { new Guid("d1a5d0d5-35c1-409a-07b8-1db3135d810a"), "3716", "Pınarbaşı", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 514 },
                    { new Guid("d1dc58b6-e562-7e7d-8846-3d4dfc46538a"), "4901", "Bulanık", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 674 },
                    { new Guid("d242bdbe-1990-b2ae-d04f-a1e83c867989"), "2302", "Alacakaya", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 293 },
                    { new Guid("d26fcd7e-44e6-018d-4c9e-74f65725fe10"), "6308", "Harran", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 847 },
                    { new Guid("d2b0e254-9a73-4cc9-c671-b45c641a7cb1"), "2515", "Pazaryolu", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 326 },
                    { new Guid("d2b156d5-fb8e-ad6b-1970-ab811a542106"), "4707", "Midyat", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 656 },
                    { new Guid("d2b58d2e-e1b2-b874-f704-a9786961e58d"), "6311", "Siverek", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 850 },
                    { new Guid("d2d4e6b3-a560-6cf6-1c1a-e4421720d88e"), "3518", "Kemalpaşa", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 478 },
                    { new Guid("d3999538-e7eb-27ca-a48c-50cdd788d38f"), "7802", "Eskipazar", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 951 },
                    { new Guid("d39ad855-2b66-10e4-773c-3941d163b4f1"), "5514", "Tekkeköy", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 754 },
                    { new Guid("d4972eca-af97-2205-89d8-23f7c2624071"), "7204", "Kozluk", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 920 },
                    { new Guid("d54a7f41-44c4-9e41-bc3d-cb66bca4ff94"), "4203", "Akşehir", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 564 },
                    { new Guid("d5605776-cd4d-27b7-58ca-c44b7d8cbecd"), "5407", "Hendek", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 731 },
                    { new Guid("d58a635d-5f3c-4423-bd42-684fa36d7e98"), "0206", "Merkez", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 20 },
                    { new Guid("d5f2103e-a86f-850a-9433-f829a2390cdd"), "0605", "Beypazarı", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 61 },
                    { new Guid("d64e7ebb-7521-275b-bde2-20a2ec088021"), "5106", "Ulukışla", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 693 },
                    { new Guid("d65a000a-3b26-e103-bff1-137fe4ee68b8"), "3523", "Menemen", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 483 },
                    { new Guid("d6e34ee3-9d19-c5b1-fbd0-e9c905138250"), "6207", "Pertek", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 838 },
                    { new Guid("d72f46ad-74bc-1726-ec73-bcf4b4c4720b"), "4110", "Karamürsel", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 559 },
                    { new Guid("d7bb9411-ce72-8a41-f3ad-b479e69ff1bd"), "1616", "Yenişehir", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 206 },
                    { new Guid("d7bdac08-1967-e0fc-2c7d-c0dcdcd5b15e"), "0706", "Elmalı", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 87 },
                    { new Guid("d7f201c1-8085-1d68-5e05-cb1f83a42780"), "8105", "Gümüşova", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 971 },
                    { new Guid("d8324c63-6adf-97e3-da83-296eb45abd7f"), "4902", "Hasköy", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 675 },
                    { new Guid("d87b1e12-e2d6-ee44-6f48-affcf7b4f1ce"), "6701", "Alaplı", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 886 },
                    { new Guid("d93b76ab-c94c-cd11-6168-b9ae281d244e"), "6801", "Ağaçören", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 892 },
                    { new Guid("d9cc7896-a582-4cda-5333-593be1143ced"), "4602", "Andırın", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 639 },
                    { new Guid("d9efa5f0-6398-4ef6-20da-1ad4e3f40c36"), "3408", "Başakşehir", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 429 },
                    { new Guid("da2545aa-ee98-3966-6033-d692d0765ba8"), "1010", "Gömeç", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 136 },
                    { new Guid("da7b7d79-d7bd-0c93-e5fa-b2897af2c939"), "5517", "Yakakent", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 757 },
                    { new Guid("da891cab-9211-74e0-c335-33b2f834bac2"), "1711", "Merkez", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 218 },
                    { new Guid("da9dbdbb-e4b2-4a34-1b40-686213426e0e"), "1504", "Çavdır", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 183 },
                    { new Guid("dab1f580-2c38-cd5f-ce31-f33ad2d21699"), "0802", "Arhavi", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 102 },
                    { new Guid("dafebfe5-594d-9464-a871-dc6e9dadde4c"), "2504", "Hınıs", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 315 },
                    { new Guid("db145cf0-eb92-a64d-2193-5c2b666f19c0"), "5416", "Taraklı", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 740 },
                    { new Guid("db154e39-4d4f-beb7-a85b-f022b3792c13"), "4112", "Körfez", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 561 },
                    { new Guid("dbea2074-d728-08cd-d25b-90ac321db530"), "3510", "Çiğli", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 470 },
                    { new Guid("dc2c53e4-3d1e-61c3-c75f-1eddaf6c7bc0"), "5405", "Ferizli", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 729 },
                    { new Guid("dc665578-61e7-d0ea-d07b-f047407f815b"), "2704", "Nizip", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 349 },
                    { new Guid("dc84d47a-e56a-4d5e-3da8-70f2f9521f15"), "3717", "Seydiler", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 515 },
                    { new Guid("dca9a6a6-e346-6752-38d9-f74a55765654"), "1706", "Eceabat", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 213 },
                    { new Guid("dcff08d0-f3a9-0578-5a34-8b07f86ef818"), "0407", "Taşlıçay", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 48 },
                    { new Guid("dd5ececb-f05f-33a4-d5da-e175f7a40edf"), "4219", "Ilgın", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 580 },
                    { new Guid("dd7ad113-6cee-fafb-939d-2d49346c8c13"), "3512", "Foça", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 472 },
                    { new Guid("dd8e643d-48b2-5fb3-2532-cb4cbceedf60"), "3412", "Beylikdüzü", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 433 },
                    { new Guid("ddfdc533-34ca-d3f3-4d0c-707154990f5f"), "7102", "Balışeyh", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 909 },
                    { new Guid("de02faf7-13e4-2001-f9ac-19b5582f239a"), "1910", "Oğuzlar", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 241 },
                    { new Guid("de281e59-b1ec-d960-f18b-49b7ce2d0fb0"), "4703", "Derik", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 652 },
                    { new Guid("de447a59-e230-9d5a-0832-b4c028256c0f"), "6107", "Dernekpazarı", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 820 },
                    { new Guid("de508fb8-f841-6778-809c-385d51739733"), "6610", "Sarıkaya", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 881 },
                    { new Guid("de527934-e8c2-9f24-b757-8fd7e68ed3c1"), "6118", "Yomra", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 831 },
                    { new Guid("dfb643c5-d4c5-0cd5-91c9-0ea8130b00bd"), "5906", "Malkara", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 796 },
                    { new Guid("dfd3c93c-b0ec-6dc9-7373-4d2ce8993482"), "4808", "Menteşe", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 668 },
                    { new Guid("e020ad0f-bd23-7f08-2b5e-d110452a59c9"), "3524", "Narlıdere", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 484 },
                    { new Guid("e04601c4-096c-edf6-3dc6-fc1849a98532"), "2209", "Uzunköprü", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 291 },
                    { new Guid("e04b7f14-f302-f646-eb7e-899aed288a11"), "6004", "Erbaa", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 805 },
                    { new Guid("e06e4702-c644-d30c-b87b-95d3c946d7f7"), "0702", "Aksu", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 83 },
                    { new Guid("e0723170-9e65-ed06-0df2-dde829bf1f52"), "1805", "Ilgaz", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 224 },
                    { new Guid("e12b1b4d-f9b5-0b39-4776-cd854db61da0"), "6903", "Merkez", true, new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), 901 },
                    { new Guid("e1395f70-27d7-3e45-141d-0da4cd88dc55"), "2614", "Tepebaşı", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 345 },
                    { new Guid("e1aa7ee4-bfdf-fdf6-c34b-52acce8f9bd4"), "2105", "Çüngüş", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 270 },
                    { new Guid("e1dee709-8a91-694e-e45c-a3955a64ea97"), "6105", "Çarşıbaşı", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 818 },
                    { new Guid("e216d70f-4354-339e-c5fb-ccbd90ca7c2a"), "7106", "Keskin", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 913 },
                    { new Guid("e248a65c-d702-f741-5dba-0fa715f075e5"), "3410", "Beşiktaş", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 431 },
                    { new Guid("e256c7c0-15af-457d-3fa3-bc4fae3592eb"), "4225", "Sarayönü", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 586 },
                    { new Guid("e2f90cf6-1a30-7b80-65a7-d3ab80f5eecc"), "7107", "Merkez", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 914 },
                    { new Guid("e332bb8e-165c-9fbf-1b7b-a305f980311d"), "3111", "Kumlu", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 391 },
                    { new Guid("e37ab14c-ac1b-afa7-4fb6-4762757e07ef"), "1811", "Şabanözü", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 230 },
                    { new Guid("e395421c-ad96-91cc-46be-cbc8b5f99dfa"), "1710", "Lapseki", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 217 },
                    { new Guid("e3e92612-3626-7681-26b7-db1cd47cf886"), "4218", "Hüyük", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 579 },
                    { new Guid("e41341c7-1867-b37e-ce51-da35f119364e"), "3107", "Erzin", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 387 },
                    { new Guid("e452dfe1-3461-923a-eb13-fc61fcfb277d"), "2115", "Silvan", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 280 },
                    { new Guid("e452fe48-27cd-5099-4bd7-ca9229b34086"), "7301", "Beytüşşebap", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 923 },
                    { new Guid("e48466b7-77de-2b32-433c-1e057e288b40"), "1808", "Kurşunlu", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 227 },
                    { new Guid("e4b0d72c-ec18-8638-09ad-8e9b65faf0ff"), "2904", "Merkez", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 374 },
                    { new Guid("e4fae5d6-bab4-9397-31cb-774f1bf50508"), "5008", "Ürgüp", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 687 },
                    { new Guid("e54c143a-057d-3006-5014-2e2dc137fcb1"), "2518", "Tortum", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 329 },
                    { new Guid("e56095d7-5ee7-f188-d77b-4c802ecae7d3"), "2206", "Meriç", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 288 },
                    { new Guid("e5727348-6dea-585e-2e1d-43f123f93099"), "3521", "Konak", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 481 },
                    { new Guid("e5a0fe4e-b639-d714-ad3e-c85de121e33c"), "3511", "Dikili", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 471 },
                    { new Guid("e5a6a7b8-8ff6-260f-381d-1de3e4a6d8df"), "5709", "Türkeli", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 773 },
                    { new Guid("e6d6f812-f51f-93fe-8202-2237b83b4e81"), "5102", "Bor", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 689 },
                    { new Guid("e6ed7b38-d002-7ace-2965-8195e636cb11"), "3307", "Gülnar", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 415 },
                    { new Guid("e6fa0c68-df4d-299f-e2d7-d304e7c659c5"), "5701", "Ayancık", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 765 },
                    { new Guid("e71ac279-2a64-9ea8-d104-a14e3a8ce3ce"), "3436", "Tuzla", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 457 },
                    { new Guid("e74e671e-ff18-2f32-1aed-46192377bd81"), "3308", "Mezitli", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 416 },
                    { new Guid("e7d31a08-ce3c-82be-9233-0c0c34c524eb"), "0102", "Ceyhan", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 1 },
                    { new Guid("e7f7a242-bd25-3042-09cb-b24879fcb2ba"), "2007", "Buldan", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 252 },
                    { new Guid("e8040c1d-f774-d4b5-3b9f-7772098072b5"), "4404", "Battalgazi", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 609 },
                    { new Guid("e845ae78-8e40-0378-5034-ad94cdf50979"), "0619", "Mamak", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 75 },
                    { new Guid("e84c6301-06af-4364-04e2-643892fd8ccc"), "2611", "Sarıcakaya", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 342 },
                    { new Guid("e8abc456-e6f4-9d43-b037-dc1be1913acb"), "3805", "Hacılar", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 523 },
                    { new Guid("e8cd475b-eae7-8364-8281-f3695863fe25"), "1014", "Karesi", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 140 },
                    { new Guid("e9414541-3f0a-c0a0-507b-ee04bf1bdbba"), "4518", "Yunusemre", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 637 },
                    { new Guid("e993b1d3-c0ee-adda-c19d-238c48339f24"), "5105", "Merkez", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 692 },
                    { new Guid("ea6a2656-7492-44c5-a7d7-91490c460467"), "2103", "Çermik", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 268 },
                    { new Guid("eaeed400-49ff-5d13-a54b-49769ba9a137"), "1614", "Orhangazi", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 204 },
                    { new Guid("eb5f31e4-16a9-47a3-13ab-0fe409a17795"), "5803", "Divriği", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 776 },
                    { new Guid("eb6267f9-7d53-0477-b7f4-73642980deb5"), "1202", "Genç", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 157 },
                    { new Guid("ec2e82d2-ecbd-0e1a-ab94-f0ac39ae4ae1"), "6206", "Ovacık", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 837 },
                    { new Guid("ec376856-c371-bfda-e17b-2024812502cc"), "7002", "Başyayla", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 903 },
                    { new Guid("ec6614a9-3fab-a07d-d741-d55c50a5a378"), "4514", "Selendi", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 633 },
                    { new Guid("ec98408e-6197-8ca5-64a9-f2fc657802c4"), "0703", "Alanya", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 84 },
                    { new Guid("ecc6de80-bfa5-2de8-fcc5-e680e40d3d66"), "4605", "Ekinözü", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 642 },
                    { new Guid("ecde6528-772f-b26a-34d9-a1f33850fce3"), "2104", "Çınar", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 269 },
                    { new Guid("ece45593-b076-1621-6653-432498ea6702"), "1208", "Yedisu", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 163 },
                    { new Guid("ece5f636-52fc-5e2a-4ade-d71c72077387"), "4103", "Darıca", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 552 },
                    { new Guid("ed6b5c13-0606-efc1-ec79-3715f2f08ce5"), "6406", "Ulubey", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 858 },
                    { new Guid("ed8a95f8-6157-44a8-6d2b-5ee8553be31d"), "3406", "Bahçelievler", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 427 },
                    { new Guid("edf3f832-1a7e-8323-c82a-b6636a735cf5"), "2003", "Baklan", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 248 },
                    { new Guid("edff82a3-8d7c-1448-a05a-93a213dfccac"), "4812", "Ula", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 672 },
                    { new Guid("ee6fb71c-d634-5516-cbc0-96f2cd2a2312"), "4413", "Yazıhan", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 618 },
                    { new Guid("eea8cc8c-305e-db5c-e123-5cdd4a9e62ae"), "3527", "Selçuk", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 487 },
                    { new Guid("eeb94e65-22b6-a4b0-e784-5e3e4c7a23f0"), "4610", "Onikişubat", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 647 },
                    { new Guid("eed9398e-055e-b6b0-fc6b-54068b52c078"), "4802", "Dalaman", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 662 },
                    { new Guid("ef035ee6-c750-8244-fd44-4a3db7e259f4"), "3207", "Merkez", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 402 },
                    { new Guid("ef26486b-9ac6-620a-6c9b-5fb6aa77ca86"), "4503", "Alaşehir", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 622 },
                    { new Guid("ef48bd7b-0199-99e5-7cae-24bd1efee957"), "2708", "Şehitkamil", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 353 },
                    { new Guid("ef814486-4ec4-4b8a-4d9f-6093a4d1e526"), "0903", "Çine", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 111 },
                    { new Guid("f01d877e-fd92-85d2-5562-e9af72b4475d"), "6405", "Sivaslı", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 857 },
                    { new Guid("f058ae37-e867-cbd3-5305-3cb051ee544f"), "3312", "Toroslar", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 420 },
                    { new Guid("f088e58f-7a95-a84c-55f9-de1a854a0ec9"), "4516", "Şehzadeler", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 635 },
                    { new Guid("f09159b4-bae6-70e5-14ba-6af733538a84"), "1405", "Mengen", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 175 },
                    { new Guid("f0edcacc-44ae-5b59-bbd4-1e1e7516e501"), "3808", "Melikgazi", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 526 },
                    { new Guid("f1ae16de-077c-2825-807f-a6e9b3019406"), "6512", "Saray", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 870 },
                    { new Guid("f1d6842c-47ae-565c-6863-3d2b555074e9"), "4229", "Tuzlukçu", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 590 },
                    { new Guid("f265023d-e444-1034-0d24-c4ef71c73b23"), "4102", "Çayırova", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 551 },
                    { new Guid("f2b0c1e0-3c8c-ebf5-c032-2ef0f6946388"), "6510", "Muradiye", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 868 },
                    { new Guid("f33b0216-b4b4-fe98-3ecf-cd02dba2eb56"), "0918", "Yenipazar", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 126 },
                    { new Guid("f3458f5e-2d5d-f1b6-c086-a71876aa22d3"), "0101", "Aladağ", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 0 },
                    { new Guid("f3e075f1-0684-4296-36e2-be62f49064b6"), "5411", "Kocaali", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 735 },
                    { new Guid("f3fdb635-119a-3e4c-621f-7d59930b77ee"), "4505", "Gölmarmara", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 624 },
                    { new Guid("f402bb0b-9a98-e715-0e52-1b1d47b0b9fa"), "5816", "Yıldızeli", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 789 },
                    { new Guid("f42b6196-dff7-cac8-8880-86ce56ea9242"), "2810", "Güce", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 364 },
                    { new Guid("f49c29fa-f90e-3447-8eec-ccdef5167e3c"), "6106", "Çaykara", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 819 },
                    { new Guid("f4d270b6-ecd5-baa6-2173-bd984e2adb70"), "0503", "Hamamözü", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 52 },
                    { new Guid("f545e4df-60be-a21a-1bd6-aa8bf06d5bb1"), "5904", "Hayrabolu", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 794 },
                    { new Guid("f5ee8540-475e-80eb-ca8a-517e51d3eca5"), "4108", "İzmit", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 557 },
                    { new Guid("f5f8e779-e0fa-d0c8-07bc-6dbe6f5913c0"), "7001", "Ayrancı", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 902 },
                    { new Guid("f613488b-2b90-83e2-b996-3dbf54c33cb6"), "6506", "Erciş", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 864 },
                    { new Guid("f62fe353-1bd9-b0c6-57e1-5ac7cd1c67a9"), "7203", "Hasankeyf", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 919 },
                    { new Guid("f643d606-4e75-e185-bd00-5fe3aa4edc74"), "2015", "Merkez", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 260 },
                    { new Guid("f74b615b-8146-7c96-ec56-7a3635272893"), "4217", "Halkapınar", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 578 },
                    { new Guid("f7553dcd-7abf-0489-d504-a90101d6f2ef"), "1907", "Laçin", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 238 },
                    { new Guid("f7b8ac97-746c-4d78-29da-692b3e11acf3"), "4104", "Derince", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 553 },
                    { new Guid("f7cc07ee-040f-5ee4-8ddf-b04bbad9b022"), "2520", "Yakutiye", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 331 },
                    { new Guid("f82f79c1-f5ed-0780-f183-a5a72e824a14"), "6501", "Bahçesaray", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 859 },
                    { new Guid("f8313cac-47bf-0836-9670-c300c406b646"), "5903", "Ergene", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 793 },
                    { new Guid("f8577f00-d98f-34d0-c80c-81ee4ebb7451"), "0310", "Hocalar", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 33 },
                    { new Guid("f87cd5be-ef6f-e8d7-ded1-746c400a9182"), "0612", "Gölbaşı", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 68 },
                    { new Guid("f88b58d6-faca-f2cc-30ad-29f865f6ab05"), "7801", "Eflani", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 950 },
                    { new Guid("f9236247-4918-c359-1211-8fd103dcf4bc"), "3417", "Esenler", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 438 },
                    { new Guid("f961b600-17fc-1c89-d61f-3a0598d67409"), "3719", "Taşköprü", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 517 },
                    { new Guid("f9800d71-27dd-69ae-abdb-d605c2aebc63"), "3209", "Sütçüler", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 404 },
                    { new Guid("f98b3532-1380-2429-a68a-ae2220032b67"), "0916", "Söke", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 124 },
                    { new Guid("f98e85ae-f9ec-eee8-df71-4573659dc3b1"), "7505", "Merkez", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 938 },
                    { new Guid("f9bd03f5-25dd-064f-3b02-7df8c0830eaa"), "6301", "Akçakale", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 840 },
                    { new Guid("f9e4753b-b3c7-2f07-a70f-d755b4ed0508"), "1503", "Bucak", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 182 },
                    { new Guid("f9f6f960-708a-13c8-a271-9ab01c9888ae"), "2108", "Ergani", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 273 },
                    { new Guid("fa075721-9b5a-c037-1b6b-40380c2676c4"), "3709", "Devrekani", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 507 },
                    { new Guid("fa8fdebf-e324-aed5-b29f-c009f68fb88d"), "1020", "Sındırgı", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 146 },
                    { new Guid("fb12025d-395a-170f-8720-d03c0f532237"), "1301", "Adilcevaz", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 164 },
                    { new Guid("fb21399b-f6e2-85ec-5a1f-e8701f081060"), "2010", "Çardak", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 255 },
                    { new Guid("fb36ec2b-0631-1c1b-504f-e02e47dae823"), "2106", "Dicle", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 271 },
                    { new Guid("fb3b2da0-724f-dc41-98a8-69032d450a70"), "1106", "Pazaryeri", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 153 },
                    { new Guid("fb5ba93b-d1e0-fe08-e672-d20d740ece7d"), "3526", "Seferihisar", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 486 },
                    { new Guid("fba7523b-8ba7-553b-4d12-b9035cbea5aa"), "4512", "Sarıgöl", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 631 },
                    { new Guid("fca7427e-3706-ca66-bf44-3c92d1114deb"), "5305", "Fındıklı", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 717 },
                    { new Guid("fccaf095-2e80-208b-0c2a-fa41eb9319c3"), "0111", "Sarıçam", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 10 },
                    { new Guid("fceb8a7e-0a1b-18d3-d6a2-fc0492e6c35e"), "4805", "Kavaklıdere", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 665 },
                    { new Guid("fcf1c325-fc0a-e1dd-4523-5ef6a59740bf"), "6112", "Of", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 825 },
                    { new Guid("fd0139ed-7943-761f-edc0-1f1ce53d0eff"), "2813", "Piraziz", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 367 },
                    { new Guid("fd15439d-181f-3065-0612-e0f49dc2bbc3"), "6504", "Çatak", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 862 },
                    { new Guid("fd53ca50-786b-70ae-d7b4-b993a459b287"), "3433", "Sultangazi", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 454 },
                    { new Guid("fd9d31a7-f5f5-0601-eea4-8ca6c46c9f92"), "2011", "Çivril", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 256 },
                    { new Guid("fdd072ba-03a1-bf5f-1bc6-92eb9222cac2"), "3801", "Akkışla", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 519 },
                    { new Guid("fe12886e-400d-8077-8935-d2900c11f03f"), "1101", "Bozüyük", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 148 },
                    { new Guid("fe5de94e-4d8b-7359-bb89-dd0b1f515327"), "5902", "Çorlu", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 792 },
                    { new Guid("fe9774f1-3b3e-4b24-b86b-07e1d50241c8"), "3713", "İnebolu", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 511 },
                    { new Guid("fecaf787-137f-91cb-3e19-97f7f2348c84"), "3515", "Karabağlar", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 475 },
                    { new Guid("ff078cc3-78ca-7494-ccfb-57ce13cf836c"), "2117", "Yenişehir", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 282 },
                    { new Guid("ff24d4da-a7a8-656a-b583-aa1f7706ffb2"), "6513", "Tuşba", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 871 },
                    { new Guid("ff870416-197e-acd6-ada0-04a594c58cf5"), "3302", "Anamur", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 410 },
                    { new Guid("ffabd487-061f-6f12-dc22-f4ba76f2af9e"), "6705", "Gökçebey", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 890 },
                    { new Guid("ffb68448-39c0-053b-f7c6-023955693a11"), "3814", "Tomarza", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 532 },
                    { new Guid("ffcf6aad-aec7-52b4-2b14-4d80ed417bfa"), "7404", "Ulus", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 933 },
                    { new Guid("ffde4f9e-e7bb-75c2-2eb5-56d7607cace9"), "1015", "Kepsut", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 141 }
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("04063756-480d-8e2d-df09-976b56560e13"), "da", "Danca", true, 23 },
                    { new Guid("0e554a2b-67e8-477d-3212-d6478a45ac9b"), "az", "Azerbaycanca", true, 14 },
                    { new Guid("1b540ea1-83fc-d2c0-5074-21e65c4d23e5"), "es", "İspanyolca", true, 5 },
                    { new Guid("21f66c97-4c97-25a8-e147-5094d9fcb32d"), "nl", "Flemenkçe", true, 12 },
                    { new Guid("227a3cbc-6b21-e100-03a8-8467547eb0af"), "pt", "Portekizce", true, 11 },
                    { new Guid("2a370370-ddce-50ce-8919-f3a5c607f9a9"), "de", "Almanca", true, 2 },
                    { new Guid("2dbfb7ed-db08-e890-08b7-8625e61cc295"), "uk", "Ukraynaca", true, 16 },
                    { new Guid("3549ba53-a6a9-38bc-b335-a42235bef026"), "it", "İtalyanca", true, 7 },
                    { new Guid("3f300615-618c-1331-68f2-a012bd75df93"), "sr", "Sırpça", true, 29 },
                    { new Guid("41707a24-ff8c-b58a-0f8d-43b4ad78148c"), "sv", "İsveççe", true, 21 },
                    { new Guid("4c720c2f-3d69-c921-9048-60bcb27b4d8c"), "ka", "Gürcüce", true, 15 },
                    { new Guid("698e3564-efcd-c435-6b54-83a816222201"), "tr", "Türkçe", true, 0 },
                    { new Guid("71e44150-274a-e4f0-ccfb-d7c84727006e"), "el", "Yunanca", true, 18 },
                    { new Guid("8185e52c-b524-b5c3-3c7c-6e376a661b2b"), "ru", "Rusça", true, 6 },
                    { new Guid("825bae8d-67fd-aa92-41b4-97a0892bed0a"), "ko", "Korece", true, 10 },
                    { new Guid("8714dd60-dd25-3903-cfbf-febd1d014c9e"), "pl", "Lehçe", true, 20 },
                    { new Guid("96f19e63-2ede-522e-f31d-8e2697d686e2"), "ur", "Urduca", true, 25 },
                    { new Guid("9a669d72-92ab-f3fd-dd8f-2a924a612e00"), "bg", "Bulgarca", true, 17 },
                    { new Guid("9c47147c-0a87-e74e-9b2e-7c6c07a10e11"), "zh", "Çince", true, 8 },
                    { new Guid("9e7ba066-79a7-00b7-fa27-dd5e4198f101"), "ku", "Kürtçe", true, 26 },
                    { new Guid("a61c504e-e953-7685-69ed-67b73c0ff859"), "ja", "Japonca", true, 9 },
                    { new Guid("aa4b5513-1e09-933a-cc41-47065a3c6834"), "fa", "Farsça", true, 13 },
                    { new Guid("b310c6f9-0c43-a8d7-d695-ab81af3f2dd7"), "no", "Norveççe", true, 22 },
                    { new Guid("bb1f25d3-e7ba-e4b1-4020-84914e97d797"), "ro", "Rumence", true, 19 },
                    { new Guid("c02b255f-c917-6491-aaa3-2c017d6a43a6"), "bs", "Boşnakça", true, 28 },
                    { new Guid("c7a547e6-e5fa-e2f4-4d5e-ec444bb1253f"), "ar", "Arapça", true, 4 },
                    { new Guid("cdf95308-bcb5-5050-fd40-e9bd24e76603"), "fr", "Fransızca", true, 3 },
                    { new Guid("de2fe6f5-2331-19a4-7f32-fa5701737270"), "sq", "Arnavutça", true, 27 },
                    { new Guid("efdd5e9f-4e18-873b-8260-00917e639f69"), "en", "İngilizce", true, 1 },
                    { new Guid("fef1694e-858e-87ff-37a9-c87ce29b89f7"), "hi", "Hintçe", true, 24 }
                });

            migrationBuilder.InsertData(
                table: "Provinces",
                columns: new[] { "Id", "Code", "CountryId", "DisplayName", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), "64", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Uşak", true, 63 },
                    { new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), "36", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kars", true, 35 },
                    { new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), "24", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Erzincan", true, 23 },
                    { new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), "53", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Rize", true, 52 },
                    { new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), "04", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Ağrı", true, 3 },
                    { new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), "22", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Edirne", true, 21 },
                    { new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), "12", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bingöl", true, 11 },
                    { new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), "42", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Konya", true, 41 },
                    { new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), "20", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Denizli", true, 19 },
                    { new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), "01", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Adana", true, 0 },
                    { new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), "68", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Aksaray", true, 67 },
                    { new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), "77", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Yalova", true, 76 },
                    { new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), "07", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Antalya", true, 6 },
                    { new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), "35", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "İzmir", true, 34 },
                    { new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), "51", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Niğde", true, 50 },
                    { new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), "43", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kütahya", true, 42 },
                    { new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), "46", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kahramanmaraş", true, 45 },
                    { new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), "56", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Siirt", true, 55 },
                    { new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), "60", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Tokat", true, 59 },
                    { new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), "10", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Balıkesir", true, 9 },
                    { new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), "13", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bitlis", true, 12 },
                    { new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), "29", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Gümüşhane", true, 28 },
                    { new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), "80", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Osmaniye", true, 79 },
                    { new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), "48", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Muğla", true, 47 },
                    { new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), "62", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Tunceli", true, 61 },
                    { new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), "21", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Diyarbakır", true, 20 },
                    { new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), "32", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Isparta", true, 31 },
                    { new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), "69", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bayburt", true, 68 },
                    { new Guid("749874a3-dad5-8938-70fc-a581bf400444"), "03", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Afyonkarahisar", true, 2 },
                    { new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), "28", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Giresun", true, 27 },
                    { new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), "17", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Çanakkale", true, 16 },
                    { new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), "11", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bilecik", true, 10 },
                    { new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), "33", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Mersin", true, 32 },
                    { new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), "50", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Nevşehir", true, 49 },
                    { new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), "26", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Eskişehir", true, 25 },
                    { new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), "44", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Malatya", true, 43 },
                    { new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), "76", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Iğdır", true, 75 },
                    { new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), "19", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Çorum", true, 18 },
                    { new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), "67", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Zonguldak", true, 66 },
                    { new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), "40", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kırşehir", true, 39 },
                    { new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), "47", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Mardin", true, 46 },
                    { new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), "72", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Batman", true, 71 },
                    { new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), "78", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Karabük", true, 77 },
                    { new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), "61", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Trabzon", true, 60 },
                    { new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), "25", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Erzurum", true, 24 },
                    { new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), "55", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Samsun", true, 54 },
                    { new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), "63", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Şanlıurfa", true, 62 },
                    { new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), "16", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bursa", true, 15 },
                    { new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), "71", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kırıkkale", true, 70 },
                    { new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), "49", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Muş", true, 48 },
                    { new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), "81", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Düzce", true, 80 },
                    { new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), "31", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Hatay", true, 30 },
                    { new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), "75", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Ardahan", true, 74 },
                    { new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), "58", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Sivas", true, 57 },
                    { new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), "41", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kocaeli", true, 40 },
                    { new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), "65", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Van", true, 64 },
                    { new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), "45", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Manisa", true, 44 },
                    { new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), "39", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kırklareli", true, 38 },
                    { new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), "23", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Elazığ", true, 22 },
                    { new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), "08", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Artvin", true, 7 },
                    { new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), "54", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Sakarya", true, 53 },
                    { new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), "30", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Hakkari", true, 29 },
                    { new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), "37", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kastamonu", true, 36 },
                    { new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), "05", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Amasya", true, 4 },
                    { new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), "18", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Çankırı", true, 17 },
                    { new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), "38", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kayseri", true, 37 },
                    { new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), "74", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bartın", true, 73 },
                    { new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), "06", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Ankara", true, 5 },
                    { new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), "73", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Şırnak", true, 72 },
                    { new Guid("d85db233-5de9-9d19-cd18-eab551693140"), "02", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Adıyaman", true, 1 },
                    { new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), "70", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Karaman", true, 69 },
                    { new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), "15", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Burdur", true, 14 },
                    { new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), "52", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Ordu", true, 51 },
                    { new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), "09", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Aydın", true, 8 },
                    { new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), "59", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Tekirdağ", true, 58 },
                    { new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), "66", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Yozgat", true, 65 },
                    { new Guid("f0592215-05a0-6a49-3096-f59843037897"), "57", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Sinop", true, 56 },
                    { new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), "27", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Gaziantep", true, 26 },
                    { new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), "79", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Kilis", true, 78 },
                    { new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), "14", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "Bolu", true, 13 },
                    { new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), "34", new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"), "İstanbul", true, 33 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("19dd32b6-c88b-38ca-a652-2f7684b66c9b"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("310defc2-7751-249b-e14c-714d111c3bf9"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("334dfa42-b719-48bb-1c40-dcc29142a55f"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("339ec35b-b804-b34e-1411-177ebf460af6"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("3428e0d1-3bb5-9d44-fabb-154ca81d52cd"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("3ccbbfe4-5a69-93c7-8fc9-7c9dd351af4f"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("3ec7b43a-80ce-3386-358b-2be79299ca2b"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("470fafbf-e115-1a03-2a93-402d69a979e2"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("4805face-e426-2c07-87df-a89b47709c3f"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("52b6c985-f549-8e2c-2086-98470d74326c"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("5a044f5e-d300-13b6-eef3-93ee19b91721"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("67bc4e14-dbe5-c887-d1db-9ed24c18e55e"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("85294520-2815-4b48-1983-ec3abac2a408"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("96f906d9-d34b-b5c6-19d6-3b8c0bf26aa3"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("9ad1b2ee-8f01-1148-4d3c-54a4aba56961"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("9cfafafa-8acd-9521-f4cd-e2fb1959ef16"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("9e4bbd94-33ec-29d3-19b4-72a71d3d39f8"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("a1dcfeb2-e5b4-792b-8c6c-8007fffd2abc"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("a3334d2c-4306-a771-dbb5-36426513db1a"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("a54d73d4-006f-5863-c6b7-c6b0597e1406"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("abf7428b-dd51-fa6c-1c62-08ee4fe937b9"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("addb5ad9-e8e8-9a93-401a-1d1448309ab7"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("afa01412-a075-56e4-2869-4d57b6d09429"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("bbe4c8c1-5c41-fff1-beeb-3bb273bc1a74"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("bebe0683-3fad-a9cf-4d1f-c67d8500d359"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("c0031388-a2a2-4e8d-954a-c464181c04b6"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("c15ace5e-ffd1-e75b-cf4b-b7a2139fc0a5"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("c3657d85-8158-bf6e-25e2-f4d4d5bde7ca"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("cb14515a-62e1-60c4-4088-e64010ec9e29"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("cdfc042c-436f-ba43-f7b8-104f0c739317"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("d2ab7e74-ff1e-4f61-af87-53160afab9a3"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("d3c7161e-6bc5-f484-8ba3-b68b985b0be9"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("de096156-7c0a-d247-584c-0b38e2a3451a"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("ecf10f2a-7ce5-a914-586a-1813478d2cc8"));

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: new Guid("fb8f7265-b628-4534-335a-17853e1b9729"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("00179d17-0dd5-fb8c-5f9e-7786b5daf27e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("007cac78-e8d3-3a9a-9bc2-97bb513656c0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("007d6d54-6c36-5eae-5cfb-0bb6218113eb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("00938280-24b4-b33c-a20f-4eb8974b1e9a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0187d27d-745b-4486-9547-799ac39f006d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0194d002-8576-e881-10fc-3489af96aa01"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("01de1dca-842a-47d9-ebd0-005bd581fcf0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0239e566-f2ab-5cc5-12ab-b800ec1fb68d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("024a1e0b-9067-cd58-55a7-fbc665ab9f19"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("028a6c5d-b017-4675-515b-88bcb94dd082"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0293c270-d024-48fe-d4c3-161f6a4805cf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("029a70c5-305b-59f6-da37-b22e471f08dc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("02abbd5f-7839-c08f-20d4-fc7087e8e53b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("03687df6-c493-bff5-da5d-a5671fc23edf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("03d4ae60-6bc0-64f5-57bd-86f3aee0b681"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("03eba2ac-e4bf-8ad6-631e-c3e35fd69b0a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0442b458-8f1b-6615-68cf-6b679b888dc7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("04c8fe07-7195-da88-bc0d-9eab02f09d09"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0503fcd5-f2a1-e94e-ad52-96f7eac69df0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("052ae632-0193-1b03-023a-95a142c3cde0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0531019f-e64d-8c11-d451-cf3000a022b7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0550db70-aa37-2f47-e202-b66354e7b128"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("05996af4-b947-fba1-427e-fdb3cbb3d8b9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("059a9f00-77af-65aa-22d3-add55034f2d9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("05aedf14-e473-7209-0d92-88f50de89f36"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("05d549c9-c24a-cf87-fd36-d8b0af86f5f8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0668f442-452a-1f4d-0b41-7c791810227e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("06a35f89-9d7e-29e2-d1c7-5f2c6507ab04"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("06e216da-242c-b84b-2d9f-aabbdbb7be72"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("07353e33-cfcd-8e1a-8452-b2ad60d8ce8c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0755047d-f405-7659-c8a2-74c9ddfabb74"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("07a87907-4e5f-ff3f-f63e-706690babc45"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("07ba810e-d562-ca0f-077a-705a22db1b51"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("08c64f89-122a-d034-4938-be9e14e5a672"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("08e56a66-26c2-7160-00b0-124371292a7a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("08f34c0a-4c7a-09a6-f3fe-916f8ee818ac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0913b88a-9708-9e80-6623-1c1a8f7b7bec"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("091ae9de-794b-4575-bc71-77456e4e3207"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("09903b91-f419-0a1d-069a-049c90af8c4a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0abdfab9-a9bd-b7e9-89fa-468edd5cfc5d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0b2b8885-5ac8-d0f0-2753-96afb63151b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0b467da7-7a95-9ddf-172a-3e3447669c06"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0b9c2e23-bec4-74aa-5a7f-9506a5b71b1d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0d585c4e-e5db-f294-a1de-b7588f79a80e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0d6e601c-f126-b36e-3ff3-bb0bace6f9f7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0e020603-4620-1086-e855-7539e7c9d8a1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0e86b451-f60f-aa64-b281-1315e21cec90"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0e937d62-de7f-60cb-b1ab-1a25fd666fd4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0ebe869f-1b1d-aff2-75a9-5cbb4ffd7f75"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0ec358f9-7e22-b1b0-3b4e-f3bb6b76e3f2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0ec5f580-3149-be00-b549-fc5a2f4cb298"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f2ca77c-c661-bf70-1679-57621a89a038"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f36076e-5cf8-89f8-394d-9ad0006612bd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f4e833e-0a96-ae6c-64f4-f719c335731e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f65aa7f-704e-b715-dbf2-e718dcfc60da"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f9356d6-6781-ad03-9c50-fc0ceeb3517f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0f96c21b-5f2c-d9c1-0dc5-13fdaa40d7ed"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0fb15df1-15ca-9eea-aaa7-732156372753"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0fdefc50-f43d-69c2-09c3-989733d46f22"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("0fe2eb45-7d70-2a39-168d-e36be67bc671"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1000a622-99f6-16cb-ebeb-c6be947ce042"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1023f337-64f9-350f-2a4e-acd532b9d7b9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("104bac4b-ee1e-a5fd-79d7-dccdd0731b25"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("104c8461-b927-6f1b-4790-30452489b3e5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("107600e0-342f-be51-e9d7-4210d3565fb7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1083eab5-621f-1405-84f0-b7be8c6206ac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("10916687-0fc9-cc3b-9ac9-98f573435297"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("10ca8f53-593b-47a2-d505-199f5de6ad3a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("10d98c90-0c56-1d89-25ef-123394b6e3dc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("10eb6b4d-ed48-350b-1cde-eeeb51842a96"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("110521e7-e46b-4e2a-8b16-36e69411270d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("114cd08c-e70b-07a1-0e50-6b0893381a8e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("116fc35a-a1ed-4c13-5ee7-81fb2d508882"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("11e9c93d-1e8f-485b-1ea2-57c094e620e5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("120bb334-f020-adee-b014-a03e09832f37"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("12126022-ffa2-2c78-15e9-5e0bc2521f6d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("12d6ebcf-977b-c65e-9530-32e784789078"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1331d1dd-29f4-ffe7-27dd-74d55fc75515"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("13c5415c-8335-3876-d99a-6b2948879514"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("13f26922-d129-2bec-3366-f1ee15c058f4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("140afadd-858c-d63f-3f21-1e418fe1f6d3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("140b31b6-f0c7-a416-1ad8-3d2589e5a548"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("14537dcf-35f4-46a5-3f31-d916f6ee7be1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("14585b38-dcf3-f001-7869-c19c9a343345"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1473dcef-125f-7de4-3693-e4e401bc4d31"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("14a580c2-c7dd-6609-2286-bb847e6192d4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("15052912-50d4-0b28-4750-e54044297b04"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1594b928-b81f-e2e4-7029-83d46972bd2e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("16deba2d-4129-eef5-030a-4d2020a74d56"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1743cc59-40b4-63f0-444c-5baa4331b866"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1748d4d6-719a-e6b8-b6cb-810603fd10ab"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1753c64c-1070-0faa-e028-d98164e67966"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("17b14cf8-241c-4c0a-d5ee-8b4c7cde8ef5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("17c75370-80a9-88d8-5be1-e11f929089d2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("17d23cba-e25b-cda5-08d0-b9036e331173"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("17e9548b-f2d2-3353-7b85-7a848b6b6844"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1811465b-d0aa-b3fe-9a32-8a817274838e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("18360516-134f-f65c-7646-c95f06c36065"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("18eb94b9-ce15-8c6a-86e0-f6b587b0de94"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1914252c-eb75-7cd2-9073-01862d596382"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("194293db-0e83-67df-1dbf-d96a3317056d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1962f24e-b773-a4c5-8aaa-5c71a1053306"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("197561ea-a138-81b5-cc1d-5fa99815a3bc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("19a30190-f1e4-dc90-4205-ea0b8559c40f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("19ba79da-8c36-dfdf-ba81-2527ca23bd50"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("19ffdffd-93ca-3244-4dee-2be7e91ba8e3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1a09e524-6107-a8e5-42b7-7aa61eb02205"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1a10dad0-937b-5ddb-bcad-167b6178cfc2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1a3627f4-760a-108d-dee6-09efa4469133"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1a4bfb2f-b75a-fa7f-d36f-9329ecfbce5d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1a87bd79-318a-2aba-4acb-0cdc22661d2e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1b7aa360-8fcf-8c15-2e5e-274220bd1fed"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1b97799c-3e9c-cd0e-23e6-94d7c203f79b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1bab9c53-58c6-fe3f-7e70-cc48db236aba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1c7748d5-0379-4ff8-b7ac-3db38ccd1d85"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1c80889c-e1cb-bec6-28ee-1b8d1c3da26e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1c9d5d98-2026-a08e-9ceb-01f3be8c0ab3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1c9d6663-9413-a1c5-10c0-a5f9c8ff24ff"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1d14777c-993a-4d47-fc5f-a2b659c594b3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1d3d7282-ae7b-991c-1364-34bf4de71c06"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1d50701d-137a-7cbc-274b-87611fa00bfe"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1d9bab36-32a6-fc54-5117-94a2546f1de1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1dade086-c903-d14c-a2a8-9b99561d9db9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1dec1ac8-e49c-0fb5-4ad9-d9265ec1ef20"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1ebbae63-365b-25ce-d066-d934de24a8b8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1f34fdf4-8bf1-8195-1198-4b161232e2a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1f43ebb2-c0ff-f614-2ec1-c35bd13cab5c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1f8ef7d0-a55e-4b73-271a-eabd7351b113"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("1fef895c-9415-5b32-4a00-e4faaba1f844"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("201ce73a-b717-deff-6ff2-6ea18a0c3f2e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2024f47e-4cfa-7841-8882-4f8701bd75f4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("202711b3-b50e-e3fa-253f-e5165d54c503"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2051f7c8-fc72-0f91-9eca-a247360b8ccc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20524eaa-6129-6a48-b221-94cda08eee98"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20920848-c1c8-046c-8c3c-020d2f6f0256"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20a1d7f3-b6b3-5e45-e53e-af819c18137d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("20f7e17a-8df6-68e9-c926-506badcc2d07"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2172c52c-2c82-9d93-c9f2-355f0e323452"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("22259c42-62e7-a209-c5f8-8535ac213d67"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("222a3a67-4c8c-bc4f-5e4e-47dc66c73631"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("22d70a45-87c8-b160-3782-7eaa11c89077"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2349c4e6-4cba-6ffb-39c1-d7801b78c8fc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2376b198-d420-9b69-c5e8-b7a0639a17de"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("240941c8-fd1e-7a98-a6e4-3cddbf03e48d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("24296299-a56d-1f13-24fe-ec362dd46b24"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("24796970-0a55-d380-08a8-2d455b88d312"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("248d16d4-5cfe-dc93-e2bc-0106ed90275a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("24c24367-69e2-4a4b-3e0c-9f86a22c38a1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("24d52fb0-0b73-b292-6f9f-3e2e82ce94af"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("255a8a63-5530-0b32-dd78-d903f5b4f3bc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("25833b0b-87d4-f9b3-45b3-feef3d1007ad"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("25b51f1a-c709-236e-9c5f-91fa48cfe4ac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("25ff5de0-1042-2509-f63c-2a49d63e73aa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("260c30e7-74dc-dcd9-78fb-1b74cfc82437"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("279c5bd1-ad87-3b09-09b1-ce30b5809c2a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("27b4e054-f362-2a00-ca5e-0829a407d0fe"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("280608b6-ac57-b205-d2df-0abb4768e25a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("287f1302-c9cc-1d6b-7d36-da25e3e8046b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("28f1f7bf-47ed-f12d-f6ff-2ed3055d171a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2975d9b0-7ee7-9bb1-2c9c-566e015a6f0f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2a4d25db-20ea-37d1-99a5-82c70574f657"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2ab2663f-a3f5-c431-640a-84dbeade7a30"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2ad67f83-8435-33b5-9210-a8974e175cfc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2b3823de-32ad-cf4f-7de7-906a91b9413c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2ba3acdc-cad2-452b-0307-5d0affa8e8a1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2bcf5464-96d0-fc55-54e9-fb897882a4a4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2bea3b5d-41c1-b28b-cd35-0e4b71662f8b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2bf81188-78fc-f902-452f-b56b223294ba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2c1718d5-250e-e6b1-6557-9702f40222b0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2c3d3eb5-492c-e753-9d84-b4f120126de8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2c913fca-4fb5-b2b2-e73b-37db01b0ceae"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2cb1c187-1a75-2113-faf2-63a8920e7ff4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2cd71f8a-3c50-8b73-6ae5-b6b8b4fe40fa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2cf912d9-a16f-ab30-ff36-1d13b3b1c848"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2d76347c-fbc7-e85d-df35-d2480068a0ef"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2d97204b-f8a9-efdd-d880-521a584f92d1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2dd895fd-69b9-d489-9df9-31a6b834c4f7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2ed65f36-ef33-8208-0478-d5bace00742a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("2f0b92e1-2fd9-0ebe-353a-83e6d20f2b31"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3030ffdf-7397-932e-c350-777414b6faa8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("30379c7c-80a2-7ba7-9eeb-676a8142cd62"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("30a63a6b-7b7a-3498-7e2a-5282ca34d78c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("31462072-705a-b329-599e-af0ff81b31b0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("31470dd3-a622-09b7-0c1b-1c5a77789743"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("315259be-1820-fc1b-47cf-7d2579d428d9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("31adcf3c-1dfe-a2b7-424f-d74612714e9c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("31e08767-acb8-e9dc-1996-c7869bb41db7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("31e1433c-97d0-858b-35a1-390fb04fabb2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("322ab3f2-81e4-41cf-0a2e-c2b9e387f4a9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("32672a10-a256-459c-0929-c18ff5072db5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3287733f-d933-c666-5373-2a15a5dae76c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("32a5ac19-e4b0-58b1-00f0-188e48240638"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("32e8a6dc-362c-ca71-5b9b-6a39d4720e97"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3304e2e7-e903-c76b-c352-af470f7e5fdc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("336129ca-df72-0d2e-6f41-1ffa0517ad14"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3399132e-2b16-805f-2ab6-aecdefb761d6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("33d5da1f-bc58-30b9-4d38-2edc5d3ee418"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("343c0ba2-ebcd-5664-3c98-54f48b68d726"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("35120efb-782d-3d98-66ba-44d8a3a3d174"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3562c94d-7e3b-d259-6165-783ef8eae7ef"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3566e03a-bf2c-bb4a-3300-23ac2da95e52"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3589cfa5-79af-9a62-c117-2e9b8dd878cf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("35d1a642-e8d8-ca3a-7749-7c5ef666493a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("35df9d65-a829-c85a-f238-0db0e58c77cd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3626eb68-d999-6677-3140-55be0c3fa5b6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("36693398-e8f9-677a-9071-6ec1ca67a911"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("36702f58-b2fb-071f-c840-418f38e5a30f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("36ee8166-466d-588d-afdd-5da7846e22f3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("371bad61-1f00-ced1-8f07-cacd0708dfa1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("37bd4683-01c1-7080-52fb-e9d6841dc4e1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("37cf1b37-0ab6-a665-f24c-f9cb545cbc60"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("37d17b43-eabc-52ce-3ccc-4956b3ed57d9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3812a3c0-3d70-95a9-2c08-96a3525fbf1b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("390739cc-a48f-7ce7-c5ee-f86156d606d6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("396c06c0-77a8-cee3-ee1e-8c4ce84bd337"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("39782ce8-8b89-7844-bc47-6756754589a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("397a5a74-0efa-3bae-23d7-1dc957559735"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("39905b8f-17bf-27b8-c46d-d789cbfbc412"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3a1e69f7-0924-6dbd-0007-66c80ffbc7fc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3a35a340-950c-a049-a8ad-231ad774b27f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3a514dbb-205e-aea2-cf29-651bdaf6bd8d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3aac1f68-41df-5d3e-0889-a1a44c45c204"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3abda89b-a277-11a7-54c2-369486f30e84"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3ac32fcf-ed50-9b79-624b-c53da0367bf4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3af68b9e-a194-9041-8d11-2a9b501f424d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3b6a32e4-042a-04cb-262a-6ec2a7ef1dd1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3bc71e84-d44f-b731-fe97-d387be20217d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3c0e4855-3c11-b4f5-692c-410ac571f62c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3c108f54-ca49-7548-1b61-8e2fcfc846fe"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3c74a279-420a-58e8-bed6-217fb9c4351e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3cfa699f-f65e-c45a-10cb-ed514e163ccb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3d3808fd-d2ba-3ddf-ed64-42ce90af5582"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3d6328a4-1c36-3bbf-1313-db9cf13a2caa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3e864610-98fc-206a-9c8d-1e74ef7a8225"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3fcbada4-a612-a27d-ad92-90bc2a96fbff"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3fd1d7d2-d891-d625-c8ad-aaf946a0f51a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("3fff030e-0e0c-48f3-ae86-7d4808a388ac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("401029db-4922-414b-6f0e-3811d7921046"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("404cad8f-b557-f3e6-ab4e-22d0c25bfbb7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4064ab68-696d-ac98-5aa5-ae1d0094840b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("40706ab3-1638-8195-ccac-7645219d54b6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("407d5679-81da-a74a-d408-38b0cf9de4a8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("40a4efb7-8945-1903-e087-4c467c1739e1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("40ba322e-a054-dc50-709f-47177d2fc4c7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("40c8547b-30ab-4491-6d8c-321992cd620d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("416965f4-4c25-b3d0-e8e7-70fe337dbc55"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("41748864-79db-cd36-c4c0-90e854420edb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("41cf7f8a-0f19-5529-aecc-78c217944418"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4227f803-c228-6f6d-0127-009c94c5f9f1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("429eccf2-2383-d704-cdc1-c5dd33437003"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("42ea4d04-9b0a-a23c-7bd0-0efeaf0aed06"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("432f6ce4-e658-2cc9-a596-428f5926f78f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43646202-f1bd-d15f-fb5e-471beb122134"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43819ac2-685c-8a93-afbf-ac113bd6c571"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("439015ef-ae0a-a972-13d8-49afc34506c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43a64314-5720-45af-522b-6cee1ac5dc21"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43b7a6ac-7be5-df9e-dfa9-00a9ceb8c7ea"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43c6cea5-5ad3-5d6d-4ad0-4a625b07bbac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("43f89a00-9e19-88c8-b088-606aacd2e39e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("44656a4c-615a-12d4-1f13-6d7b88b52259"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4470e27e-f003-f101-bc89-00722d61bc4e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("44d78bc9-8a32-bf9a-8aab-958559f52163"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("44e2409d-dc06-5d7f-3a6c-6f997a9e77fd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4514203f-2a80-9aed-9d89-b32e7fb84bf0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("454694eb-de3f-78e1-a526-6f5d76fa4290"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("454aa006-7c2d-2f2f-103d-46e75fa9a39e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("45784aee-689f-4867-4a84-7496b67fb8a6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("457a4421-5276-55bb-956e-b6341c93e495"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("45c2365b-db9d-0cf1-8378-58cb889bac8b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("45d271ba-7f0a-fafa-76fd-c22f61278f58"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("462128c1-9355-4322-8fdf-4de7beb31585"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("467913ad-dbb8-87b3-281c-d661736acfda"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("468ed254-c6b4-74b4-37df-1a03dd1dc4ad"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("46b36ee7-0afc-6426-a3e5-bfe2a1a8ff49"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("46eb7293-6b87-b464-d227-ad70a9e45f00"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("47446bb4-8b68-8830-9e5d-4db5125ed0e2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4764820c-e9f4-0a9c-8963-4fc597f80e64"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4776fca5-bf25-78c1-2296-bf43b8cbaf91"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("47821e95-ff26-19fd-cb88-259ed32c1825"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4789c4f2-e5ff-5122-03ae-b7262e43af55"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("47afd20d-a183-d02c-43dd-567ec36783ea"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("47b555ae-c29a-5808-0371-584418d43813"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("48365ce2-fcc1-6622-76df-51d9ad3d5e53"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4859093b-db5a-6bc7-d933-03f2896fbb95"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("486f19c2-d2a8-1805-91fd-b8d3725f6c70"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("48ace69f-8ee1-ee1e-025f-263e82636e72"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4924adeb-da83-7468-7281-10ff918db508"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4938c3c7-cf3c-a5e4-d2ff-c225dd09d85d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4993a8f5-22ab-7936-a6e2-4540824d0895"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4995ad5e-b1db-d7f8-e8a3-9047351fc792"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("49a39b93-c1e2-6b50-5c00-bd5d7df237ba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("49bb282f-c3e9-d656-61f1-fc7b19fa8063"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("49e569ae-de67-5636-96f6-7096855afc9c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4a5cd184-2b23-c8fc-7467-6db6989a0a69"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4a764bce-f68a-a013-7c25-d8263254aa58"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4a8cb38e-5ccd-260b-6381-762c3be84e32"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4ac48d81-596d-1ec4-e11f-3964e606f04c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4acc8665-acc6-dc9b-d24a-43da843628b8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4ad9d0eb-a114-7290-80de-2ec1ca4f846e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4b605dc7-dcc0-ffdd-070f-06e555b9cc84"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4b8f9da7-955d-bdc7-64e5-bf114feb22df"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4bb1b3b6-8524-e446-79fc-c35cd11e3995"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4bc2028b-71c8-2d79-82cc-5eb655a83b3c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4c9cb824-16a0-e28d-27c2-6299b55a0e99"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4cc3c3b9-230c-6339-46d0-ae8974ff8f51"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4d06578b-fc77-4e3e-b999-c32e9855a4ab"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4dedd4a8-c18c-d145-ba42-82e9cdb31d7d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4e08f818-0a06-e7cf-36bc-4d61e85925ab"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4e6f7bcf-06ac-50c9-1c2a-f98792b86a4b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4e8ff246-96ae-5025-217d-6b303afcad41"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4f47e290-4e25-e34b-da9a-5924eba0fe19"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4fa69017-36e2-ff67-42ff-19eae6edd31f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("4ffddaf3-d9ce-2832-801c-8ce1902bffb4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5028724a-5f9c-1fe4-dffe-331457db06f2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5032aff0-8c8c-9123-d825-6348c5acd9aa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("515eab35-40d1-e53c-4f0a-b9e936bbfccb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("527dc86f-fac5-9464-48bf-45442e666b65"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("52a76a87-3d8c-5fcb-b60f-40b98692b3ee"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("538738d1-822e-906f-0453-06fab8c9bed0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5388a04f-3e4f-d253-5633-99c26d1a7818"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5397662e-3348-bf6c-61dd-1c8ba5ddd1d2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("53a1e9ad-9ac2-94a8-4fcd-467a9265dd61"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("53e1d86c-4a27-f598-0c64-ad6db24a5270"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("53f9b317-03c2-b92b-7969-bbaeea51cacb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5463bd87-2fab-e2f8-3e47-04666c708221"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5481d3b6-a7c5-97dd-c49b-1dbf721bfaf2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("54cb9217-7532-36ba-ac0b-af46370d183f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("54f04cee-2d44-beed-189a-bcc8865d9d96"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("550a8669-3d66-227a-c2d5-ef09931b7e73"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("55750b7f-cc76-0d51-a350-986eb88bfb76"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("55c71578-b9ac-0e2b-5048-ec9c0bbdac99"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("55db2f5e-35a4-86b8-1bee-b7ebae146438"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("56da1c4e-18aa-1207-3781-8316e1f56ea3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("572153b4-cf8b-1bbb-4612-ee1d42313566"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("574bf7cd-e16d-bb75-e82b-d369ed278962"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5797e912-0106-46ec-3f4d-716926f6c0c1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("579e53c0-f214-6210-4ed8-344430354f49"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("57d1a26c-47ba-4203-c09f-7d86eaee477a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("58782f6d-8cfc-8c58-4271-f74913de9ba0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("58ab9b91-4ad7-f736-a03f-c721d1f277a4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("58c8f933-4046-1c41-4f4d-632d37ee89e3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("58ed6855-389a-18a9-3027-6cd756a56b50"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("594166f2-17d1-b8e1-a016-1781b5e6adde"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("595b6156-eb1c-f8fc-96d1-dd25d222243f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("59b69028-568c-106b-0c99-9f672ac64c3a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5a24d4f9-e9c3-67bd-8682-d5dc516392db"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b167d78-b265-d2de-4b46-eddd18fa2f7e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b43ea66-cf60-bab0-69fd-bef30993c907"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b5ef148-3d24-e970-cad9-3c2942abfb6c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b769350-3d45-9dde-4b2f-2997b8e05733"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b782054-c364-0aba-70d8-7f86c2adb910"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5b85f39b-5d74-5dd1-4dfc-eeaa4d5c0c54"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5bd88228-0545-d37a-cad8-4a704e62dc7c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5c090e57-3628-4786-7595-c489f2609c7b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5cadb949-bfb9-a679-7f60-c99f07f87428"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5d541259-7a86-773c-f638-7fb7c985a75e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5d6108a9-50b4-b305-bf84-65e516a818d2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5da6ffb2-40e1-0378-3ab1-661bac9205e2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5db09ae6-444a-d4a5-3c16-60c347b8ac48"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5dbec8c2-f316-e7a7-8063-a2b6ef6c8eb2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5e0b3ad5-929f-0c63-bc63-1cec522c245f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5e213938-4ce6-4c2e-8c85-76a50be75621"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5e37640d-e781-4164-c129-0caf9bc33176"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5e7638fd-ac3b-b3e9-528a-893b601d5124"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5e82613f-7810-7aa0-6b20-1ef20b09135c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f0e3969-4300-6647-e8e7-b252f2939ab4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f1d3d4a-c05f-f4df-19fb-1522bad55b36"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f29e889-7dca-4afb-41af-5ae77e0221b3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f2ea40b-21d3-4b04-7655-dc3d9f582b50"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f5ae402-ea77-3ed2-09bd-98a2f14e0829"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5f64d6da-b6c9-7671-0680-ae916e90193f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5fa96775-c1ff-3091-b676-6c8c4cdac058"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5fb2db04-fedd-78fd-be65-f8159ec54802"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5fc0f2a5-9460-fb52-c633-30b858806a94"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("5fe37211-5794-1239-095f-9688b73afbee"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6014ffab-296b-f782-0570-26b4dd1f86e3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("60abd7fa-ff96-79b1-862a-c410811636b9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("60efa5f9-1746-c6dc-c91c-22a3f49caf47"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6170460f-a3cf-9fba-daa7-ae8c92c71437"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("61a76485-69ca-cc29-3598-d428add07406"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("61de2ade-94ea-f45e-8510-a09531e96a97"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("620faafb-cb6a-58bb-8c71-8b9f9427c5a6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6270433b-6353-4161-8925-bda9905afa5e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("62ddf38b-2762-e442-681d-e6b4116b5667"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("62e79b98-9d69-0e97-cf1c-406b69626290"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("63a9decf-cad1-5777-0dd3-01216cbb1048"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("63f3dffe-db98-90ea-b7d0-79ca8ece118d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("64146f0e-9866-3a29-c473-cdf5e1803e23"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("64646a1d-dc04-df96-0dd0-ad5714d0a677"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("64a88e33-c968-6878-91db-118e77cff765"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("64a8a9c8-4317-0198-f68d-78d4165236fc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("65286f80-d508-81c2-6635-a6d255ab013c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("654c27f3-c447-6bfe-1368-f71e862173ea"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("65785331-530d-77e6-e27b-47af62425228"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("66131569-c770-5a46-a365-f25b466e6691"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6650717b-c5ca-32d8-89bd-855c8ba31bc4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("66a7f653-e559-e13d-261d-71c9e16d580d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("66b3111a-6057-b068-a5d9-11041585774e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("66c4e39a-ddc9-f740-631d-2602d4a9f123"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("675159e5-7f1a-ace2-7d9b-1b996e98fe81"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("67947715-e087-b3e7-3adb-9e5249a4db3c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("67ed840d-8c0f-196b-7d92-8fd21fe913d7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("680a5795-bceb-2192-4154-92898154c489"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("681e8eca-5428-d598-f9fc-c29dfdab7978"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("68b83ef9-eb66-91f6-0a5f-655d18557101"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("68c329fc-1889-cd62-9056-09d8b6e1145b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("693632da-150a-60b1-f979-799db7767ee9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("69363856-a288-bd8b-c387-713341fdeeb6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("69707d3e-6954-3fbd-54a1-ae0a30729a17"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("69e17305-9e1a-2892-0c58-c2c7d3bdf076"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6a095501-0881-364b-70f2-3c1923697ebf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6a28492a-d964-d1fa-a6c2-da35a60d7465"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6a80e275-90f3-eac4-82a1-01739171ca42"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6b05f4a6-d365-2c88-4209-e3ef99c5cd45"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6b858cdc-2013-3fbb-6cbe-0aa66415c688"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6b946810-e2a0-e40c-4244-e1df88e8f8a7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6bc6a89f-ed5c-1abc-78e4-97436d2cdf68"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6bf52d55-2ccc-e2a6-e4ea-8279757db070"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6bffe4d9-fa11-2c6b-1f17-ad40f7ef6b51"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6c150530-c6a1-a21d-544a-69bacd8f451c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6c21da57-8360-d69f-e58b-1e6b4595f302"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6c3a5a49-da15-b6ab-8a92-45ed59f2d329"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6c44d397-8c01-94ee-05a9-a66210666be1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6ccde6f7-060b-c505-43ab-e951acb882e0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6cd21196-6d75-16a3-effc-8666529ecc2b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6d56a3d0-2d42-6506-8d71-761de33c5e0a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6d6a9d94-63e1-2530-f4cb-ecd03c57aa72"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6d7fe760-b8fa-ca22-1ae7-68cb9fae70b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6d83ecd5-53bd-3b72-40b4-3ebce25f84a9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6d93d76c-f8c7-e2d0-a8f4-0ac8fb8f5731"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6e1ebaaa-3ffa-cb79-a4a5-7fd94fda8d37"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6f406ca4-69c9-5000-a40d-f2ec0e5c0e72"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("6f709d57-e883-b4e2-0ea6-ae10c2e2173c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("707d5933-1dfd-3e31-4214-ff24563f6654"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("70aee598-75ca-8ab0-f7d0-ecd798bb7512"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("70da7b4c-eaa8-1a66-e668-86d1d6a8717d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("70f1428e-07df-f56f-358a-e40e9e5ee85f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("716aee55-c66b-fc49-ea8f-0a88871e3a88"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("71a4dee5-443e-14a3-9486-8adcef53537d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("71d4563e-c1ae-6189-8489-72f7be41f2b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("721edc0c-4a65-cf41-20bd-0dc057db4576"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7295a940-0ea3-8b10-9ecd-83f7beb56b55"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7298969e-d165-471a-cd3b-65a492d042c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("72b7bb5a-1630-a95b-bfca-4744b0292039"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("731f9835-c900-a01b-0f8c-91f153d4cf47"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("73fb4bc9-9af7-9de8-fc5c-abf6ed007a24"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7455c696-0629-02e5-712a-425f5c696d37"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7540edd8-ec91-5710-0ef5-913bc727d428"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("75578ea7-2ba4-f3d6-94e4-79a731ba1cbf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("756b8e1c-4136-1d44-9acd-35bf2861b5be"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("75842dc1-b74a-3f67-0a8d-83b757572b9f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("75c30990-72d3-1df1-bf80-ec947ca19d92"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("75c4361d-81af-6b93-977c-31d0cbc5bc41"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("75f985ac-93af-0480-c033-20e87059660f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7614eb83-84e6-4015-0cef-302b9197600c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("76677dbf-42bb-cf35-966c-1cba423a3e1d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("768ebeb6-a859-cf64-3ca6-7cd66756320f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("76c82282-6ba6-8baa-fa02-24fa663c7f6f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("76fc5951-6c24-05f6-c7d1-39223998bca3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("778ee021-25c2-ec2c-c74c-ea1d4d27cc74"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7798608b-a40f-5d01-dcf5-781dc3d1db8f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7799f2ff-2300-4e1e-1016-ec97655c3d47"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("793decc1-c90f-1145-fb7c-04a7f3c7dae6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7a571c1b-161a-52c6-d9b1-d210fe3664e3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7a77f453-0dc8-f3c9-53c6-f497d7aac453"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7aa863d5-60ce-4f43-bb8f-82a6216f7812"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7ac77975-64af-360d-61a0-3259f0d8fb55"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7b33f287-ba83-8d17-227f-75216edce139"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7b37b89b-0428-f099-122f-fc8b83f2632f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7b7ca908-2413-fc9a-68e0-4551fd495d22"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7bf5b221-0457-9eb0-0fd0-443e07e6a0b3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7c1a824e-9fa4-b585-5f81-84a0196224fc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7c29a90b-e2ed-e581-17fe-837797e58c0a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7c55c12b-32ea-ee48-07cc-a019d306cdf6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7c7db3bb-bf23-496b-7f6e-721d6058c4e9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7ccabb50-7cf5-0b1c-d679-6cb22ada83fa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7d23ad8f-9792-a9e8-6497-df59c44cc18e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7d3212dd-dd9c-67f0-0434-8ed46a93ef8b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7d71b256-cd4b-2c41-e93b-6260acc883a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7d72500d-99fa-0258-7049-fe84e5bbef8b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7d7a695b-5d31-2bdc-6686-f3f7b75da558"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7dcd0d2b-d076-5301-c37d-db71fd3f929d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7de5e5c6-8c6f-037a-3f54-6971cbf76663"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7e7af28a-dcc7-6ecd-5689-08e2caace578"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7e92f1eb-bc41-f3e8-2759-3e020d0a1661"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7f63eb6d-3745-2b24-c68a-3b17a0c30339"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7f9ee06b-3387-5d43-02a9-ab5b6f3de62b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7fcb5da2-a157-cb04-771e-affa6809a611"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("7fff01d0-ab62-5800-3df4-0970543ea2af"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8008ebd4-64db-a68e-221d-688f827a8cb1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8031cd42-7fee-11d7-7af2-2ddeab8bb79e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("80890687-5b75-284d-807c-dbed62d6ae6a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8090e2cb-604b-ffaa-fdca-51d7886cc7fc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("80a32e56-065b-ab5f-ff39-2536218751bd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("80b3bb04-fccc-ad56-9bc7-723aef635cdb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("80e56c02-5989-c64f-8c1e-6bbecfceecb6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("81de7abf-fd6c-6a3a-efcd-e19db8f610f1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("825b27b7-904f-8d32-46c9-b3f09378cdfb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("825dbc34-4d00-75c8-0eb3-2b5d2eb6e3a9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("82633a06-dac2-6015-38fa-a5d84e7b9c8e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("826fc047-50a4-27c5-d916-f38fc94da30a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("82e85400-bbf7-3cdb-9675-2b5a3802042a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8373b8a7-eac5-78b9-c090-4c38980fb32d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("838b15c6-dc6c-05df-4cfb-0b857a6033b8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("83b403a3-ce34-5bc0-7ea7-309adfb1ca27"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("84140825-959a-465c-661c-076fffa991d5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8414f20d-df17-eb2a-ee34-2a17b1c2b4b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("84208953-82fa-092a-65e5-7828b54273a8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("846d94df-68f2-9dc1-f040-1d4bbdf8dadb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8484635e-b304-7605-0010-e0f3281506a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("84f9e3e1-c5ed-6ac1-95f7-53fe76199bb8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8504ece9-e987-16c4-8052-ef39634479b2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("85113758-7084-4ea6-8855-842f65bd8ed3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8527f38e-0373-397c-33fb-b489484006da"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("85416a9f-9b6b-a4d7-628b-bc2ef7427300"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("85c964dc-3ca5-0703-a155-635297259fda"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("85e64730-927e-7f83-36c0-a3ce3a436435"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8650ad30-ca3e-b4c3-adda-e5829e4a7cd2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("867bba55-e925-1b30-9634-343b91f5146b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8688d48f-ee3a-f4db-a634-5b187cfa9bde"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("86a35804-0b7a-83d0-a723-1260208ef4d3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("875a92d4-ebea-3f22-1ae0-68cd600838f0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("87745af4-ba91-c4bf-4ebb-3734e7a462dd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("87b37628-19b4-2ea8-624f-d90271db89e0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("884d0b4b-7b1c-dd7d-fdba-bcdd0ebc08f9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("886db355-3c90-2122-c831-05302f948896"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("88c916b3-fb44-4441-b058-ac0edb1e2a2e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("890751ea-c24e-33ff-d259-c70bc6be4c19"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("89148880-f269-54cd-3750-eb087ee7a3d6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("893b4efe-c877-4681-7d0e-dc846c0e7df2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("89a2cc4e-c9a8-26ef-0493-12b1e42fcba2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("89bb02a3-880a-ecad-0af4-8e679f61a62c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8a39012f-aeb6-7620-e434-2a3d21d25441"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8a3b7c67-46ac-bef4-1da3-6e3c907289c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8aac744b-ce32-b16b-1dbf-983d836a6c84"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8ab577ef-c06a-0400-b518-865db7768c46"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8b01e8cd-73c2-0b01-2f49-2d9712d0518b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8b61109f-93b5-fd1c-4682-84a6f2b65ec4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8ba4f467-2dd7-7a66-6058-024a53abbe14"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8bc377e4-eeaa-6719-796e-459333f0f090"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8bc415da-ff43-6799-45ff-2ec8b040978f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8c4265c7-d845-736c-9f97-a1dbc874e3fd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8c42cec4-2904-a0b6-9112-c31d03637b36"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8c947a40-05d4-1c42-dac5-88c802ee7bd1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8ce4d199-3d0b-2d31-8262-390bbdb9bd5f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8cf40d7a-da54-dc1b-3580-e55d867fd302"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8d5bb10e-3fdc-7ee7-c6bd-2393dbafe3c8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8d5e138b-d183-8c21-1ccb-cf8dabb87e5c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8d63b22c-9434-6abf-1abb-9f6a346bcf5f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8ef38b58-8490-7762-c3ed-1d4807cbe1a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8eface48-f830-55fb-97b7-2297fa55cb66"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8eff5bd9-915a-31a8-9a45-6986c18f3002"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8fa4932a-cf41-9e50-1bcb-916fba429c42"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("8fde2518-f714-133a-19b7-086bbfd6581c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("90485d8d-10df-c011-01a2-b850e9af0c1c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("904abd3b-f437-1d56-ba0a-fb575e1f05b3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("907d0baa-6a42-f286-671a-a107e14bf6f1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("91144ec5-9581-4916-c426-224f164702b7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("915b235e-a1c0-daf1-e4a4-cd3113127bed"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9182d202-a09f-d9b7-f105-6714a5fca87c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("918bfaad-7f9b-c6fd-dfb0-0b8d89a8bdfc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("91f19438-2267-d423-38d3-3d5a4c206943"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("91feba61-bc38-95ed-4c7a-272d53b76f26"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("924ddd80-9410-358d-08bb-87da78336a50"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("926bbb94-0623-0e1b-e7f9-1c7a0d9ad315"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9353ead3-2012-d83f-c1f3-c108882e0726"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("935d8741-8187-fbac-87e8-01686d27fe62"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("93849d88-9b27-fb57-0999-5f28fba76a51"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("938c102a-73b4-a041-e66b-b1da108da246"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("94b3ea2d-7570-f546-ad0d-c46126924aa9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("94c2f857-b0ae-1690-3c2d-f79f1248ef18"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9563a2cb-627d-d867-b220-f57b82e92c1e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("95896ff1-978b-4e1e-dd23-ad73eefc6d6a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("95d2610e-1284-c7bb-5009-4236742deac0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("95f4a3d8-d354-3560-ebf4-55fc9bcf1948"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("95fd376a-e70b-ed22-cc06-2847d42dba32"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9657d8a7-3cea-660c-e087-126bcf36fc4c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("969bf133-7696-2ce4-e921-fc72f4a83b7a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("969d314a-74f5-6175-4d16-160349c290b8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("96b552c6-5380-971a-32f0-1ec1ba9d46f2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9716f0c5-6a55-d915-3091-dbfd3ad2f7c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("97830230-79f8-d3b2-0f9a-34792547bfd1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9863eb76-d3ff-bbfe-10cf-3c8b8730c1cf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("989a4319-d1f9-f42b-7cd5-164e5c7b333e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("98baa8ac-5685-4471-5b89-4579d0fb17e8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("98c25074-10fe-455a-1617-b1b50e69a531"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9936e3f3-0a7a-ae35-ef0c-273a250a60b5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("99c5f876-faea-c451-94ba-c8f459795554"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("99d54a9e-f003-3bed-554b-2a5efa9f62dd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("99d95882-7ce0-2796-0eb5-469df3edc8af"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("99ffbb26-4d34-fae5-ccdb-fed92fc45b42"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9a0065e1-c074-de1e-f6c4-91abbb046fe5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9a4c1f6d-4efb-b968-da19-1cd6623c728f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9ab36369-ba1e-fb0d-8b9c-a4014b2049dc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9ac6bb65-7678-9938-9dec-969572371e1e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9ade470a-2603-dafb-a0dc-a4955713a7f7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9af133dd-14dd-1c52-cb0b-58ae9081c3d9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9aff0dab-3a7d-f91a-bee3-1be5c1a354fe"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9b358a90-31da-24a0-c3eb-aa03ee0bff43"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9b810d9d-1755-ae65-63f6-563a5abeaf46"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9c31b583-aa40-5874-98bd-3b6701bf0b6f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9c99ad0e-99bb-ffa9-071d-c00da016ea97"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9ccaea3d-a09e-a556-94d1-53217126d61c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9d3904af-7def-4ca5-8bf7-5606cf4dfdd4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9d4f1eaa-a299-c64e-83aa-eb2615db31b9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9dc9f9d0-7713-ff21-3546-c2ae8433dd6c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9dcff53f-0506-1c39-0ab0-9bd080185eb3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9e7fee88-e48e-b111-bab6-a8effffe4eee"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("9fdf0f67-01ba-2aec-4713-06a6398fd20c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a088f278-93a9-511d-1b31-539500da18ed"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a09ca274-f187-3ce1-1c44-b9087fcbc4f4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a0b9d3e8-4ef1-d3b8-91db-089b4e5906d6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a11bdb92-effb-e19c-e791-23874879c801"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a1752844-b33a-1223-1f9b-5a402f2694b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a18a07c4-192f-ad22-ea6c-b573a651d711"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a1f850b4-59b1-596b-829d-beb7a3fa318b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a1fa1768-8132-b257-9b75-3b8d21ecbbb5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a20b0d44-a1c2-0e99-2934-c32d82ab6f9c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a24b91ba-86e1-39ef-588d-762c8912b3c6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a2ded523-9c45-232a-98d0-b6290589964b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a36d7635-5621-8d67-771a-9890d48b9c26"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a3acdc03-d800-f353-a371-04a1f7a39d46"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a49146ef-65ac-18d7-a0dc-475f76ce8204"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a5173f45-76e6-e96e-9863-6030cb6fcca0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a576b209-c382-66f8-4c5e-f6ff7b3b8d08"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a5a470b5-ffa5-d863-bab9-a45097acdf7a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a5c2a68e-184c-a52c-5f76-229a49014bcf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a5dc233c-1fd0-e702-dd71-a2187897e242"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a61ab9e0-88f6-65b3-770e-1709f312e8c0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a61b2920-f0c6-b5de-a59d-bd748bb96990"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a6793949-a372-42e6-924a-0bad6d0e2e47"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a67d53ec-4872-b1ee-99cc-5c8dff094625"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a6810063-6b99-be61-28ba-1bf8b565f7be"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a6bc0c40-1c26-663b-4db1-898babc32f89"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a6cccd80-8ed9-f8a6-38a3-222a79f4b769"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a749ca4d-0767-054f-ac11-134677c45a2b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a74a20f2-0124-8a37-ec5e-debbbd49b146"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a7799388-63ec-681c-8161-97bce01a730d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a78d93f8-a359-37f4-59ae-662241542c72"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a7e9c9cf-ad43-a075-ab79-af07de6e46d5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a80500b3-2e15-086a-7458-7ac5e8d94356"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a85a4558-f31a-ac4b-1531-799ff3ae4da5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a92f20ae-2208-a3f0-e3db-1f79a60c7de1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a9b206a5-a6e1-3fb9-8b37-ce8b1896ea75"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a9bc7c5f-e42b-0d7e-971c-e383a5fab0cb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("a9f9b263-81ec-e6f9-a600-67b4db2810e6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("aa2b8de3-ec9e-751b-c312-f8f2b479facf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("aa6ff543-280a-d106-ca4d-11eda5280864"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("aae09c91-18a6-c265-9af1-e7791f84bf46"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ab2184aa-a391-03ce-0719-b5733be396ab"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ab3bec02-414b-b095-3d67-f8083c06053f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ab4b330c-4cc2-234e-7f95-d01b5a39bf8e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ab7d4767-37ea-c786-cefb-cb8c7b93c7b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("abfa030c-da66-3739-1ad6-1cd5c74d4f23"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ac63e3dd-14eb-aa72-8075-b61ac09f6547"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ac7ce36b-712d-eb47-c5dc-119b82be4bf0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("acbf7e14-65a6-0222-94e1-db319d5170d0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("acd05185-f828-07c0-b384-031702595d73"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ad11cca0-1702-6a6c-80ad-7dff2663cc2d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ad2dc8f7-6376-eeb9-a36e-e83e33866687"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ad734da8-7f1c-44f1-e66d-f98185061b39"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ad761760-02de-aff2-ebb0-77015ea33496"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("adabea3c-dd11-ebc9-257b-5ab4bc9b4a4a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ae831c3b-06f5-219f-af32-dd1d78f689ff"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ae90b22d-6fd0-eeca-17c3-f2e944996d38"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("aeb96c1a-2d50-441a-deaf-f298d960cdc7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("aefc2e18-bd23-43a8-f32f-07ffaec66d74"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("af574fea-c57f-1b1f-90b2-263aac4018b6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("afabb77a-6106-e9f7-9916-8e57e55b1381"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("afc9a475-287b-000c-521c-05f193d76f36"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("afd6d0d3-5417-76f4-5890-400c52a17761"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("afd81fe3-0756-740e-9898-accc9a66e154"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b03388f3-f4b6-ba44-52af-851b89751f21"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b06d0380-9f36-6f2a-4fc0-afae2d6c84dc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b0988fea-d584-0f0f-450d-549707610697"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b0cdd709-0402-d6cd-a11d-da1fa443a10e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b0fdaab9-c8f7-0fb0-cb30-603e2e1dd9f0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b104f50d-3df5-a134-798d-51b27017c8de"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b11505d3-efd1-f735-d3a6-80172b3b018e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b1160eae-4d4e-35f3-051e-dde09f81a7d8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b13a8a7c-7826-6ae1-4f48-f8535a1e7df4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b13ad327-df03-a5c6-ca66-4cee91b180e1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b14cfb62-d6d9-89bc-788b-7ece2ebaf614"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b187fbf0-fde2-9ee2-977f-0690417de13e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b1c606a1-9463-5ace-4257-73cd91330688"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b2245aa8-870b-ef4f-85ce-d9a91801c312"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b2383a9d-9ba2-9205-92b4-2a3b7a3d4e4d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b249c11a-e155-2e29-a1d6-24f0a9b1902c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b25467a8-2b0a-d6a4-c876-c04fd5612114"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b25839ed-8eb9-b648-7964-58d8b89a121e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b34544eb-27e1-55e4-f5e4-c48b928cd230"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b3d8d676-dd08-bb1d-e4d9-c559ab3b610f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b527f509-4b4f-ba95-b007-440d255c0dab"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b568a95c-7395-f3ea-1a59-6116155529a5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b5b138bf-29c6-98da-6e43-31b8f85efa86"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b5cdc0df-fc8a-341a-71a3-8402ff5e7780"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b5e1ede3-0e26-96dc-b501-278842312b9b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b666b106-a19b-3a1d-7f1a-a73e894b2fbd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b6974040-0cf0-5ba1-0c43-135c2542bca5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b7188feb-f99b-6607-b99e-6c00b02346a0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b8179f00-1d3d-2d74-102f-8199e75db618"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b82994a0-3fbd-2134-c9be-8f29ecb59f6e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b8702f16-66c8-1621-f7aa-6c6b63a192a2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b8e4aed2-bf70-fe2b-0e94-12d25ba6047b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b9009057-280e-9041-1156-cf94440b37b7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b9413de8-4d7d-0105-029e-a5bda9e3ea57"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b9581aa9-3d1b-1cf8-9002-0010886f8e8c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b97421cd-6121-3c96-3e84-c3af6f1f832b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b97cc527-15ef-5c93-7f29-c6308fe38f89"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b9a779c1-adf2-e4bb-573e-2093ec00c6ac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("b9e8e38e-f0f6-ccb1-66d1-406f454a03dc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("baaf75e7-c572-d7ca-90d6-ae42e51b9bd6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bb873d3b-79bf-2283-289b-473fb3ff87e8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bba78f97-21bb-b682-70dd-5f8cd949b563"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bc096f08-eb3a-fed4-7fdd-3464481e4563"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bc8003af-a50f-69f2-1f82-238eabf85882"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bc8008e3-bf9e-bc1e-1501-9903a1bb886c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bd18f1a4-7c1a-933a-7fc8-7ef6c7fe0136"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bd3a20bf-635c-023d-84c7-a5c77b907eba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bd567a4e-7b76-d895-30e0-9bd036bb06dd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bd5dd48e-897d-6fd0-ea66-fa092c179b23"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bdf147ef-0d69-ddff-f153-4da2ec3d7172"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("befe171f-ae56-877c-5273-a309ea3bad51"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bf048e82-239d-4864-03ba-b179c42e396e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bf274e90-f01d-0cb3-1520-c906daa9249d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bf582f74-80ae-6651-d802-9f4ab32f219d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("bf677d76-08ba-8546-b5eb-fcaba56814cd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c055778c-f047-7cdd-df8e-975be17e2190"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c0764943-739a-9463-716f-bd65c580b465"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c12730d4-a1a7-21fc-335b-1dc43032746d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c12a2b5b-32b4-853b-e9a6-6c18f4ffea71"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c234755c-63b8-1958-8c4b-539e770dd68f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c26a2e40-2ec4-5a92-469d-60a3f418b924"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c2728e97-4e4e-5784-2d5f-05c0253ea332"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c273c978-0c9a-0cd1-18fb-a7e9e1b04b04"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c2b1b891-8412-3ddf-d427-db5e41efa2c6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c2b884c9-ff5f-4b14-35cd-1675a955572f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c2cc2247-6540-9dad-1746-a1919e1e8613"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c3474549-5420-8d8b-9c2d-f819b92f0687"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c365daa8-d520-d3dd-a79f-d990c0cb1565"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c3f225d6-d695-e406-c3eb-4787ba074e23"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c4025883-1bc7-6c1a-1482-e6fa1dcc2b12"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c42dffd2-6e11-bc81-2b25-176b1a2106c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c455d831-37a3-40d8-3d9e-501d6e9c6674"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c541bc2e-0d82-09dd-1eb3-86b748c3993f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c5932402-4f40-37e7-f472-3db128717d37"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c5a97f47-42b4-3034-9510-09d73883d345"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c5c801ce-da88-dc1f-5d3e-fd2694a5b442"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c5de61f5-b501-c06d-500a-41c763926099"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c605e43a-18bb-94b6-e0f5-6f89a0223cb7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c69a04bc-f164-3a40-51ba-16329ecc1739"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c707b422-2baa-060d-0ea2-129ca5c8dbe4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c7120222-24ae-e04a-f64f-89021ef6cb20"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c7648a89-8f37-1662-f3d6-655030aa840c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c820a344-b41a-dfe6-9eae-160dacb1bd96"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c8541c90-d189-4e7f-c287-a86b70473197"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c8570f49-c56a-7012-46c6-6b4ba9b22fb5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c89745fd-bfff-5529-b235-8787f0ed0cf1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c90fb872-2148-461f-2beb-169bb8c1f706"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("c943efc5-b983-fccb-9be2-ca5b699019da"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca29b1f1-a8c0-89d7-aa99-b95b07f17aa7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca4a23e6-b927-f056-9de4-dd5d321315bb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca5c73e5-a3da-e5c2-4891-83ac1f988827"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca6ee9ac-33ad-b66f-98a9-313fb70ec172"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca927d55-b991-a4ac-a186-8a94263a24bc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ca986e61-04de-0012-f16d-e38c754ffcc4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("caab9b31-f78d-74fc-df15-1df50aa1da91"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cad3e123-4c18-5f54-c1b8-30985cccfa62"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cb1c1b80-5dd9-536f-7e4e-088d80e35a3e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cb2b417a-1e04-ac7d-b1c7-7fedd6f93fad"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cb9575bb-e041-7745-b394-eb86f20e84b4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cbc13f9d-fbf4-3fd9-200a-29102c89497d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cbca2911-f75e-5c4f-9489-b177b3328388"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cbda4d3f-0160-c597-c3d0-2fffd4d22c34"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cbe6fb0d-7911-96cd-8eb5-a9e2571e5d04"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cc0f1ed5-b61b-bd52-150f-e3e31b5034b0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cd4626c3-d56c-6a9c-a6e9-491621cbe74c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cd5fb58b-74a0-ed85-7d62-6539722a4202"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cd850fa9-7bbe-1c90-366b-32a3c949c9db"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cd91af4f-f1ec-cdd6-3ae5-bd9e6a6db1c7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cdc8e52a-5c37-e24d-9cf1-4dc0677e0cb4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ce42bc7f-f563-b75a-f242-e8b306184c1c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ceaa8714-8e74-4b84-0252-4c9d74edb414"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ceb8c579-1a9f-dc42-3c63-942d4c4d4663"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("cf56a574-4bf0-c4c2-b58c-6f5e7eefe65c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d0388482-e986-e848-a0b6-f8f14e034c68"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d083ad91-0ef6-f2e2-6e53-a217f69e933f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d1337b8d-8882-53a6-ebb3-d8d36f71d36d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d1362f2a-5fbe-f286-77b9-b3247609ca6b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d1a5d0d5-35c1-409a-07b8-1db3135d810a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d1dc58b6-e562-7e7d-8846-3d4dfc46538a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d242bdbe-1990-b2ae-d04f-a1e83c867989"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d26fcd7e-44e6-018d-4c9e-74f65725fe10"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d2b0e254-9a73-4cc9-c671-b45c641a7cb1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d2b156d5-fb8e-ad6b-1970-ab811a542106"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d2b58d2e-e1b2-b874-f704-a9786961e58d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d2d4e6b3-a560-6cf6-1c1a-e4421720d88e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d3999538-e7eb-27ca-a48c-50cdd788d38f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d39ad855-2b66-10e4-773c-3941d163b4f1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d4972eca-af97-2205-89d8-23f7c2624071"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d54a7f41-44c4-9e41-bc3d-cb66bca4ff94"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d5605776-cd4d-27b7-58ca-c44b7d8cbecd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d58a635d-5f3c-4423-bd42-684fa36d7e98"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d5f2103e-a86f-850a-9433-f829a2390cdd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d64e7ebb-7521-275b-bde2-20a2ec088021"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d65a000a-3b26-e103-bff1-137fe4ee68b8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d6e34ee3-9d19-c5b1-fbd0-e9c905138250"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d72f46ad-74bc-1726-ec73-bcf4b4c4720b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d7bb9411-ce72-8a41-f3ad-b479e69ff1bd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d7bdac08-1967-e0fc-2c7d-c0dcdcd5b15e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d7f201c1-8085-1d68-5e05-cb1f83a42780"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d8324c63-6adf-97e3-da83-296eb45abd7f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d87b1e12-e2d6-ee44-6f48-affcf7b4f1ce"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d93b76ab-c94c-cd11-6168-b9ae281d244e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d9cc7896-a582-4cda-5333-593be1143ced"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("d9efa5f0-6398-4ef6-20da-1ad4e3f40c36"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("da2545aa-ee98-3966-6033-d692d0765ba8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("da7b7d79-d7bd-0c93-e5fa-b2897af2c939"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("da891cab-9211-74e0-c335-33b2f834bac2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("da9dbdbb-e4b2-4a34-1b40-686213426e0e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dab1f580-2c38-cd5f-ce31-f33ad2d21699"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dafebfe5-594d-9464-a871-dc6e9dadde4c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("db145cf0-eb92-a64d-2193-5c2b666f19c0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("db154e39-4d4f-beb7-a85b-f022b3792c13"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dbea2074-d728-08cd-d25b-90ac321db530"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dc2c53e4-3d1e-61c3-c75f-1eddaf6c7bc0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dc665578-61e7-d0ea-d07b-f047407f815b"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dc84d47a-e56a-4d5e-3da8-70f2f9521f15"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dca9a6a6-e346-6752-38d9-f74a55765654"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dcff08d0-f3a9-0578-5a34-8b07f86ef818"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dd5ececb-f05f-33a4-d5da-e175f7a40edf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dd7ad113-6cee-fafb-939d-2d49346c8c13"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dd8e643d-48b2-5fb3-2532-cb4cbceedf60"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ddfdc533-34ca-d3f3-4d0c-707154990f5f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("de02faf7-13e4-2001-f9ac-19b5582f239a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("de281e59-b1ec-d960-f18b-49b7ce2d0fb0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("de447a59-e230-9d5a-0832-b4c028256c0f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("de508fb8-f841-6778-809c-385d51739733"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("de527934-e8c2-9f24-b757-8fd7e68ed3c1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dfb643c5-d4c5-0cd5-91c9-0ea8130b00bd"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("dfd3c93c-b0ec-6dc9-7373-4d2ce8993482"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e020ad0f-bd23-7f08-2b5e-d110452a59c9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e04601c4-096c-edf6-3dc6-fc1849a98532"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e04b7f14-f302-f646-eb7e-899aed288a11"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e06e4702-c644-d30c-b87b-95d3c946d7f7"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e0723170-9e65-ed06-0df2-dde829bf1f52"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e12b1b4d-f9b5-0b39-4776-cd854db61da0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e1395f70-27d7-3e45-141d-0da4cd88dc55"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e1aa7ee4-bfdf-fdf6-c34b-52acce8f9bd4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e1dee709-8a91-694e-e45c-a3955a64ea97"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e216d70f-4354-339e-c5fb-ccbd90ca7c2a"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e248a65c-d702-f741-5dba-0fa715f075e5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e256c7c0-15af-457d-3fa3-bc4fae3592eb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e2f90cf6-1a30-7b80-65a7-d3ab80f5eecc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e332bb8e-165c-9fbf-1b7b-a305f980311d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e37ab14c-ac1b-afa7-4fb6-4762757e07ef"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e395421c-ad96-91cc-46be-cbc8b5f99dfa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e3e92612-3626-7681-26b7-db1cd47cf886"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e41341c7-1867-b37e-ce51-da35f119364e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e452dfe1-3461-923a-eb13-fc61fcfb277d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e452fe48-27cd-5099-4bd7-ca9229b34086"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e48466b7-77de-2b32-433c-1e057e288b40"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e4b0d72c-ec18-8638-09ad-8e9b65faf0ff"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e4fae5d6-bab4-9397-31cb-774f1bf50508"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e54c143a-057d-3006-5014-2e2dc137fcb1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e56095d7-5ee7-f188-d77b-4c802ecae7d3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e5727348-6dea-585e-2e1d-43f123f93099"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e5a0fe4e-b639-d714-ad3e-c85de121e33c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e5a6a7b8-8ff6-260f-381d-1de3e4a6d8df"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e6d6f812-f51f-93fe-8202-2237b83b4e81"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e6ed7b38-d002-7ace-2965-8195e636cb11"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e6fa0c68-df4d-299f-e2d7-d304e7c659c5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e71ac279-2a64-9ea8-d104-a14e3a8ce3ce"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e74e671e-ff18-2f32-1aed-46192377bd81"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e7d31a08-ce3c-82be-9233-0c0c34c524eb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e7f7a242-bd25-3042-09cb-b24879fcb2ba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e8040c1d-f774-d4b5-3b9f-7772098072b5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e845ae78-8e40-0378-5034-ad94cdf50979"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e84c6301-06af-4364-04e2-643892fd8ccc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e8abc456-e6f4-9d43-b037-dc1be1913acb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e8cd475b-eae7-8364-8281-f3695863fe25"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e9414541-3f0a-c0a0-507b-ee04bf1bdbba"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("e993b1d3-c0ee-adda-c19d-238c48339f24"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ea6a2656-7492-44c5-a7d7-91490c460467"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eaeed400-49ff-5d13-a54b-49769ba9a137"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eb5f31e4-16a9-47a3-13ab-0fe409a17795"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eb6267f9-7d53-0477-b7f4-73642980deb5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ec2e82d2-ecbd-0e1a-ab94-f0ac39ae4ae1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ec376856-c371-bfda-e17b-2024812502cc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ec6614a9-3fab-a07d-d741-d55c50a5a378"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ec98408e-6197-8ca5-64a9-f2fc657802c4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ecc6de80-bfa5-2de8-fcc5-e680e40d3d66"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ecde6528-772f-b26a-34d9-a1f33850fce3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ece45593-b076-1621-6653-432498ea6702"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ece5f636-52fc-5e2a-4ade-d71c72077387"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ed6b5c13-0606-efc1-ec79-3715f2f08ce5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ed8a95f8-6157-44a8-6d2b-5ee8553be31d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("edf3f832-1a7e-8323-c82a-b6636a735cf5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("edff82a3-8d7c-1448-a05a-93a213dfccac"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ee6fb71c-d634-5516-cbc0-96f2cd2a2312"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eea8cc8c-305e-db5c-e123-5cdd4a9e62ae"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eeb94e65-22b6-a4b0-e784-5e3e4c7a23f0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("eed9398e-055e-b6b0-fc6b-54068b52c078"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ef035ee6-c750-8244-fd44-4a3db7e259f4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ef26486b-9ac6-620a-6c9b-5fb6aa77ca86"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ef48bd7b-0199-99e5-7cae-24bd1efee957"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ef814486-4ec4-4b8a-4d9f-6093a4d1e526"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f01d877e-fd92-85d2-5562-e9af72b4475d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f058ae37-e867-cbd3-5305-3cb051ee544f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f088e58f-7a95-a84c-55f9-de1a854a0ec9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f09159b4-bae6-70e5-14ba-6af733538a84"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f0edcacc-44ae-5b59-bbd4-1e1e7516e501"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f1ae16de-077c-2825-807f-a6e9b3019406"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f1d6842c-47ae-565c-6863-3d2b555074e9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f265023d-e444-1034-0d24-c4ef71c73b23"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f2b0c1e0-3c8c-ebf5-c032-2ef0f6946388"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f33b0216-b4b4-fe98-3ecf-cd02dba2eb56"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f3458f5e-2d5d-f1b6-c086-a71876aa22d3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f3e075f1-0684-4296-36e2-be62f49064b6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f3fdb635-119a-3e4c-621f-7d59930b77ee"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f402bb0b-9a98-e715-0e52-1b1d47b0b9fa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f42b6196-dff7-cac8-8880-86ce56ea9242"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f49c29fa-f90e-3447-8eec-ccdef5167e3c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f4d270b6-ecd5-baa6-2173-bd984e2adb70"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f545e4df-60be-a21a-1bd6-aa8bf06d5bb1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f5ee8540-475e-80eb-ca8a-517e51d3eca5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f5f8e779-e0fa-d0c8-07bc-6dbe6f5913c0"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f613488b-2b90-83e2-b996-3dbf54c33cb6"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f62fe353-1bd9-b0c6-57e1-5ac7cd1c67a9"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f643d606-4e75-e185-bd00-5fe3aa4edc74"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f74b615b-8146-7c96-ec56-7a3635272893"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f7553dcd-7abf-0489-d504-a90101d6f2ef"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f7b8ac97-746c-4d78-29da-692b3e11acf3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f7cc07ee-040f-5ee4-8ddf-b04bbad9b022"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f82f79c1-f5ed-0780-f183-a5a72e824a14"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f8313cac-47bf-0836-9670-c300c406b646"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f8577f00-d98f-34d0-c80c-81ee4ebb7451"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f87cd5be-ef6f-e8d7-ded1-746c400a9182"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f88b58d6-faca-f2cc-30ad-29f865f6ab05"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f9236247-4918-c359-1211-8fd103dcf4bc"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f961b600-17fc-1c89-d61f-3a0598d67409"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f9800d71-27dd-69ae-abdb-d605c2aebc63"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f98b3532-1380-2429-a68a-ae2220032b67"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f98e85ae-f9ec-eee8-df71-4573659dc3b1"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f9bd03f5-25dd-064f-3b02-7df8c0830eaa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f9e4753b-b3c7-2f07-a70f-d755b4ed0508"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("f9f6f960-708a-13c8-a271-9ab01c9888ae"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fa075721-9b5a-c037-1b6b-40380c2676c4"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fa8fdebf-e324-aed5-b29f-c009f68fb88d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fb12025d-395a-170f-8720-d03c0f532237"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fb21399b-f6e2-85ec-5a1f-e8701f081060"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fb36ec2b-0631-1c1b-504f-e02e47dae823"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fb3b2da0-724f-dc41-98a8-69032d450a70"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fb5ba93b-d1e0-fe08-e672-d20d740ece7d"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fba7523b-8ba7-553b-4d12-b9035cbea5aa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fca7427e-3706-ca66-bf44-3c92d1114deb"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fccaf095-2e80-208b-0c2a-fa41eb9319c3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fceb8a7e-0a1b-18d3-d6a2-fc0492e6c35e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fcf1c325-fc0a-e1dd-4523-5ef6a59740bf"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fd0139ed-7943-761f-edc0-1f1ce53d0eff"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fd15439d-181f-3065-0612-e0f49dc2bbc3"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fd53ca50-786b-70ae-d7b4-b993a459b287"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fd9d31a7-f5f5-0601-eea4-8ca6c46c9f92"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fdd072ba-03a1-bf5f-1bc6-92eb9222cac2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fe12886e-400d-8077-8935-d2900c11f03f"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fe5de94e-4d8b-7359-bb89-dd0b1f515327"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fe9774f1-3b3e-4b24-b86b-07e1d50241c8"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("fecaf787-137f-91cb-3e19-97f7f2348c84"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ff078cc3-78ca-7494-ccfb-57ce13cf836c"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ff24d4da-a7a8-656a-b583-aa1f7706ffb2"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ff870416-197e-acd6-ada0-04a594c58cf5"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ffabd487-061f-6f12-dc22-f4ba76f2af9e"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ffb68448-39c0-053b-f7c6-023955693a11"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ffcf6aad-aec7-52b4-2b14-4d80ed417bfa"));

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: new Guid("ffde4f9e-e7bb-75c2-2eb5-56d7607cace9"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("04063756-480d-8e2d-df09-976b56560e13"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("0e554a2b-67e8-477d-3212-d6478a45ac9b"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("1b540ea1-83fc-d2c0-5074-21e65c4d23e5"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("21f66c97-4c97-25a8-e147-5094d9fcb32d"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("227a3cbc-6b21-e100-03a8-8467547eb0af"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("2a370370-ddce-50ce-8919-f3a5c607f9a9"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("2dbfb7ed-db08-e890-08b7-8625e61cc295"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("3549ba53-a6a9-38bc-b335-a42235bef026"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("3f300615-618c-1331-68f2-a012bd75df93"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("41707a24-ff8c-b58a-0f8d-43b4ad78148c"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("4c720c2f-3d69-c921-9048-60bcb27b4d8c"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("698e3564-efcd-c435-6b54-83a816222201"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("71e44150-274a-e4f0-ccfb-d7c84727006e"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("8185e52c-b524-b5c3-3c7c-6e376a661b2b"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("825bae8d-67fd-aa92-41b4-97a0892bed0a"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("8714dd60-dd25-3903-cfbf-febd1d014c9e"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("96f19e63-2ede-522e-f31d-8e2697d686e2"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("9a669d72-92ab-f3fd-dd8f-2a924a612e00"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("9c47147c-0a87-e74e-9b2e-7c6c07a10e11"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("9e7ba066-79a7-00b7-fa27-dd5e4198f101"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("a61c504e-e953-7685-69ed-67b73c0ff859"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("aa4b5513-1e09-933a-cc41-47065a3c6834"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("b310c6f9-0c43-a8d7-d695-ab81af3f2dd7"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("bb1f25d3-e7ba-e4b1-4020-84914e97d797"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("c02b255f-c917-6491-aaa3-2c017d6a43a6"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("c7a547e6-e5fa-e2f4-4d5e-ec444bb1253f"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("cdf95308-bcb5-5050-fd40-e9bd24e76603"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("de2fe6f5-2331-19a4-7f32-fa5701737270"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("efdd5e9f-4e18-873b-8260-00917e639f69"));

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: new Guid("fef1694e-858e-87ff-37a9-c87ce29b89f7"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("45fa93a2-4920-384c-3e68-ed9355735687"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("749874a3-dad5-8938-70fc-a581bf400444"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a4486c7b-56b9-3ada-d479-520388515e31"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a757a0f2-7faa-80ae-a403-9603134698df"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("d85db233-5de9-9d19-cd18-eab551693140"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("f0592215-05a0-6a49-3096-f59843037897"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"));

            migrationBuilder.DeleteData(
                table: "Provinces",
                keyColumn: "Id",
                keyValue: new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"));
        }
    }
}
