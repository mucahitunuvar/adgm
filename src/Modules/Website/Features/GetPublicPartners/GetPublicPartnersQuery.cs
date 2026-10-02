using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicPartners;

public sealed record GetPublicPartnersQuery(string? Lang) : IRequest<Result<IReadOnlyList<PublicPartnerResponse>>>;
