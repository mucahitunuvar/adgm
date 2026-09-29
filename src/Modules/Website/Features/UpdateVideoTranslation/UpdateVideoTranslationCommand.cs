using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;

public sealed record UpdateVideoTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Title, string? Description) : IRequest<Result>;
