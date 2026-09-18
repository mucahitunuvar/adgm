using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.CareerAdvisor.Domain;

// CandidateCvId/CandidateUserId, Candidate modülüne cross-module ID referansları - CareerAdvisor
// bunları doğrulayamaz (ADR-022 §1, aynı gerekçe CandidateNote.cs'te). CandidateUserId, ADR-022 §4'ün
// alan listesinde yok ama TarihOnerildi/Reddedildi geçişlerinde adaya bildirim göndermek için gerekli
// (CareerAdvisor, Candidate'ın CandidateCv'sini sorgulayamadığı için email/isim Identity'nin
// IIdentityService.GetUserProfileAsync contract'ından çekiliyor, oraya userId lazım).
public sealed class MeetingRequest : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid CandidateUserId { get; private set; }

    public Guid CareerAdvisorId { get; private set; }

    public MeetingRequestStatus Status { get; private set; }

    public DateTime? ProposedDateTimeUtc { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private MeetingRequest(Guid id, Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        CandidateUserId = candidateUserId;
        CareerAdvisorId = careerAdvisorId;
        Status = MeetingRequestStatus.TalepEdildi;
        CreatedAtUtc = createdAtUtc;
    }

    public static MeetingRequest Create(Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, candidateUserId, careerAdvisorId, createdAtUtc);

    public Result ProposeTime(DateTime proposedDateTimeUtc)
    {
        if (Status != MeetingRequestStatus.TalepEdildi)
        {
            return Result.Failure(Error.Conflict(
                "MeetingRequest.InvalidTransition", $"Cannot propose a time while status is {Status}."));
        }

        ProposedDateTimeUtc = proposedDateTimeUtc;
        Status = MeetingRequestStatus.TarihOnerildi;

        return Result.Success();
    }

    public Result Confirm(DateTime confirmedAtUtc)
    {
        if (Status != MeetingRequestStatus.TarihOnerildi)
        {
            return Result.Failure(Error.Conflict(
                "MeetingRequest.InvalidTransition", $"Cannot confirm while status is {Status}."));
        }

        ConfirmedAtUtc = confirmedAtUtc;
        Status = MeetingRequestStatus.Onaylandi;

        return Result.Success();
    }

    public Result Reject()
    {
        if (Status is not (MeetingRequestStatus.TalepEdildi or MeetingRequestStatus.TarihOnerildi))
        {
            return Result.Failure(Error.Conflict(
                "MeetingRequest.InvalidTransition", $"Cannot reject while status is {Status}."));
        }

        Status = MeetingRequestStatus.Reddedildi;

        return Result.Success();
    }
}
