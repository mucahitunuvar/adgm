using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitPersonnelNeed;

public sealed record SubmitPersonnelNeedCommand(Guid PersonnelNeedId) : IRequest<Result>;
