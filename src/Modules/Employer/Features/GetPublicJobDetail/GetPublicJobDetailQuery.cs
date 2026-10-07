using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;

public sealed record GetPublicJobDetailQuery(string Slug) : IRequest<Result<GetPublicJobDetailResponse>>;
