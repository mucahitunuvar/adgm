using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemap;

public sealed record GetSitemapQuery : IRequest<Result<string>>;
