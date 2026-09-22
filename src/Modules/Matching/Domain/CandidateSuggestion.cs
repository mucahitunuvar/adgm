using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Matching.Domain;

// Aggregate root (ADR-022 §5-6, Görev 7). PersonnelNeedId/CandidateCvId, Company/Job'daki
// FulfilledByCandidateCvId deseniyle aynı - başka modüllere cross-module referans, FK yok. Company/
// Job/PersonnelNeed'le tutarlı olarak event fırlatmaz (Matching de senkron/event-free tasarım).
public sealed class CandidateSuggestion : AggregateRoot
{
    public Guid PersonnelNeedId { get; private set; }

    public Guid CandidateCvId { get; private set; }

    public Guid SuggestingAdvisorId { get; private set; }

    public CandidateSuggestionStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? DecidedByAdvisorId { get; private set; }

    public DateTime? DecidedAtUtc { get; private set; }

    private CandidateSuggestion(
        Guid id, Guid personnelNeedId, Guid candidateCvId, Guid suggestingAdvisorId, DateTime createdAtUtc)
        : base(id)
    {
        PersonnelNeedId = personnelNeedId;
        CandidateCvId = candidateCvId;
        SuggestingAdvisorId = suggestingAdvisorId;
        Status = CandidateSuggestionStatus.Onerildi;
        CreatedAtUtc = createdAtUtc;
    }

    public static CandidateSuggestion Create(
        Guid personnelNeedId, Guid candidateCvId, Guid suggestingAdvisorId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), personnelNeedId, candidateCvId, suggestingAdvisorId, createdAtUtc);

    public Result Accept(Guid decidedByAdvisorId, DateTime decidedAtUtc)
    {
        if (Status != CandidateSuggestionStatus.Onerildi)
        {
            return Result.Failure(Error.Conflict(
                "CandidateSuggestion.InvalidTransition", $"Cannot accept a suggestion while status is {Status}."));
        }

        Status = CandidateSuggestionStatus.KabulEdildi;
        DecidedByAdvisorId = decidedByAdvisorId;
        DecidedAtUtc = decidedAtUtc;

        return Result.Success();
    }

    public Result Reject(Guid decidedByAdvisorId, DateTime decidedAtUtc)
    {
        if (Status != CandidateSuggestionStatus.Onerildi)
        {
            return Result.Failure(Error.Conflict(
                "CandidateSuggestion.InvalidTransition", $"Cannot reject a suggestion while status is {Status}."));
        }

        Status = CandidateSuggestionStatus.Reddedildi;
        DecidedByAdvisorId = decidedByAdvisorId;
        DecidedAtUtc = decidedAtUtc;

        return Result.Success();
    }
}
