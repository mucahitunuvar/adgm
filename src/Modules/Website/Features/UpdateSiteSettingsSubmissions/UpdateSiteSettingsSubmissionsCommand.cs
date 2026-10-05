using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsSubmissions;

public sealed record UpdateSiteSettingsSubmissionsCommand(byte[] RowVersion, string? SubmissionReferencePrefix) : IRequest<Result>;
