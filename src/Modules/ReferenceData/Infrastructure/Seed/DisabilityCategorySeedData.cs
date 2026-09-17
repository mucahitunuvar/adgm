namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;

// Standard Turkish disability categorization used in official forms.
internal static class DisabilityCategorySeedData
{
    public static readonly (string Code, string Name)[] All =
    [
        ("VISUAL", "Görme Engeli"),
        ("HEARING", "İşitme Engeli"),
        ("ORTHOPEDIC", "Ortopedik Engel"),
        ("INTELLECTUAL", "Zihinsel Engel"),
        ("MENTAL_EMOTIONAL", "Ruhsal ve Duygusal Engel"),
        ("CHRONIC_ILLNESS", "Süreğen Hastalık"),
        ("SPEECH_LANGUAGE", "Dil ve Konuşma Güçlüğü"),
    ];
}
