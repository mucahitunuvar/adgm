namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1 "GET /api/v1/admin/website/block-types ... alan açıklamaları (ad, tip, zorunlu mu, dile bağlı
// mı, sınırlar)". Fields is populated only for an object/array-of-object field, describing its own
// nested shape (e.g. quick-links' items[]) the same way this type describes a block's top-level
// Settings/Texts record.
public sealed record BlockTypeFieldDescriptor(
    string Name,
    string Type,
    bool IsRequired,
    bool IsLanguageDependent,
    int? MinLength,
    int? MaxLength,
    double? Min,
    double? Max,
    IReadOnlyList<BlockTypeFieldDescriptor>? Fields);
