namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

// Candidate.md "Referans Bilgileri" - who a reference is, relative to the candidate.
internal static class ReferenceTypeSeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("MANAGER", "Yönetici / Amir"),
        ("COLLEAGUE", "İş Arkadaşı"),
        ("ACADEMIC", "Akademisyen / Öğretim Görevlisi"),
        ("CLIENT", "Müşteri"),
        ("OTHER", "Diğer"),
    ];
}
