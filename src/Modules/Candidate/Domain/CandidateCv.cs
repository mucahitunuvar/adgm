using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Header aggregate (ADR-017 / Candidate module design ADR): the nadiren-değişen fields shown on
// list/search screens. CandidateCvContent (the collection-heavy, sık-güncellenen detail data) is a
// separate aggregate on purpose - see that ADR's rationale.
//
// CountryId/ProvinceId are documented as "Zorunlu" in Candidate.md's field table, but are modeled
// as nullable here: a CandidateCv is first created synchronously during registration (Görev 5),
// seeded only with what the User already has (name/email/phone) - country/province are not known
// yet at that point. "Zorunlu" describes what a complete profile needs (CompletionPercentage exists
// precisely to track that), not a row-insert constraint; the Application layer enforces it when the
// candidate actually submits this section (Görev 7).
public sealed class CandidateCv : AggregateRoot
{
    private readonly List<SocialMediaLink> _socialMediaLinks = [];

    public Guid UserId { get; private set; }

    // İletişim Bilgileri
    public FileAttachment? Photo { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public Guid? CountryId { get; private set; }

    public Guid? ProvinceId { get; private set; }

    public Guid? DistrictId { get; private set; }

    public string? Address { get; private set; }

    public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks => _socialMediaLinks.AsReadOnly();

    // Kişisel Bilgiler
    public string? Title { get; private set; }

    public Guid? GenderId { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public Guid? DriversLicenseTypeId { get; private set; }

    public Guid? NationalityId { get; private set; }

    public decimal? NetSalaryExpectation { get; private set; }

    public Guid? MilitaryStatusId { get; private set; }

    public DisabilityInfo? DisabilityInfo { get; private set; }

    // Sistem alanları
    public Guid? CareerAdvisorId { get; private set; }

    public int CompletionPercentage { get; private set; }

    private CandidateCv(Guid id, Guid userId, string firstName, string lastName, string email, string? phoneNumber)
        : base(id)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    // Kayıt anında Identity.User'dan bir kerelik seed edilir (ADR-017 Decision 2); sonrasında
    // UpdateContactInfo ile bağımsız olarak düzenlenebilir, User'la senkron kalmaz.
    public static CandidateCv Create(Guid userId, string firstName, string lastName, string email, string? phoneNumber) =>
        new(Guid.NewGuid(), userId, firstName, lastName, email, phoneNumber);

    public void UpdateContactInfo(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        Guid? countryId,
        Guid? provinceId,
        Guid? districtId,
        string? address)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        CountryId = countryId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        Address = address;

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
    }

    public void UpdatePersonalInfo(
        string? title,
        Guid? genderId,
        DateOnly? birthDate,
        Guid? driversLicenseTypeId,
        Guid? nationalityId,
        decimal? netSalaryExpectation,
        Guid? militaryStatusId)
    {
        Title = title;
        GenderId = genderId;
        BirthDate = birthDate;
        DriversLicenseTypeId = driversLicenseTypeId;
        NationalityId = nationalityId;
        NetSalaryExpectation = netSalaryExpectation;
        MilitaryStatusId = militaryStatusId;

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
    }

    public void UpdateDisabilityInfo(DisabilityInfo? disabilityInfo)
    {
        DisabilityInfo = disabilityInfo;

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
    }

    public void SetPhoto(FileAttachment? photo)
    {
        Photo = photo;

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
    }

    public SocialMediaLink AddSocialMediaLink(string platform, string url)
    {
        var link = SocialMediaLink.Create(Id, platform, url);
        _socialMediaLinks.Add(link);

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));

        return link;
    }

    public void RemoveSocialMediaLink(Guid socialMediaLinkId)
    {
        var link = _socialMediaLinks.FirstOrDefault(l => l.Id == socialMediaLinkId);

        if (link is not null)
        {
            _socialMediaLinks.Remove(link);
            RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
        }
    }

    // CareerAdvisor modülü henüz yok; alan nullable/deferred (ADR-017 Decision 6).
    public void AssignCareerAdvisor(Guid? careerAdvisorId)
    {
        CareerAdvisorId = careerAdvisorId;

        RaiseDomainEvent(new CandidateCvUpdatedDomainEvent(Id));
    }

    // Görev 6'daki profil tamamlanma read-model handler'ı tarafından çağrılır. Bilerek
    // CandidateCvUpdatedDomainEvent raise ETMEZ - aksi halde handler kendi kendini sonsuz döngüde
    // tekrar tetikler (recalculate -> UpdateCompletionPercentage -> event -> recalculate -> ...).
    public void UpdateCompletionPercentage(int percentage)
    {
        CompletionPercentage = percentage;
    }
}
