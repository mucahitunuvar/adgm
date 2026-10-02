using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreatePartner;

public sealed record CreatePartnerCommand(
    Guid LogoMediaId,
    string? WebsiteUrl,
    int SortOrder,
    string? DefaultLanguageName,
    string? DefaultLanguageDescription) : IRequest<Result<CreatePartnerResponse>>;
