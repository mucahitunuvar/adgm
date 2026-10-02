using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicHome;

public sealed record GetPublicHomeQuery(string? Lang) : IRequest<Result<PublicHomeResponse>>;
