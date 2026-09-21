using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;

public sealed record SubmitJobForReviewCommand(Guid JobId) : IRequest<Result>;
