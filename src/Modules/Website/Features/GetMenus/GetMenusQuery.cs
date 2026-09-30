using GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMenus;

public sealed record GetMenusQuery : IRequest<Result<IReadOnlyList<MenuResponse>>>;
