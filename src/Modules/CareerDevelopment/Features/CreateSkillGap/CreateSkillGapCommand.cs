using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;

public sealed record CreateSkillGapCommand(Guid CandidateCvId, Guid SkillId, string? Notes) : IRequest<Result<CreateSkillGapResponse>>;
