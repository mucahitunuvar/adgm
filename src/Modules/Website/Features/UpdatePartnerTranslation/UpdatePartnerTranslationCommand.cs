using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;

public sealed record UpdatePartnerTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Name, string? Description) : IRequest<Result>;
