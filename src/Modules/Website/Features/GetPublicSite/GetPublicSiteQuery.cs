using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

public sealed record GetPublicSiteQuery(string? Lang) : IRequest<Result<PublicSiteResponse>>;
