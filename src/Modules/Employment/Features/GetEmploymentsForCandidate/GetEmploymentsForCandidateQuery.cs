using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;

public sealed record GetEmploymentsForCandidateQuery(Guid CandidateCvId) : IRequest<Result<IReadOnlyList<EmploymentResponse>>>;
