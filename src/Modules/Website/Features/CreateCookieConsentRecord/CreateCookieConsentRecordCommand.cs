using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;

public sealed record CreateCookieConsentRecordCommand(
    Guid ConsentId, IReadOnlyList<string>? Categories, int PolicyVersion, string? Action) : IRequest<Result>;
