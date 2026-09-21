using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetJobsPendingReview;

public sealed record GetJobsPendingReviewQuery : IRequest<Result<IReadOnlyList<JobResponse>>>;
