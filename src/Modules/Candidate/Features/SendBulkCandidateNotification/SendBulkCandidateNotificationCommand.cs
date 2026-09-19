using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;

// Görev 8: "danışmanın kendi adaylarının tamamına (veya seçili alt kümesine)" - CandidateCvIds
// null/boş ise tümü, doluysa yalnızca bu id'lerle kesişen (ve çağıran danışmana ait olan) adaylar
// hedeflenir.
public sealed record SendBulkCandidateNotificationCommand(IReadOnlyList<Guid>? CandidateCvIds, string Subject, string Message)
    : IRequest<Result<SendBulkCandidateNotificationResponse>>;
