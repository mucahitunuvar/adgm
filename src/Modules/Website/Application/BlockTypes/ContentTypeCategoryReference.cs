namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// A block that filters content by type+category (content-list, faq) references the type by its Key,
// not its Id (the admin-facing settings shape matches ContentType.Key, not an internal Guid) - and
// optionally one category of that type. PageLayoutReferenceValidator resolves the key, then confirms
// the category (if any) actually belongs to that resolved type (§4.2 "content-list'teki kategori o
// türe ait olmalı").
public sealed record ContentTypeCategoryReference(string ContentTypeKey, Guid? CategoryId);
