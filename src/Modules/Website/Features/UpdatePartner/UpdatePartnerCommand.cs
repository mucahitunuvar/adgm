using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartner;

public sealed record UpdatePartnerCommand(
    Guid Id, byte[] RowVersion, Guid LogoMediaId, string? WebsiteUrl, int SortOrder) : IRequest<Result>;
