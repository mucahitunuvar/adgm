using System.Text.Json;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.ContentRevisions;

// ADR-024 §4 (Faz 5 Görev 7): the only place that turns a live ContentItem into the captured snapshot
// shape and back into JSON. Collections are always sorted before serializing (languages
// alphabetically, TagIds/CategoryIds ascending) so that two calls against logically identical state
// produce byte-identical JSON - ContentRevisionRecorder's hash-based "no-op save" dedup depends on
// this determinism.
public static class ContentItemRevisionSnapshotSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public static ContentItemRevisionSnapshot BuildFromContentItem(ContentItem contentItem)
    {
        var translations = contentItem.Translations
            .OrderBy(t => t.LanguageCode.Value, StringComparer.Ordinal)
            .Select(t => new ContentItemRevisionSnapshotTranslation(
                t.LanguageCode.Value, t.Title, t.Summary, t.Body, t.Seo.MetaTitle, t.Seo.MetaDescription, t.Seo.OgTitle,
                t.Seo.OgDescription, t.Seo.NoIndex, t.Seo.CanonicalUrl, t.TagIds.OrderBy(id => id).ToList()))
            .ToList();

        var categoryIds = contentItem.CategoryIds.OrderBy(id => id).ToList();

        return new ContentItemRevisionSnapshot(translations, categoryIds);
    }

    public static string Serialize(ContentItemRevisionSnapshot snapshot) => JsonSerializer.Serialize(snapshot, SerializerOptions);

    public static ContentItemRevisionSnapshot Deserialize(string snapshotJson) =>
        JsonSerializer.Deserialize<ContentItemRevisionSnapshot>(snapshotJson)
        ?? new ContentItemRevisionSnapshot([], []);
}
