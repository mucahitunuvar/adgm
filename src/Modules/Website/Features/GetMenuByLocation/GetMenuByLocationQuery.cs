using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;

public sealed record GetMenuByLocationQuery(string Location) : IRequest<Result<MenuResponse>>;
