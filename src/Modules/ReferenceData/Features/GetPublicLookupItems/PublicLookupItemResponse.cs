namespace GenclikMerkezi.Modules.ReferenceData.Features.GetPublicLookupItems;

// Public-safe shape (madde 39, Faz 3 Koşu 3C): an anonymous /public/jobs filter dropdown only ever
// needs id+name - never Code/IsActive/SortOrder, which the admin-facing LookupItemSummary carries.
public sealed record PublicLookupItemResponse(Guid Id, string Name);
