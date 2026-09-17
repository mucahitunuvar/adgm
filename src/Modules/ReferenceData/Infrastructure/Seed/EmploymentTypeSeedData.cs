namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

internal static class EmploymentTypeSeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("FULL_TIME", "Tam Zamanlı"),
        ("PART_TIME", "Yarı Zamanlı"),
    ];
}
