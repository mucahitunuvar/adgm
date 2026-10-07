using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.SetCompanyLogoVisibility;

public class SetCompanyLogoVisibilityCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private SetCompanyLogoVisibilityCommandHandler CreateHandler(Guid? currentUserId) =>
        new(_companyRepository, new FakeCurrentUserContext(currentUserId), _unitOfWork);

    private static Company CreateCompanyWithLogo(Guid userId)
    {
        var company = Company.Create(
            userId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        company.SetLogo(FileAttachment.Create(
            "employer-logos/2026/09/21/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "Company", company.Id));
        return company;
    }

    [Fact]
    public async Task Handle_WithValidRowVersion_EnablesVisibility_AndSaves()
    {
        var userId = Guid.NewGuid();
        var company = CreateCompanyWithLogo(userId);
        _companyRepository.Add(company);

        var result = await CreateHandler(userId).Handle(
            new SetCompanyLogoVisibilityCommand(true, company.RowVersion), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(company.ShowLogoOnWebsite);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithoutLogo_ReturnsValidationError_AndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        var company = Company.Create(
            userId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler(userId).Handle(
            new SetCompanyLogoVisibilityCommand(true, company.RowVersion), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithStaleRowVersion_ReturnsConflict_AndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        var company = CreateCompanyWithLogo(userId);
        var staleRowVersion = company.RowVersion;
        company.SetShowLogoOnWebsite(true); // bumps RowVersion, simulating a concurrent change
        _companyRepository.Add(company);

        var result = await CreateHandler(userId).Handle(
            new SetCompanyLogoVisibilityCommand(false, staleRowVersion), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new SetCompanyLogoVisibilityCommand(true, [1, 2, 3]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    // "Başka firma değiştiremez" (master prompt Testler): companyId komutta/route'ta yok - çağıranın
    // kendi firması GetByUserIdAsync ile çözülüyor, bu yüzden bir kullanıcı başka bir firmanın
    // RowVersion'ını bilse bile yalnızca kendi firması üzerinde etkili olur.
    [Fact]
    public async Task Handle_OnlyAffectsCallersOwnCompany()
    {
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var ownCompany = CreateCompanyWithLogo(ownerUserId);
        var otherCompany = CreateCompanyWithLogo(otherUserId);
        _companyRepository.Add(ownCompany);
        _companyRepository.Add(otherCompany);

        var result = await CreateHandler(ownerUserId).Handle(
            new SetCompanyLogoVisibilityCommand(true, ownCompany.RowVersion), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(ownCompany.ShowLogoOnWebsite);
        Assert.False(otherCompany.ShowLogoOnWebsite);
    }
}
