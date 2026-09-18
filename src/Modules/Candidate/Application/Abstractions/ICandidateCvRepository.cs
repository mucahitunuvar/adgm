using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public interface ICandidateCvRepository
{
    Task<CandidateCv?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CandidateCv?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // En-az-yüklü danışman seçimi için (Görev 2/ADR-022 §2). Hiç adayı olmayan danışmanlar sonuçta
    // hiç görünmez (GroupBy boş grup döndürmez) - çağıran taraf eksik anahtarı 0 olarak ele almalı.
    Task<IReadOnlyDictionary<Guid, int>> GetCandidateCountsByCareerAdvisorAsync(CancellationToken cancellationToken = default);

    // Görev 3/ADR-022 §1: bir danışman deaktive edildiğinde ona atanmış tüm adayları bulup yeniden
    // atamak için. Tracked (AsNoTracking değil) - çağıran taraf AssignCareerAdvisor ile mutasyon yapacak.
    Task<IReadOnlyList<CandidateCv>> GetByCareerAdvisorIdAsync(Guid careerAdvisorId, CancellationToken cancellationToken = default);

    void Add(CandidateCv candidateCv);
}
