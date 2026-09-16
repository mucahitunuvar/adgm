namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

// Türkiye + en sık kullanılan ülkeler. Code is the ISO 3166-1 alpha-2 code.
internal static class CountrySeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("TR", "Türkiye"),
        ("DE", "Almanya"),
        ("US", "Amerika Birleşik Devletleri"),
        ("GB", "Birleşik Krallık"),
        ("FR", "Fransa"),
        ("NL", "Hollanda"),
        ("BE", "Belçika"),
        ("AT", "Avusturya"),
        ("CH", "İsviçre"),
        ("IT", "İtalya"),
        ("ES", "İspanya"),
        ("RU", "Rusya"),
        ("AZ", "Azerbaycan"),
        ("GE", "Gürcistan"),
        ("IR", "İran"),
        ("IQ", "Irak"),
        ("SY", "Suriye"),
        ("SA", "Suudi Arabistan"),
        ("AE", "Birleşik Arap Emirlikleri"),
        ("QA", "Katar"),
        ("KW", "Kuveyt"),
        ("EG", "Mısır"),
        ("UA", "Ukrayna"),
        ("BG", "Bulgaristan"),
        ("GR", "Yunanistan"),
        ("RO", "Romanya"),
        ("PL", "Polonya"),
        ("SE", "İsveç"),
        ("NO", "Norveç"),
        ("DK", "Danimarka"),
        ("CA", "Kanada"),
        ("AU", "Avustralya"),
        ("JP", "Japonya"),
        ("CN", "Çin"),
        ("KR", "Güney Kore"),
    ];
}
