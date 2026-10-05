using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;

public sealed record UpdatePopupTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Title, string? Body, string? ButtonLabel) : IRequest<Result>;
