using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;

public sealed record GetSubmissionTokenQuery : IRequest<Result<GetSubmissionTokenResponse>>;
