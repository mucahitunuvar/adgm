using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;

public sealed record GetSiteLanguagesQuery : IRequest<Result<GetSiteLanguagesResponse>>;
