namespace GenclikMerkezi.Modules.Employment.Application.Abstractions;

// Domain.Employment her yerde tam nitelikli - modülün kendi kök ad alanı da "Employment" olduğu için
// (GenclikMerkezi.Modules.Employment), bir "using ...Domain;" + çıplak "Employment" C#'ın üye-arama
// önceliği yüzünden belirsizliğe yol açar (Interview/CareerAdvisor modüllerindeki aynı desen).
public interface IEmploymentRepository
{
    Task<Domain.Employment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Employment>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(Domain.Employment employment);
}
