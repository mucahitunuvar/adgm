using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Named CandidateReference (not "Reference") to avoid reading as ReferenceData's own ReferenceType/
// module naming, and because "Reference" alone is too generic a type name here (AGENTS.md §12).
// ReferenceTypeId/ReferenceLanguageId reference ReferenceData lookups - ReferenceLanguageId reuses
// the existing Language lookup (Candidate module master prompt: "ReferansDili için mevcut Language
// lookup'ı yeniden kullanılacak"), validated at write time by the Application layer, not here.
public sealed class CandidateReference : Entity
{
    public Guid CandidateCvContentId { get; private set; }

    public Guid ReferenceTypeId { get; private set; }

    public Guid ReferenceLanguageId { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string? Company { get; private set; }

    public string? Position { get; private set; }

    public string? Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    private CandidateReference(
        Guid id,
        Guid candidateCvContentId,
        Guid referenceTypeId,
        Guid referenceLanguageId,
        string firstName,
        string lastName)
        : base(id)
    {
        CandidateCvContentId = candidateCvContentId;
        ReferenceTypeId = referenceTypeId;
        ReferenceLanguageId = referenceLanguageId;
        FirstName = firstName;
        LastName = lastName;
    }

    internal static CandidateReference Create(
        Guid candidateCvContentId, Guid referenceTypeId, Guid referenceLanguageId, string firstName, string lastName) =>
        new(Guid.NewGuid(), candidateCvContentId, referenceTypeId, referenceLanguageId, firstName, lastName);

    public void Update(
        Guid referenceTypeId,
        Guid referenceLanguageId,
        string firstName,
        string lastName,
        string? company,
        string? position,
        string? email,
        string? phoneNumber)
    {
        ReferenceTypeId = referenceTypeId;
        ReferenceLanguageId = referenceLanguageId;
        FirstName = firstName;
        LastName = lastName;
        Company = company;
        Position = position;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
