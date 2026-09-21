using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// JobGenderPreference ile aynı desen: GenderId ReferenceData'nın Gender lookup'ına referans verir,
// write-time'da doğrulanmaz (ADR-016).
public sealed class PersonnelNeedGenderPreference : Entity
{
    public Guid PersonnelNeedId { get; private set; }

    public Guid GenderId { get; private set; }

    private PersonnelNeedGenderPreference(Guid id, Guid personnelNeedId, Guid genderId)
        : base(id)
    {
        PersonnelNeedId = personnelNeedId;
        GenderId = genderId;
    }

    internal static PersonnelNeedGenderPreference Create(Guid personnelNeedId, Guid genderId) =>
        new(Guid.NewGuid(), personnelNeedId, genderId);
}
