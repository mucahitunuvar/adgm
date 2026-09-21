using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Company?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // En-az-yüklü danışman seçimi için (ADR-022 §2). Yalnızca "aktif" sayılan (Rejected/Deactivated
    // olmayan) şirketleri sayar - ADR-023: bir Company Rejected/Deactivated olduğunda atanmış olduğu
    // danışmanın aktif listesinden düşer. Hiç şirketi olmayan danışmanlar sonuçta hiç görünmez
    // (GroupBy boş grup döndürmez) - çağıran taraf eksik anahtarı 0 olarak ele almalı.
    Task<IReadOnlyDictionary<Guid, int>> GetCompanyCountsByCareerAdvisorAsync(CancellationToken cancellationToken = default);

    void Add(Company company);
}
