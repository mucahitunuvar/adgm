using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

public sealed class Certificate : Entity
{
    public Guid CandidateCvContentId { get; private set; }

    public string Name { get; private set; }

    public string IssuingInstitution { get; private set; }

    public DateOnly? CertificateDate { get; private set; }

    public string? Description { get; private set; }

    private Certificate(Guid id, Guid candidateCvContentId, string name, string issuingInstitution)
        : base(id)
    {
        CandidateCvContentId = candidateCvContentId;
        Name = name;
        IssuingInstitution = issuingInstitution;
    }

    internal static Certificate Create(Guid candidateCvContentId, string name, string issuingInstitution) =>
        new(Guid.NewGuid(), candidateCvContentId, name, issuingInstitution);

    public void Update(string name, string issuingInstitution, DateOnly? certificateDate, string? description)
    {
        Name = name;
        IssuingInstitution = issuingInstitution;
        CertificateDate = certificateDate;
        Description = description;
    }
}
