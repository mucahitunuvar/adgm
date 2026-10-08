using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetRobotsTxt;

public sealed record GetRobotsTxtQuery : IRequest<Result<string>>;
