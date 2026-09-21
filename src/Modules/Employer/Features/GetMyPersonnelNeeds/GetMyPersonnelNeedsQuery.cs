using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;

public sealed record GetMyPersonnelNeedsQuery : IRequest<Result<IReadOnlyList<PersonnelNeedResponse>>>;
