namespace GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;

public sealed record SetContentItemTranslationTagsRequest(byte[] RowVersion, IReadOnlyList<string> TagNames);
