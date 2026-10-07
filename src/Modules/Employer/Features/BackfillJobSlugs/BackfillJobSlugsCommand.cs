using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.BackfillJobSlugs;

public sealed record BackfillJobSlugsCommand : IRequest<Result<BackfillJobSlugsResponse>>;
