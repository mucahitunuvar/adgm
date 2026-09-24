using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

public sealed record GetPublicSiteSettingsQuery : IRequest<Result<PublicSiteSettingsResponse>>;
