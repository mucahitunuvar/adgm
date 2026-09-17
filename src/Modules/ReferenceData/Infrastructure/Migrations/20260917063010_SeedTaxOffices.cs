using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTaxOffices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TaxOffices",
                columns: new[] { "Id", "Code", "DisplayName", "IsActive", "ProvinceId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("0140e921-5379-36d7-9062-c2a6cb70171e"), "57260", "Sinop Vergi Dairesi", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 112 },
                    { new Guid("01ef3587-b9ae-4277-b332-1209cd10ffa3"), "36103", "Arpaçay Vergi Dairesi", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 71 },
                    { new Guid("06b77b5b-e5c6-a5ef-1657-1ee48771dc6a"), "68201", "Aksaray Vergi Dairesi", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 134 },
                    { new Guid("06eb56cc-5e57-b688-8b9c-8a6e7d081b07"), "9201", "Efeler Vergi Dairesi", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 16 },
                    { new Guid("098c7a51-1c4b-fe2c-e888-a263a5d197ec"), "69201", "Bayburt Vergi Dairesi", true, new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), 136 },
                    { new Guid("0a330a3b-6e5f-b4e5-3b2a-d4e0a2288a64"), "17260", "Çanakkale Vergi Dairesi", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 32 },
                    { new Guid("0b78dc5e-928f-09ad-868c-501eb2fa40c7"), "81102", "Cumayeri Vergi Dairesi", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 160 },
                    { new Guid("0bc58d70-d3d7-3e62-254c-6ddf3becf342"), "59201", "Süleymanpaşa Vergi Dairesi", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 116 },
                    { new Guid("0bf21eae-9541-d62c-efc3-74d7cf689a67"), "30102", "Çukurca Vergi Dairesi", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 59 },
                    { new Guid("0d5f4721-a14c-03e1-57a4-50cc88efa513"), "1251", "5 Ocak Vergi Dairesi", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 1 },
                    { new Guid("0d66223d-66f8-12b3-b0c3-d73b1a734889"), "39102", "Demirköy Vergi Dairesi", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 77 },
                    { new Guid("0e0c999b-f860-b64d-2ab9-9667421d5eec"), "19260", "Çorum Vergi Dairesi", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 36 },
                    { new Guid("124259dd-855f-95a6-f176-668736571d03"), "66101", "Akdağmadeni Vergi Dairesi", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 131 },
                    { new Guid("1272c23d-5cd3-e067-898c-1656a21df9c8"), "58201", "Kale Vergi Dairesi", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 114 },
                    { new Guid("13417c41-78ed-9287-6b33-4e6e88952fe2"), "65260", "Van Vergi Dairesi", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 128 },
                    { new Guid("13c11f6f-cbc4-2a50-8f20-59acadef879d"), "70201", "Karaman Vergi Dairesi", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 138 },
                    { new Guid("14caf5d3-ce72-2ed8-d74f-040225568b72"), "5101", "Göynücek Vergi Dairesi", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 9 },
                    { new Guid("154276df-0950-81f7-524c-1bc93344bc38"), "7252", "Kalekapı Vergi Dairesi", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 13 },
                    { new Guid("158316ba-b816-939b-466c-7abdb81aa5a1"), "31203", "Antakya Vergi Dairesi", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 61 },
                    { new Guid("15f8f40f-58e0-56a2-d952-49095eb1d1ce"), "28260", "Giresun Vergi Dairesi", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 54 },
                    { new Guid("17a9a4a9-6390-7056-a093-4f569aa4bede"), "14104", "Göynük Vergi Dairesi", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 27 },
                    { new Guid("1b6a479d-dcaf-5970-49ae-30e8e855a8db"), "71202", "Kaletepe Vergi Dairesi", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 141 },
                    { new Guid("1c08e0cb-fd26-2fd4-d8b1-ca5f2c303e8d"), "40101", "Çiçekdağı Vergi Dairesi", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 79 },
                    { new Guid("1c7929a0-3ac7-1b58-aa31-2ad492a16150"), "14260", "Bolu Vergi Dairesi", true, new Guid("f639847a-3507-3f2c-38ef-e52ac33e2a3a"), 26 },
                    { new Guid("1d5ab769-e091-35db-cd3b-04ec71af65af"), "18260", "Çankırı Vergi Dairesi", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 34 },
                    { new Guid("1d8d9eba-20c6-e70e-f27e-941974c0a3ce"), "52260", "Köprübaşı Vergi Dairesi", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 103 },
                    { new Guid("20b7f698-6845-0e09-12d8-ff7eb0951bb9"), "35114", "Seferihisar Vergi Dairesi", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 69 },
                    { new Guid("2158c160-2a27-521e-42f6-c71766ceb4cc"), "3280", "Kocatepe Vergi Dairesi", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 5 },
                    { new Guid("226a9415-0522-8674-1706-38cc142a9cf4"), "47102", "Derik Vergi Dairesi", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 93 },
                    { new Guid("22a63b2f-f2f1-326a-ef48-6e91e9cbfecb"), "73101", "Beytüşşebap Vergi Dairesi", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 145 },
                    { new Guid("22ef6e49-7f3c-b074-9d48-d755b04af690"), "61201", "Hızırbey Vergi Dairesi", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 120 },
                    { new Guid("2544e443-68a4-4c67-fb04-bc9a5f339d0f"), "38251", "Mimar Sinan Vergi Dairesi", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 75 },
                    { new Guid("28fc67a8-19eb-e862-6c87-3a1990a76183"), "28101", "Alucra Vergi Dairesi", true, new Guid("76aade05-c08d-2b94-97f5-183048ad5ecc"), 55 },
                    { new Guid("2970e414-2ba2-36cc-b451-4af01dd74b11"), "32201", "Davraz Vergi Dairesi", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 62 },
                    { new Guid("2c7480d6-8c81-6cf0-87c2-6251e070cd0b"), "56260", "Siirt Vergi Dairesi", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 110 },
                    { new Guid("2f0cfd48-946d-d8f7-646d-fc7edd17df7f"), "52201", "Boztepe Vergi Dairesi", true, new Guid("df829f8c-df9c-1bb3-91ac-2fb6d021921d"), 102 },
                    { new Guid("30511515-255f-ecd6-fd03-358e8553df55"), "5260", "Amasya Vergi Dairesi", true, new Guid("d3a52e79-b58e-7e52-b273-77488df4330d"), 8 },
                    { new Guid("332d0a10-c233-4936-c94f-dbdccc0eeabc"), "6102", "Ayaş Vergi Dairesi", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 10 },
                    { new Guid("33b229c6-ae71-fc3b-bd77-7e1e65066943"), "53201", "Kaçkar Vergi Dairesi", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 104 },
                    { new Guid("34338677-66e7-78d4-0911-c39bee32ac2d"), "4101", "Diyadin Vergi Dairesi", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 7 },
                    { new Guid("350cca3c-94e0-036f-31d4-8fc9f8444647"), "68101", "Ağaçören Vergi Dairesi", true, new Guid("2cdfc295-4351-7b47-cece-3f05b2bd4397"), 135 },
                    { new Guid("35a1601d-73b4-9a8d-af51-b50c2b502ec9"), "54111", "Karapürçek Vergi Dairesi", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 106 },
                    { new Guid("376db0b6-71b0-f175-7bea-64eaae930489"), "35106", "Foça Vergi Dairesi", true, new Guid("45fa93a2-4920-384c-3e68-ed9355735687"), 68 },
                    { new Guid("39074a23-9c50-a7b0-03c7-b6ceb7b7bc78"), "62260", "Tunceli Vergi Dairesi", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 122 },
                    { new Guid("391fdf26-f060-1261-c03d-264b8943f27c"), "69101", "Aydıntepe Vergi Dairesi", true, new Guid("6fde442d-cc53-5a3c-3cf4-1ef6997f7d91"), 137 },
                    { new Guid("398b058b-0d6f-93f1-111c-4dc1a404a5f5"), "50260", "Nevşehir Vergi Dairesi", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 98 },
                    { new Guid("3adb2ddb-3eac-7a31-61ce-6d587182db3f"), "16201", "Gemlik Vergi Dairesi", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 30 },
                    { new Guid("3d02948a-dcdd-151b-b757-d75b992e76d8"), "7251", "Üçkapılar Vergi Dairesi", true, new Guid("301bd23a-d4a4-cb58-b966-7e6366af91fb"), 12 },
                    { new Guid("3f290780-87f7-f989-48ba-5b0cd9354e51"), "22201", "Arda Vergi Dairesi", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 42 },
                    { new Guid("3fd165dd-1c4d-33c9-fca0-aed4677eea7d"), "74260", "Bartın Vergi Dairesi", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 146 },
                    { new Guid("40c05435-d168-6d5b-9554-5ed18255d88f"), "8101", "Ardanuç Vergi Dairesi", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 15 },
                    { new Guid("428d4939-a916-24ea-f4db-23769875ea16"), "12260", "Bingöl Vergi Dairesi", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 22 },
                    { new Guid("44bc944a-053d-3cd5-c3cc-babc475d4120"), "26102", "Mahmudiye Vergi Dairesi", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 50 },
                    { new Guid("4728251b-e618-da0d-637d-b86759ce91de"), "78201", "Karabük Vergi Dairesi", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 154 },
                    { new Guid("4a8ebb39-ccab-6a13-6cbd-b124870f0ed9"), "27250", "Gaziantep İhtisas Vergi Dairesi", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 53 },
                    { new Guid("4b952284-7dde-9019-408a-2a211fbf680f"), "34203", "Silivri Vergi Dairesi", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 66 },
                    { new Guid("4e20912c-f8cc-01b5-c02e-2671428efb3f"), "26103", "Mihalıççık Vergi Dairesi", true, new Guid("80a6196c-053a-9c2e-795b-32113d2d0574"), 51 },
                    { new Guid("4fbe7b37-56ea-0881-9877-0f8418788dc0"), "27105", "Oğuzeli Vergi Dairesi", true, new Guid("f2468a1e-e48c-49f4-716d-3116e0795356"), 52 },
                    { new Guid("5298f47a-695c-7686-9b17-324f2bc8e653"), "64103", "Karahallı Vergi Dairesi", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 127 },
                    { new Guid("55c6bf2f-7c99-590f-311f-a393f2922998"), "24260", "Fevzipaşa Vergi Dairesi", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 46 },
                    { new Guid("56ce8b00-e562-526e-d636-44f09407600e"), "60260", "Tokat Vergi Dairesi", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 118 },
                    { new Guid("5c169d01-a734-5226-ee82-78d178859098"), "57101", "Ayancık Vergi Dairesi", true, new Guid("f0592215-05a0-6a49-3096-f59843037897"), 113 },
                    { new Guid("5cb44d0b-eefa-6ac8-9d12-fb416236d0c1"), "50101", "Avanos Vergi Dairesi", true, new Guid("7af9c8c0-a6ae-bc80-bebd-c45fe98ad453"), 99 },
                    { new Guid("5da1865e-dfe5-e05b-191e-aba311bdb72e"), "34204", "Büyükçekmece Vergi Dairesi", true, new Guid("f6b76508-7b41-d13a-db4d-c9925bd3e834"), 67 },
                    { new Guid("5e3b41b5-7f2f-9888-924c-198588fac542"), "11260", "Bilecik Vergi Dairesi", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 20 },
                    { new Guid("5f467c6f-38d3-5c98-d1d5-3eadba8b25ef"), "45251", "Alaybey Vergi Dairesi", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 89 },
                    { new Guid("64e23e20-d997-e746-46f8-1294cb7d87f4"), "24101", "Çayırlı Vergi Dairesi", true, new Guid("13e37645-a618-77d2-e7b5-d30883a074c1"), 47 },
                    { new Guid("670c3ab6-5abe-cb48-99ce-560ca864d46d"), "4260", "Ağrı Vergi Dairesi", true, new Guid("1dc29387-f42e-4ad5-0883-6379ce732fd4"), 6 },
                    { new Guid("683b4758-3d24-58c3-1441-5f2a65b0f437"), "2260", "Adıyaman Vergi Dairesi", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 2 },
                    { new Guid("69632445-f25f-b0c2-632b-069c8c03f182"), "66260", "Yozgat Vergi Dairesi", true, new Guid("ed5e6fcb-7a11-0190-8154-262a6090e469"), 130 },
                    { new Guid("6c566cd0-074f-4f91-17e9-b24d56476ac9"), "44252", "Beydağı Vergi Dairesi", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 87 },
                    { new Guid("71dc8d11-5b27-2659-dc34-6a98bc2bae05"), "22260", "Kırkpınar  Vergi Dairesi", true, new Guid("1e329a7e-54e1-b7a3-4a8a-5f166b1aaa5e"), 43 },
                    { new Guid("79023203-0a76-5c6c-9603-81aeaaa914dd"), "59260", "Namık Kemal Vergi Dairesi", true, new Guid("e94289ff-3de7-96b6-1965-6cd3607eb375"), 117 },
                    { new Guid("7a175a16-e44e-8132-3c23-344d225d319b"), "29102", "Kelkit Vergi Dairesi", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 57 },
                    { new Guid("7f44ab8a-20d9-d022-eaea-47715636bf3e"), "62101", "Çemişgezek Vergi Dairesi", true, new Guid("6b73d241-700e-9c3b-373b-d6fa107416db"), 123 },
                    { new Guid("8045a729-2dfb-a6b1-4eb9-f47d122a0ba4"), "53260", "Yeşilçay Vergi Dairesi", true, new Guid("1d0087f5-d1b2-5f16-6644-38215c223849"), 105 },
                    { new Guid("837b2f0d-9405-616c-a590-ed74c18d9a92"), "74101", "Amasra Vergi Dairesi", true, new Guid("d77f7ac3-b17d-b85b-5dab-4f86d0915023"), 147 },
                    { new Guid("85184acf-9794-3b78-1ce9-623715e487ef"), "70101", "Ayrancı Vergi Dairesi", true, new Guid("da329570-406e-99c5-04fd-92e4da2cf2fb"), 139 },
                    { new Guid("853613ec-8be8-fbd2-4330-611275e1a236"), "75201", "Ardahan Vergi Dairesi", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 148 },
                    { new Guid("86daf06a-5eb5-1018-0646-e0bb93070fd8"), "23201", "Harput Vergi Dairesi", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 44 },
                    { new Guid("8896f6f8-53bf-b669-08a9-5997c6c54158"), "65101", "Başkale Vergi Dairesi", true, new Guid("bd81aa0e-c5c0-7d6f-ee6a-eaeba3b2251b"), 129 },
                    { new Guid("89183437-f2fb-a103-3053-f1ee977f2280"), "46201", "Aslanbey Vergi Dairesi", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 90 },
                    { new Guid("8af8b6bc-6ce2-3b08-1f3a-c839163b7ad2"), "29260", "Gümüşhane Vergi Dairesi", true, new Guid("68290a90-f473-1b6f-dab3-f312dfc077ef"), 56 },
                    { new Guid("8c48fbac-972a-f6a4-75e7-8f2a5c05d7ca"), "48102", "Datça Vergi Dairesi", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 95 },
                    { new Guid("8ccc332f-46da-b381-0441-3665fbbbda90"), "77101", "Altınova Vergi Dairesi", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 153 },
                    { new Guid("910b93d4-5eb2-76db-5c93-a6f1298e78fe"), "15260", "Burdur Vergi Dairesi", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 28 },
                    { new Guid("96c5db17-7fb3-b08e-d3cb-cb2051bd59d2"), "41103", "Kandıra Vergi Dairesi", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 80 },
                    { new Guid("980de172-b20e-68a8-35f0-fbf42f8988e1"), "18101", "Çerkeş Vergi Dairesi", true, new Guid("d3e24b17-1702-1603-a49c-395ac9c60f3c"), 35 },
                    { new Guid("9853a20c-2425-5960-6397-b3495dbd305c"), "80101", "Bahçe Vergi Dairesi", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 158 },
                    { new Guid("999746af-38b9-18d5-bcbd-5fcef74b27bd"), "73260", "Şırnak Vergi Dairesi", true, new Guid("d856cb47-dd57-f377-e398-1fd8727a790d"), 144 },
                    { new Guid("9a42d156-430e-d19f-5afe-be1c9611a475"), "9280", "Güzelhisar Vergi Dairesi", true, new Guid("e4cdd9c8-b781-400f-ca76-9c149461ea30"), 17 },
                    { new Guid("9b7d6b7e-b72b-475d-0bf9-b342d9000663"), "72260", "Batman Vergi Dairesi", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 142 },
                    { new Guid("9d7432b6-2953-53ec-69be-a4bd3fdf22f9"), "76101", "Aralık Vergi Dairesi", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 151 },
                    { new Guid("a0aa4ee6-68d8-3a8d-6341-fa57ba4d7705"), "56102", "Baykan Vergi Dairesi", true, new Guid("60b91b91-ff3d-05ef-d69b-b71915e1ff77"), 111 },
                    { new Guid("a0f6b8e3-f554-8b8f-9c0d-4a063ff1dcc2"), "25117", "Aziziye (Ilıca) Vergi Dairesi", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 48 },
                    { new Guid("a1e48ce5-015f-80ad-a030-7e83e3866a6c"), "61280", "Karadeniz Vergi Dairesi", true, new Guid("9a8831e8-b41b-b40f-55cc-48492270eef0"), 121 },
                    { new Guid("a1ee3878-3181-5aaa-81ec-57d9d1300056"), "2101", "Besni Vergi Dairesi", true, new Guid("d85db233-5de9-9d19-cd18-eab551693140"), 3 },
                    { new Guid("a33d5ed5-0973-23eb-c533-330999f3f3a8"), "39260", "Kırklareli Vergi Dairesi", true, new Guid("c5c09a9e-49a0-6590-4d9b-73045d507e1d"), 76 },
                    { new Guid("a6a68e82-1d55-f2e2-6c2a-6846dd54ff23"), "67280", "Kara Elmas Vergi Dairesi", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 133 },
                    { new Guid("a7fe1b0a-3eab-b5e6-2cfc-1e6d8da80fa2"), "37260", "Kastamonu  Vergi Dairesi", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 72 },
                    { new Guid("a8041935-0962-9c8d-5fd5-08646b6ce418"), "40260", "Kırşehir Vergi Dairesi", true, new Guid("8e1aed9c-9f4f-f29f-f3d5-d03458e0e718"), 78 },
                    { new Guid("ae304633-b74f-7666-f00c-6af5bcd288a3"), "20202", "Çınar Vergi Dairesi", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 39 },
                    { new Guid("aeaa7fcc-a44f-097e-fbfc-1fa62131a3e7"), "41207", "Derince  Vergi Dairesi", true, new Guid("bb4189cc-e60a-6f9b-344c-0b4cafae2b12"), 81 },
                    { new Guid("aeb5bc70-3c96-8c1b-fa4b-428361c991af"), "17101", "Ayvacık Vergi Dairesi", true, new Guid("782dbc6e-fd3b-7bbe-4116-5d9c2aa956b9"), 33 },
                    { new Guid("aee5d2e4-4a5b-ad93-8bb2-3670e267f84f"), "55251", "19 Mayıs Vergi Dairesi", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 109 },
                    { new Guid("afbbb850-3583-c246-4c37-548054d263aa"), "48260", "Muğla Vergi Dairesi", true, new Guid("6a1a760f-6405-556a-dd11-0dc65310523c"), 94 },
                    { new Guid("b1730517-c6f2-f6f8-d501-7bc454304585"), "13101", "Adilcevaz Vergi Dairesi", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 25 },
                    { new Guid("b85ecbbf-7d9a-e178-e4be-ed96f302418f"), "10201", "Karesi Vergi Dairesi", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 18 },
                    { new Guid("b8a618d2-3132-04b9-e132-33fa3aa50945"), "80201", "Osmaniye Vergi Dairesi", true, new Guid("68b06a57-7a16-bb31-2dfb-c97cf470c1e6"), 157 },
                    { new Guid("b94c02d1-c1dc-a198-2e7f-8610f68d5fc2"), "16205", "Mudanya Vergi Dairesi", true, new Guid("a4486c7b-56b9-3ada-d479-520388515e31"), 31 },
                    { new Guid("bdb2c78f-35bf-d17f-f32c-4287f63004d1"), "36260", "Kars Vergi Dairesi", true, new Guid("09c48e7c-bb1e-829a-a8d3-2777205e8a49"), 70 },
                    { new Guid("c3c94ae0-9b23-ff16-6308-c584b4820e6a"), "47260", "Mardin Vergi Dairesi", true, new Guid("925e4e30-b0e3-fb24-4578-9a8d935d3ec0"), 92 },
                    { new Guid("c3d0892f-5509-dfc8-bfbb-933ee2b83eaa"), "81260", "Düzce Vergi Dairesi", true, new Guid("a757a0f2-7faa-80ae-a403-9603134698df"), 159 },
                    { new Guid("c460333d-a38c-7d5c-92e9-940e790c06e4"), "46280", "Aksu Vergi Dairesi", true, new Guid("5de506b1-4022-0105-96e5-09d956f9b20f"), 91 },
                    { new Guid("c736badf-55b6-1a4e-28f5-7d9b4a3d423a"), "21281", "Süleyman Nazif Vergi Dairesi", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 41 },
                    { new Guid("ca33e9fc-042f-88ac-405f-595f78d52a79"), "67201", "Uzunmehmet Vergi Dairesi", true, new Guid("8b6a3869-96f5-49bc-a182-7f77fec8fe8b"), 132 },
                    { new Guid("ca373738-eb18-837f-4460-e2c2e66fb5f1"), "10280", "Kurtdereli Vergi Dairesi", true, new Guid("663ff4b9-e4f1-1d8f-14ac-5aa13f2d2bc7"), 19 },
                    { new Guid("cb7304de-d762-1920-cc8c-1ec677a32636"), "42250", "Konya İhtisas Vergi Dairesi", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 82 },
                    { new Guid("cbb4cb22-9627-fb55-4a4c-43bf9a9a1477"), "20201", "Saraylar Vergi Dairesi", true, new Guid("26b1353c-927c-58e3-a3de-1cca25e9b68e"), 38 },
                    { new Guid("cc917691-67a4-51f9-892a-8b91787eb36f"), "31201", "23 Temmuz Vergi Dairesi", true, new Guid("a77cba1d-bc5f-105f-a5e4-c82a8e4af47f"), 60 },
                    { new Guid("cd1e61b3-1293-4b7c-74c1-508f133690d4"), "49101", "Bulanık Vergi Dairesi", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 97 },
                    { new Guid("ce0143be-950b-6d34-43e1-2c6f94a2959c"), "79201", "Kilis Vergi Dairesi", true, new Guid("f299296c-c3a5-6a1e-0a47-a0ab8bbce6dd"), 156 },
                    { new Guid("cf4b68af-f9e8-409f-8a95-346e01a1e022"), "25251", "Aziziye Vergi Dairesi", true, new Guid("9ce17875-19ca-daf7-99c0-08c5618f08c5"), 49 },
                    { new Guid("d13cbdaf-e352-7c1f-1b65-2204eb395367"), "42251", "Selçuk Vergi Dairesi", true, new Guid("22dca34c-1ea0-1e85-d6b2-04f927e9973b"), 83 },
                    { new Guid("d15855c1-6f3e-56aa-b5e2-91cc029de039"), "64260", "Uşak Vergi Dairesi", true, new Guid("026ba89e-32b5-e835-0858-1225eb32aec6"), 126 },
                    { new Guid("d2f2992e-c2dc-fbb5-005f-00e231e6adf3"), "23280", "Hazar Vergi Dairesi", true, new Guid("c74b0f7e-c105-7f9d-ff6b-ce219ba8af17"), 45 },
                    { new Guid("d38f2944-d502-78b9-35c5-3c60bd47e6ec"), "51103", "Çamardı Vergi Dairesi", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 101 },
                    { new Guid("d4175586-d583-24b1-1dd6-094d8213a7e0"), "21251", "Gökalp Vergi Dairesi", true, new Guid("6ec62645-1cc4-fe3f-c8b3-ae1df4e4aaf8"), 40 },
                    { new Guid("d4288623-30b5-eb09-7847-bf76493f7960"), "37101", "Araç Vergi Dairesi", true, new Guid("d28723c4-5490-4197-8ab8-b4b884b79ea4"), 73 },
                    { new Guid("d4990d44-e9dd-3759-85b7-5c0b6c683171"), "19101", "Alaca Vergi Dairesi", true, new Guid("8b3c2137-7680-02ef-d463-94d8d88e93a4"), 37 },
                    { new Guid("d850a179-b7fb-e507-3ff8-438a92bfc708"), "51260", "Niğde Vergi Dairesi", true, new Guid("5bb74744-bcdd-e8c8-bda4-4b7de3ec3152"), 100 },
                    { new Guid("d89bd7fc-7072-a8d0-ff5d-f1574045055b"), "49260", "Muş Vergi Dairesi", true, new Guid("a7115579-8270-3dee-d985-e7dc2876a23f"), 96 },
                    { new Guid("dcdc9092-14a3-c646-b0fc-910aa03e0660"), "55112", "Tekkeköy Vergi Dairesi", true, new Guid("a28a38a9-82c3-b139-cf90-5238d2b2ecaf"), 108 },
                    { new Guid("dda9a4a4-b477-0ec8-928d-b86a4a192e43"), "33252", "Uray Vergi Dairesi", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 65 },
                    { new Guid("ddcc2fad-b139-03b0-135a-69533709ccbc"), "54201", "Akyazı Vergi Dairesi", true, new Guid("cc22bbdf-5adb-d4b1-596b-597b8555cfdd"), 107 },
                    { new Guid("dddb9484-1011-309d-19cf-5c34f6a57c84"), "1250", "Adana İhtisas Vergi Dairesi", true, new Guid("2cca9b5f-fea6-2ae9-900e-bff61710f709"), 0 },
                    { new Guid("ded5a57b-77e1-ae53-8719-e8f285783561"), "32260", "Kaymakkapı Vergi Dairesi", true, new Guid("6fb9a3f7-0c23-a2bb-1870-209be6fb0469"), 63 },
                    { new Guid("dee675d4-aaa3-6c37-dc4d-ff4c4279f5b0"), "43280", "Çinili Vergi Dairesi", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 85 },
                    { new Guid("e0af4d50-33eb-c20e-ca86-04aa27cd7622"), "76201", "Iğdır  Vergi Dairesi", true, new Guid("873abad8-cdbc-2bc9-ecfd-9672f0d1b26f"), 150 },
                    { new Guid("e3a33fc6-668e-a268-96f1-1a2aec867fa3"), "6103", "Balâ Vergi Dairesi", true, new Guid("d7faa99c-d82e-f379-f8a1-88e4d3461fd2"), 11 },
                    { new Guid("e4d1a200-098c-e028-5de9-53091a46786d"), "38250", "Kayseri İhtisas Vergi Dairesi", true, new Guid("d4db9c9b-57ae-958e-daaf-453ee24782ab"), 74 },
                    { new Guid("e6034db5-8c78-977e-dbfb-553d56611d22"), "71201", "Irmak Vergi Dairesi", true, new Guid("a64f3b1b-3557-84fd-62dc-9527e240e8d0"), 140 },
                    { new Guid("e62feb22-0852-f264-886c-c3b19240b194"), "60101", "Almus Vergi Dairesi", true, new Guid("612118d9-30a4-3cd6-1fd2-f5ceaed26974"), 119 },
                    { new Guid("e6404671-cab0-57ed-40ac-d9f15a9c19be"), "13260", "Bitlis Vergi Dairesi", true, new Guid("6682ae41-5005-2eb1-c3e5-2827630b699b"), 24 },
                    { new Guid("e7948133-1275-7c78-f776-b9a71d153e1e"), "12101", "Genç Vergi Dairesi", true, new Guid("2215e394-669b-3cb6-f3ce-5e4aae0555de"), 23 },
                    { new Guid("e98c0fbe-c78b-1781-d0f3-51d45b467449"), "75101", "Çıldır Vergi Dairesi", true, new Guid("ae3389d2-1d3f-87c6-6272-f24f16eff891"), 149 },
                    { new Guid("eb24377e-26f1-0788-c86f-7ac7789a732d"), "72101", "Beşiri Vergi Dairesi", true, new Guid("928443b4-74ae-42dd-3dec-f7f5bf059c86"), 143 },
                    { new Guid("eb3e6877-3a02-73f7-49ed-5973bbbddf3d"), "78101", "Eflani Vergi Dairesi", true, new Guid("9772086b-055e-3b3d-d28f-c64c4db8b30e"), 155 },
                    { new Guid("ed0a3d29-370e-6a65-a61b-7d8e9b1f5d6e"), "44251", "Fırat Vergi Dairesi", true, new Guid("85dae147-f6e8-9f26-0dc5-27b291bb7dac"), 86 },
                    { new Guid("ed966a8d-1d52-34bd-ea4c-f495ec93d4f4"), "63280", "Topçu Meydanı Vergi Dairesi", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 125 },
                    { new Guid("f069370b-81f2-e6fd-4464-2bcff11b473b"), "77201", "Yalova Vergi Dairesi", true, new Guid("2fc0ac82-5682-db79-00c9-49553e18fb73"), 152 },
                    { new Guid("f085f635-1559-373b-17bf-334c4a234c96"), "8260", "Artvin  Vergi Dairesi", true, new Guid("ca1c1fbb-dfad-f3b1-bdfd-c4ccce2cc66c"), 14 },
                    { new Guid("f0a48d97-a95d-2835-1400-3fed6283e42b"), "43201", "30 Ağustos Vergi Dairesi", true, new Guid("5d387883-faac-1ea2-2505-a3b8e11c0c31"), 84 },
                    { new Guid("f17913e6-ac03-3f79-51dc-57616b9c344d"), "3201", "Tınaztepe Vergi Dairesi", true, new Guid("749874a3-dad5-8938-70fc-a581bf400444"), 4 },
                    { new Guid("f2c496c8-5bcb-b37c-4cd3-b87b297b1005"), "15101", "Ağlasun Vergi Dairesi", true, new Guid("dd586a3f-f86b-fac7-fd6c-a36a897d3972"), 29 },
                    { new Guid("f4186e98-ea1e-7db8-6efa-2794ac3a90e6"), "30260", "Hakkari Vergi Dairesi", true, new Guid("d0ace97d-df20-1ee6-b974-6f01a4aa2e37"), 58 },
                    { new Guid("fa955384-362f-8278-77f7-364bf864d65a"), "33250", "İstiklâl Vergi Dairesi", true, new Guid("7acd3f30-0de9-ec7e-0ab5-16fb7af4f9aa"), 64 },
                    { new Guid("faa9e32f-469c-4a02-6302-3167fcbdfa58"), "58280", "Site Vergi Dairesi", true, new Guid("b9bab434-90ba-3a53-17f5-d57c9bdc59bc"), 115 },
                    { new Guid("fac14879-3116-327b-1d8a-76535b640c80"), "11102", "Gölpazarı Vergi Dairesi", true, new Guid("79ab111a-fcef-21f8-5d3e-e76f58db0a31"), 21 },
                    { new Guid("fcef52e9-3054-660d-4494-d235373f9e25"), "45250", "Manisa İhtisas Vergi Dairesi", true, new Guid("bf6efe89-6f39-4b33-236f-83710c5b88f6"), 88 },
                    { new Guid("fd910fdb-4bbd-b7a6-2b03-51618a38f287"), "63201", "Şehitlik Vergi Dairesi", true, new Guid("a42af5f6-6653-ec58-e776-7f8c8a9c524e"), 124 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0140e921-5379-36d7-9062-c2a6cb70171e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("01ef3587-b9ae-4277-b332-1209cd10ffa3"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("06b77b5b-e5c6-a5ef-1657-1ee48771dc6a"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("06eb56cc-5e57-b688-8b9c-8a6e7d081b07"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("098c7a51-1c4b-fe2c-e888-a263a5d197ec"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0a330a3b-6e5f-b4e5-3b2a-d4e0a2288a64"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0b78dc5e-928f-09ad-868c-501eb2fa40c7"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0bc58d70-d3d7-3e62-254c-6ddf3becf342"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0bf21eae-9541-d62c-efc3-74d7cf689a67"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0d5f4721-a14c-03e1-57a4-50cc88efa513"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0d66223d-66f8-12b3-b0c3-d73b1a734889"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("0e0c999b-f860-b64d-2ab9-9667421d5eec"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("124259dd-855f-95a6-f176-668736571d03"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1272c23d-5cd3-e067-898c-1656a21df9c8"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("13417c41-78ed-9287-6b33-4e6e88952fe2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("13c11f6f-cbc4-2a50-8f20-59acadef879d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("14caf5d3-ce72-2ed8-d74f-040225568b72"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("154276df-0950-81f7-524c-1bc93344bc38"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("158316ba-b816-939b-466c-7abdb81aa5a1"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("15f8f40f-58e0-56a2-d952-49095eb1d1ce"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("17a9a4a9-6390-7056-a093-4f569aa4bede"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1b6a479d-dcaf-5970-49ae-30e8e855a8db"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1c08e0cb-fd26-2fd4-d8b1-ca5f2c303e8d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1c7929a0-3ac7-1b58-aa31-2ad492a16150"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1d5ab769-e091-35db-cd3b-04ec71af65af"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("1d8d9eba-20c6-e70e-f27e-941974c0a3ce"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("20b7f698-6845-0e09-12d8-ff7eb0951bb9"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("2158c160-2a27-521e-42f6-c71766ceb4cc"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("226a9415-0522-8674-1706-38cc142a9cf4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("22a63b2f-f2f1-326a-ef48-6e91e9cbfecb"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("22ef6e49-7f3c-b074-9d48-d755b04af690"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("2544e443-68a4-4c67-fb04-bc9a5f339d0f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("28fc67a8-19eb-e862-6c87-3a1990a76183"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("2970e414-2ba2-36cc-b451-4af01dd74b11"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("2c7480d6-8c81-6cf0-87c2-6251e070cd0b"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("2f0cfd48-946d-d8f7-646d-fc7edd17df7f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("30511515-255f-ecd6-fd03-358e8553df55"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("332d0a10-c233-4936-c94f-dbdccc0eeabc"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("33b229c6-ae71-fc3b-bd77-7e1e65066943"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("34338677-66e7-78d4-0911-c39bee32ac2d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("350cca3c-94e0-036f-31d4-8fc9f8444647"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("35a1601d-73b4-9a8d-af51-b50c2b502ec9"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("376db0b6-71b0-f175-7bea-64eaae930489"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("39074a23-9c50-a7b0-03c7-b6ceb7b7bc78"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("391fdf26-f060-1261-c03d-264b8943f27c"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("398b058b-0d6f-93f1-111c-4dc1a404a5f5"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("3adb2ddb-3eac-7a31-61ce-6d587182db3f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("3d02948a-dcdd-151b-b757-d75b992e76d8"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("3f290780-87f7-f989-48ba-5b0cd9354e51"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("3fd165dd-1c4d-33c9-fca0-aed4677eea7d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("40c05435-d168-6d5b-9554-5ed18255d88f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("428d4939-a916-24ea-f4db-23769875ea16"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("44bc944a-053d-3cd5-c3cc-babc475d4120"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("4728251b-e618-da0d-637d-b86759ce91de"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("4a8ebb39-ccab-6a13-6cbd-b124870f0ed9"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("4b952284-7dde-9019-408a-2a211fbf680f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("4e20912c-f8cc-01b5-c02e-2671428efb3f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("4fbe7b37-56ea-0881-9877-0f8418788dc0"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5298f47a-695c-7686-9b17-324f2bc8e653"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("55c6bf2f-7c99-590f-311f-a393f2922998"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("56ce8b00-e562-526e-d636-44f09407600e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5c169d01-a734-5226-ee82-78d178859098"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5cb44d0b-eefa-6ac8-9d12-fb416236d0c1"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5da1865e-dfe5-e05b-191e-aba311bdb72e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5e3b41b5-7f2f-9888-924c-198588fac542"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("5f467c6f-38d3-5c98-d1d5-3eadba8b25ef"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("64e23e20-d997-e746-46f8-1294cb7d87f4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("670c3ab6-5abe-cb48-99ce-560ca864d46d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("683b4758-3d24-58c3-1441-5f2a65b0f437"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("69632445-f25f-b0c2-632b-069c8c03f182"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("6c566cd0-074f-4f91-17e9-b24d56476ac9"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("71dc8d11-5b27-2659-dc34-6a98bc2bae05"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("79023203-0a76-5c6c-9603-81aeaaa914dd"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("7a175a16-e44e-8132-3c23-344d225d319b"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("7f44ab8a-20d9-d022-eaea-47715636bf3e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("8045a729-2dfb-a6b1-4eb9-f47d122a0ba4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("837b2f0d-9405-616c-a590-ed74c18d9a92"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("85184acf-9794-3b78-1ce9-623715e487ef"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("853613ec-8be8-fbd2-4330-611275e1a236"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("86daf06a-5eb5-1018-0646-e0bb93070fd8"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("8896f6f8-53bf-b669-08a9-5997c6c54158"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("89183437-f2fb-a103-3053-f1ee977f2280"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("8af8b6bc-6ce2-3b08-1f3a-c839163b7ad2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("8c48fbac-972a-f6a4-75e7-8f2a5c05d7ca"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("8ccc332f-46da-b381-0441-3665fbbbda90"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("910b93d4-5eb2-76db-5c93-a6f1298e78fe"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("96c5db17-7fb3-b08e-d3cb-cb2051bd59d2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("980de172-b20e-68a8-35f0-fbf42f8988e1"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("9853a20c-2425-5960-6397-b3495dbd305c"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("999746af-38b9-18d5-bcbd-5fcef74b27bd"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("9a42d156-430e-d19f-5afe-be1c9611a475"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("9b7d6b7e-b72b-475d-0bf9-b342d9000663"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("9d7432b6-2953-53ec-69be-a4bd3fdf22f9"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a0aa4ee6-68d8-3a8d-6341-fa57ba4d7705"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a0f6b8e3-f554-8b8f-9c0d-4a063ff1dcc2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a1e48ce5-015f-80ad-a030-7e83e3866a6c"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a1ee3878-3181-5aaa-81ec-57d9d1300056"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a33d5ed5-0973-23eb-c533-330999f3f3a8"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a6a68e82-1d55-f2e2-6c2a-6846dd54ff23"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a7fe1b0a-3eab-b5e6-2cfc-1e6d8da80fa2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("a8041935-0962-9c8d-5fd5-08646b6ce418"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ae304633-b74f-7666-f00c-6af5bcd288a3"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("aeaa7fcc-a44f-097e-fbfc-1fa62131a3e7"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("aeb5bc70-3c96-8c1b-fa4b-428361c991af"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("aee5d2e4-4a5b-ad93-8bb2-3670e267f84f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("afbbb850-3583-c246-4c37-548054d263aa"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("b1730517-c6f2-f6f8-d501-7bc454304585"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("b85ecbbf-7d9a-e178-e4be-ed96f302418f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("b8a618d2-3132-04b9-e132-33fa3aa50945"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("b94c02d1-c1dc-a198-2e7f-8610f68d5fc2"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("bdb2c78f-35bf-d17f-f32c-4287f63004d1"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("c3c94ae0-9b23-ff16-6308-c584b4820e6a"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("c3d0892f-5509-dfc8-bfbb-933ee2b83eaa"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("c460333d-a38c-7d5c-92e9-940e790c06e4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("c736badf-55b6-1a4e-28f5-7d9b4a3d423a"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ca33e9fc-042f-88ac-405f-595f78d52a79"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ca373738-eb18-837f-4460-e2c2e66fb5f1"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("cb7304de-d762-1920-cc8c-1ec677a32636"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("cbb4cb22-9627-fb55-4a4c-43bf9a9a1477"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("cc917691-67a4-51f9-892a-8b91787eb36f"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("cd1e61b3-1293-4b7c-74c1-508f133690d4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ce0143be-950b-6d34-43e1-2c6f94a2959c"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("cf4b68af-f9e8-409f-8a95-346e01a1e022"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d13cbdaf-e352-7c1f-1b65-2204eb395367"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d15855c1-6f3e-56aa-b5e2-91cc029de039"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d2f2992e-c2dc-fbb5-005f-00e231e6adf3"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d38f2944-d502-78b9-35c5-3c60bd47e6ec"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d4175586-d583-24b1-1dd6-094d8213a7e0"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d4288623-30b5-eb09-7847-bf76493f7960"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d4990d44-e9dd-3759-85b7-5c0b6c683171"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d850a179-b7fb-e507-3ff8-438a92bfc708"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("d89bd7fc-7072-a8d0-ff5d-f1574045055b"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("dcdc9092-14a3-c646-b0fc-910aa03e0660"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("dda9a4a4-b477-0ec8-928d-b86a4a192e43"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ddcc2fad-b139-03b0-135a-69533709ccbc"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("dddb9484-1011-309d-19cf-5c34f6a57c84"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ded5a57b-77e1-ae53-8719-e8f285783561"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("dee675d4-aaa3-6c37-dc4d-ff4c4279f5b0"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e0af4d50-33eb-c20e-ca86-04aa27cd7622"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e3a33fc6-668e-a268-96f1-1a2aec867fa3"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e4d1a200-098c-e028-5de9-53091a46786d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e6034db5-8c78-977e-dbfb-553d56611d22"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e62feb22-0852-f264-886c-c3b19240b194"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e6404671-cab0-57ed-40ac-d9f15a9c19be"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e7948133-1275-7c78-f776-b9a71d153e1e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("e98c0fbe-c78b-1781-d0f3-51d45b467449"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("eb24377e-26f1-0788-c86f-7ac7789a732d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("eb3e6877-3a02-73f7-49ed-5973bbbddf3d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ed0a3d29-370e-6a65-a61b-7d8e9b1f5d6e"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("ed966a8d-1d52-34bd-ea4c-f495ec93d4f4"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f069370b-81f2-e6fd-4464-2bcff11b473b"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f085f635-1559-373b-17bf-334c4a234c96"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f0a48d97-a95d-2835-1400-3fed6283e42b"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f17913e6-ac03-3f79-51dc-57616b9c344d"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f2c496c8-5bcb-b37c-4cd3-b87b297b1005"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("f4186e98-ea1e-7db8-6efa-2794ac3a90e6"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("fa955384-362f-8278-77f7-364bf864d65a"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("faa9e32f-469c-4a02-6302-3167fcbdfa58"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("fac14879-3116-327b-1d8a-76535b640c80"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("fcef52e9-3054-660d-4494-d235373f9e25"));

            migrationBuilder.DeleteData(
                table: "TaxOffices",
                keyColumn: "Id",
                keyValue: new Guid("fd910fdb-4bbd-b7a6-2b03-51618a38f287"));
        }
    }
}
