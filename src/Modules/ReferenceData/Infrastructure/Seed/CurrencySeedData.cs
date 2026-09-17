namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

// Code is the ISO 4217 code.
internal static class CurrencySeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("TRY", "Türk Lirası"),
        ("USD", "Amerikan Doları"),
        ("EUR", "Euro"),
    ];
}
