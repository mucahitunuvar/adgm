using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.CareerAdvisor;

// The published, in-process contract other modules depend on instead of CareerAdvisor's own
// DbContext (same pattern as IIdentityService / IReferenceDataLookupReader - ADR-016 Decision 2,
// Option C). Implemented in CareerAdvisor.Infrastructure, registered once at the Host composition
// root. Kept deliberately minimal: consuming modules (Candidate/Employer) compute least-loaded
// assignment themselves from their own CareerAdvisorId groupings, so only the active advisor id
// list is exposed here, not workload counts.
public interface ICareerAdvisorModuleContract
{
    Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(CancellationToken cancellationToken = default);

    // Görev 5/ADR-022 §4: adayın kendi danışmanından görüşme talep etmesi (Candidate → CareerAdvisor
    // yönü). candidateUserId, MeetingRequest.CandidateUserId'ye seed edilir - CareerAdvisor, Candidate
    // modülünü sorgulayamadığı için ileride adaya bildirim gönderirken (tarih önerme/reddetme)
    // Identity'nin GetUserProfileAsync contract'ı üzerinden bu id kullanılır.
    Task<Result<Guid>> CreateMeetingRequestAsync(
        Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, CancellationToken cancellationToken = default);

    // Görev 5/ADR-022 §4: aday, danışmanın önerdiği tarihi onaylar (yine Candidate → CareerAdvisor
    // yönü). candidateUserId, MeetingRequest.CandidateUserId ile eşleşmezse Forbidden döner - çağıranın
    // yalnızca kendi talebini onaylayabilmesini garanti eder.
    Task<Result> ConfirmMeetingRequestAsync(
        Guid meetingRequestId, Guid candidateUserId, CancellationToken cancellationToken = default);

    // Görev 8: Candidate modülünün, "bu kullanıcı aktif bir danışman mı" sorusunu sorması için -
    // Candidate kendi CandidateCv.CareerAdvisorId verisini bu CareerAdvisorId'yle gruplar. null,
    // kullanıcının aktif bir danışman olmadığı anlamına gelir.
    Task<Guid?> GetAdvisorIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Görev 8/ADR-022 §6: "CareerAdvisor → Notification: toplu bildirim". candidateUserIds,
    // çağıran modül (Candidate) tarafından zaten sahiplik doğrulaması yapılmış bir liste olarak
    // gelir - CareerAdvisor bu listeyi tekrar doğrulamaz (Candidate verisini sorgulayamaz).
    Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> candidateUserIds, string subject, string message, CancellationToken cancellationToken = default);
}
