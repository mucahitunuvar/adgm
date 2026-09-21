using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.RegisterEmployer;

public class RegisterEmployerCommandHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RegisterEmployerCommandHandler CreateHandler() =>
        new(_identityService, _careerAdvisorModuleContract, _companyRepository, _unitOfWork);

    private static RegisterEmployerCommand ValidCommand() =>
        new(
            Email: "firma@example.com",
            Password: "Sifre123",
            Name: "Acme A.Ş.",
            SectorId: Guid.NewGuid(),
            FoundedYear: 2010,
            EmployeeCount: 50,
            WebsiteUrl: "https://acme.example.com",
            CountryId: Guid.NewGuid(),
            ProvinceId: Guid.NewGuid(),
            DistrictId: Guid.NewGuid(),
            Address: "Örnek Mah. No:1",
            AboutHtml: "<p>Hakkımızda</p>",
            ContactFirstName: "Ayşe",
            ContactLastName: "Kaya",
            ContactPhone: "05551234567",
            TaxOfficeId: Guid.NewGuid(),
            TaxNumber: "1234567890",
            MarketingConsent: true);

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserThenCompany()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateUserResult = Result.Success(userId);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);

        var company = Assert.Single(_companyRepository.Companies);
        Assert.Equal(userId, company.UserId);
        Assert.Equal("Acme A.Ş.", company.Name);
        Assert.Equal("firma@example.com", company.ContactEmail);
        Assert.Equal(CompanyStatus.PendingApproval, company.Status);
        Assert.Equal(company.Id, result.Value.CompanyId);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
        Assert.Null(company.CareerAdvisorId);
    }

    [Fact]
    public async Task Handle_WithNoActiveCareerAdvisors_RegistersSuccessfully_AndLeavesCareerAdvisorIdNull()
    {
        _identityService.CreateUserResult = Result.Success(Guid.NewGuid());
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var company = Assert.Single(_companyRepository.Companies);
        Assert.Null(company.CareerAdvisorId);
    }

    [Fact]
    public async Task Handle_WithActiveCareerAdvisors_AssignsTheLeastLoadedOne()
    {
        _identityService.CreateUserResult = Result.Success(Guid.NewGuid());
        var busyAdvisorId = Guid.NewGuid();
        var idleAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors =
        [
            new ActiveCareerAdvisorSummary(busyAdvisorId),
            new ActiveCareerAdvisorSummary(idleAdvisorId),
        ];

        // busyAdvisorId already has two existing (non-rejected/deactivated) companies, idleAdvisorId has none.
        _companyRepository.Add(CreateCompanyAssignedTo(busyAdvisorId));
        _companyRepository.Add(CreateCompanyAssignedTo(busyAdvisorId));

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var newCompany = _companyRepository.Companies.Single(c => c.ContactEmail == "firma@example.com");
        Assert.Equal(idleAdvisorId, newCompany.CareerAdvisorId);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceReportsEmailAlreadyExists_ReturnsFailure_AndDoesNotCreateCompany()
    {
        var error = Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        _identityService.CreateUserResult = Result.Failure<Guid>(error);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(_companyRepository.Companies);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
    }

    [Fact]
    public async Task Handle_WhenCompanyPersistenceFails_DeactivatesUser_AndRethrows()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateUserResult = Result.Success(userId);
        _unitOfWork.ThrowOnSave = new InvalidOperationException("Simulated Company persistence failure.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(ValidCommand(), CancellationToken.None));

        Assert.True(_identityService.DeactivateUserAsyncCalled);
        Assert.Equal(userId, _identityService.DeactivatedUserId);
    }

    private static Company CreateCompanyAssignedTo(Guid careerAdvisorId) =>
        Company.Create(
            Guid.NewGuid(), "Diğer Firma", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", $"{Guid.NewGuid():N}@example.com", "05550000000", Guid.NewGuid(),
            Guid.NewGuid().ToString("N")[..10], false, careerAdvisorId, DateTime.UtcNow);
}
