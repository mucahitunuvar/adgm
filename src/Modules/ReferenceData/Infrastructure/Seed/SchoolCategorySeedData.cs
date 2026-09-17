namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

internal static class SchoolCategorySeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("PRIMARY", "İlkokul"),
        ("MIDDLE", "Ortaokul"),
        ("HIGH_SCHOOL", "Lise"),
        ("UNIVERSITY", "Üniversite"),
    ];
}
