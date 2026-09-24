using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record GetSiteSettingsQuery : IRequest<Result<SiteSettingsResponse>>;
