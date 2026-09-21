using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employer.Domain;

// Aggregate root (ADR-023 §1). Company.md hariç Logo (ayrı bir upload adımı - UploadCandidatePhoto
// deseniyle sonradan eklenecek, bu görevin kapsamında değil). CandidateCv'nin aksine düz bir aggregate:
// child collection/owned value object yok, tüm alanlar skaler.
public sealed class Company : AggregateRoot
{
    public Guid UserId { get; private set; }

    // Kayıt anında, Admin onayından bağımsız ve önce atanır (ADR-023 §1: en-az-yüklü danışman).
    // Danışman deaktive edildiğinde Host-seviyesi ReassignOrphanedCompaniesCommand ile (ileriki görev)
    // yeniden atanabileceği için nullable/mutable kalıyor - CandidateCv.CareerAdvisorId ile aynı desen.
    public Guid? CareerAdvisorId { get; private set; }

    // Firma Bilgileri
    public string Name { get; private set; }

    public Guid SectorId { get; private set; }

    public int? FoundedYear { get; private set; }

    public int? EmployeeCount { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public Guid CountryId { get; private set; }

    public Guid ProvinceId { get; private set; }

    public Guid DistrictId { get; private set; }

    public string Address { get; private set; }

    public string? AboutHtml { get; private set; }

    // Hesap Bilgileri
    public string ContactFirstName { get; private set; }

    public string ContactLastName { get; private set; }

    public string ContactEmail { get; private set; }

    public string ContactPhone { get; private set; }

    // "Vergi Dairesi İli" ayrı bir alan değil (ADR-023 §1 notu): TaxOffice zaten Province'e bağlı.
    public Guid TaxOfficeId { get; private set; }

    public string TaxNumber { get; private set; }

    public bool MarketingConsent { get; private set; }

    // Sistem alanları
    public CompanyStatus Status { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }

    public DateTime? ApprovedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public Guid? DeactivatedByUserId { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Company(
        Guid id,
        Guid userId,
        string name,
        Guid sectorId,
        int? foundedYear,
        int? employeeCount,
        string? websiteUrl,
        Guid countryId,
        Guid provinceId,
        Guid districtId,
        string address,
        string? aboutHtml,
        string contactFirstName,
        string contactLastName,
        string contactEmail,
        string contactPhone,
        Guid taxOfficeId,
        string taxNumber,
        bool marketingConsent,
        Guid? careerAdvisorId,
        DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        Name = name;
        SectorId = sectorId;
        FoundedYear = foundedYear;
        EmployeeCount = employeeCount;
        WebsiteUrl = websiteUrl;
        CountryId = countryId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        Address = address;
        AboutHtml = aboutHtml;
        ContactFirstName = contactFirstName;
        ContactLastName = contactLastName;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        TaxOfficeId = taxOfficeId;
        TaxNumber = taxNumber;
        MarketingConsent = marketingConsent;
        CareerAdvisorId = careerAdvisorId;
        Status = CompanyStatus.PendingApproval;
        CreatedAtUtc = createdAtUtc;
    }

    public static Company Create(
        Guid userId,
        string name,
        Guid sectorId,
        int? foundedYear,
        int? employeeCount,
        string? websiteUrl,
        Guid countryId,
        Guid provinceId,
        Guid districtId,
        string address,
        string? aboutHtml,
        string contactFirstName,
        string contactLastName,
        string contactEmail,
        string contactPhone,
        Guid taxOfficeId,
        string taxNumber,
        bool marketingConsent,
        Guid? careerAdvisorId,
        DateTime createdAtUtc) =>
        new(
            Guid.NewGuid(),
            userId,
            name,
            sectorId,
            foundedYear,
            employeeCount,
            websiteUrl,
            countryId,
            provinceId,
            districtId,
            address,
            aboutHtml,
            contactFirstName,
            contactLastName,
            contactEmail,
            contactPhone,
            taxOfficeId,
            taxNumber,
            marketingConsent,
            careerAdvisorId,
            createdAtUtc);

    // Durum geçişleri Result döner (MeetingRequest.ProposeTime/Confirm/Reject deseni) - geçersiz
    // geçiş bir domain exception değil, çağıranın (command handler) ele alması gereken beklenen bir
    // iş kuralı ihlalidir.
    public Result Approve(Guid approvedByUserId, DateTime approvedAtUtc)
    {
        if (Status != CompanyStatus.PendingApproval)
        {
            return Result.Failure(Error.Conflict(
                "Company.InvalidTransition", $"Cannot approve a company while status is {Status}."));
        }

        Status = CompanyStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = approvedAtUtc;

        return Result.Success();
    }

    public Result Reject(string reason, DateTime rejectedAtUtc)
    {
        if (Status != CompanyStatus.PendingApproval)
        {
            return Result.Failure(Error.Conflict(
                "Company.InvalidTransition", $"Cannot reject a company while status is {Status}."));
        }

        Status = CompanyStatus.Rejected;
        RejectionReason = reason;

        return Result.Success();
    }

    // Yalnızca Approved -> Deactivated (master prompt: "zaten deaktifse veya onay bekliyorsa hata
    // fırlat"). MeetingRequest.Reject'in aksine burada idempotent no-op değil (CareerAdvisor.Deactivate
    // gibi) - master prompt açıkça bir hata bekliyor, sessizce yutmuyor.
    public Result Deactivate(Guid deactivatedByUserId, DateTime deactivatedAtUtc)
    {
        if (Status != CompanyStatus.Approved)
        {
            return Result.Failure(Error.Conflict(
                "Company.InvalidTransition", $"Cannot deactivate a company while status is {Status}."));
        }

        Status = CompanyStatus.Deactivated;
        DeactivatedByUserId = deactivatedByUserId;
        DeactivatedAtUtc = deactivatedAtUtc;

        return Result.Success();
    }

    // Kayıt sonrası yeniden atama için (ADR-022 §1 emsali: ReassignOrphanedCompaniesCommand, ileriki
    // görev). Hiç aktif danışman yoksa null geçilebilir - CandidateCv.AssignCareerAdvisor ile aynı desen.
    public void AssignCareerAdvisor(Guid? careerAdvisorId)
    {
        CareerAdvisorId = careerAdvisorId;
    }
}
