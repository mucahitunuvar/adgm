using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.PoolPersonnelNeed;

public sealed record PoolPersonnelNeedCommand(Guid PersonnelNeedId) : IRequest<Result>;
