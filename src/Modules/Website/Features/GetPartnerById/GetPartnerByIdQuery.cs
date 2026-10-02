using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPartnerById;

public sealed record GetPartnerByIdQuery(Guid Id) : IRequest<Result<PartnerDetailResponse>>;
