using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Interview.Domain;

// Aggregate root (PROJECT.md §8.4-8.5). CandidateCvId/CompanyId/OrganizingAdvisorId, Matching.
// CandidateSuggestion'daki desenle aynı - başka modüllere cross-module referans, FK yok. Company/Job/
// PersonnelNeed/CandidateSuggestion'la tutarlı olarak event fırlatmaz (senkron/event-free tasarım).
//
// Sonuç alanı bilinçli olarak "Outcome" adını taşıyor, master prompt'un önerdiği "Result" değil:
// SharedKernel.Results.Result bu sınıfın kendi durum-geçiş metodlarının (Schedule/RecordResult/Cancel)
// dönüş tipi ve içeride "return Result.Success()/Result.Failure(...)" olarak çağrılıyor. Aynı sınıfta
// "Result" adında bir instance property de olsaydı, metod gövdelerindeki unqualified "Result.Success()"
// çağrıları C#'ın üye-arama önceliği yüzünden bu instance property'ye yönlenip derleme hatası verirdi
// (bir property, expression bağlamında aynı adlı bir tip adının önüne geçer). "Outcome" bu çakışmayı
// önler, semantik olarak da doğru (görüşmenin sonucu).
public sealed class Interview : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid OrganizingAdvisorId { get; private set; }

    public InterviewStatus Status { get; private set; }

    public DateTime? ScheduledAtUtc { get; private set; }

    public InterviewResult? Outcome { get; private set; }

    public string? ResultNotes { get; private set; }

    public InterviewRequestedByRole RequestedByRole { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Interview(
        Guid id,
        Guid candidateCvId,
        Guid companyId,
        Guid organizingAdvisorId,
        InterviewRequestedByRole requestedByRole,
        DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        CompanyId = companyId;
        OrganizingAdvisorId = organizingAdvisorId;
        RequestedByRole = requestedByRole;
        Status = InterviewStatus.TalepEdildi;
        CreatedAtUtc = createdAtUtc;
    }

    public static Interview Create(
        Guid candidateCvId,
        Guid companyId,
        Guid organizingAdvisorId,
        InterviewRequestedByRole requestedByRole,
        DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, companyId, organizingAdvisorId, requestedByRole, createdAtUtc);

    public Result Schedule(DateTime scheduledAtUtc)
    {
        if (Status != InterviewStatus.TalepEdildi)
        {
            return Result.Failure(Error.Conflict(
                "Interview.InvalidTransition", $"Cannot schedule an interview while status is {Status}."));
        }

        Status = InterviewStatus.Planlandi;
        ScheduledAtUtc = scheduledAtUtc;

        return Result.Success();
    }

    public Result RecordResult(InterviewResult result, string? resultNotes, DateTime completedAtUtc)
    {
        if (Status != InterviewStatus.Planlandi)
        {
            return Result.Failure(Error.Conflict(
                "Interview.InvalidTransition", $"Cannot record a result while status is {Status}."));
        }

        Status = InterviewStatus.Tamamlandi;
        Outcome = result;
        ResultNotes = resultNotes;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is not (InterviewStatus.TalepEdildi or InterviewStatus.Planlandi))
        {
            return Result.Failure(Error.Conflict(
                "Interview.InvalidTransition", $"Cannot cancel an interview while status is {Status}."));
        }

        Status = InterviewStatus.IptalEdildi;

        return Result.Success();
    }
}
