using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employment.Domain;

// Aggregate root (PROJECT.md §12). CandidateCvId/CompanyId/PositionId/InterviewId, Interview.
// Interview'daki desenle aynı - başka modüllere cross-module referans, FK yok, doğrulanmaz. Kayıt
// Interview'in pozitif sonucundan otomatik doğmaz - danışman işe yerleşmeyi kendisi, manuel olarak
// kaydeder (InterviewId yalnızca isteğe bağlı bir iz, hangi görüşmenin sonucunda olduğunu belirtmek
// için). Company/Job/PersonnelNeed/CandidateSuggestion/Interview'la tutarlı olarak event fırlatmaz
// (senkron/event-free tasarım).
//
// Modülün kök ad alanı da "Employment" olduğu için (GenclikMerkezi.Modules.Employment), Domain
// katmanının dışındaki her referans "Domain.Employment" olarak yazılmalı - Interview/CareerAdvisor
// modüllerindeki aynı isim çakışması deseni (bkz. Interview.cs'teki yorum).
public sealed class Employment : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid PositionId { get; private set; }

    public Guid? InterviewId { get; private set; }

    public DateTime StartDateUtc { get; private set; }

    public EmploymentStatus Status { get; private set; }

    public DateTime? EndDateUtc { get; private set; }

    public string? DepartureReason { get; private set; }

    public Guid CreatedByAdvisorId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Employment(
        Guid id,
        Guid candidateCvId,
        Guid companyId,
        Guid positionId,
        Guid? interviewId,
        DateTime startDateUtc,
        Guid createdByAdvisorId,
        DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        CompanyId = companyId;
        PositionId = positionId;
        InterviewId = interviewId;
        StartDateUtc = startDateUtc;
        Status = EmploymentStatus.DevamEdiyor;
        CreatedByAdvisorId = createdByAdvisorId;
        CreatedAtUtc = createdAtUtc;
    }

    public static Employment Create(
        Guid candidateCvId,
        Guid companyId,
        Guid positionId,
        Guid? interviewId,
        DateTime startDateUtc,
        Guid createdByAdvisorId,
        DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, companyId, positionId, interviewId, startDateUtc, createdByAdvisorId, createdAtUtc);

    public Result EndEmployment(string departureReason, DateTime endDateUtc)
    {
        if (Status != EmploymentStatus.DevamEdiyor)
        {
            return Result.Failure(Error.Conflict(
                "Employment.InvalidTransition", $"Cannot end an employment while status is {Status}."));
        }

        Status = EmploymentStatus.SonaErdi;
        EndDateUtc = endDateUtc;
        DepartureReason = departureReason;

        return Result.Success();
    }
}
