using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;

public sealed record RestoreContentItemRevisionCommand(
    Guid ContentItemId, int RevisionNumber, byte[]? RowVersion, string? LanguageCode, bool RestoreCategories) : IRequest<Result>;
