using GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetOwnPoolPersonnelNeeds;

public sealed record GetOwnPoolPersonnelNeedsQuery : IRequest<Result<IReadOnlyList<PersonnelNeedResponse>>>;
