namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

internal static class WorkLocationTypeSeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("REMOTE", "Uzaktan"),
        ("HYBRID", "Hibrit"),
        ("OFFICE", "Ofis"),
    ];
}
