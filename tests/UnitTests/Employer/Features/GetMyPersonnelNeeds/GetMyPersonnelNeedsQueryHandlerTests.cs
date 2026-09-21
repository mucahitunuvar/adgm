using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetMyPersonnelNeeds;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetMyPersonnelNeeds;

public class GetMyPersonnelNeedsQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly Guid _employerUserId = Guid.NewGuid();

    private GetMyPersonnelNeedsQueryHandler CreateHandler() =>
        new(_companyRepository, _personnelNeedRepository, new FakeCurrentUserContext(_employerUserId));

    private static PersonnelNeed CreatePersonnelNeed(Guid companyId) =>
        PersonnelNeed.Create(
            companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_ReturnsOnlyPersonnelNeedsBelongingToCurrentUsersCompany()
    {
        var company = Company.Create(
            _employerUserId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var own = CreatePersonnelNeed(company.Id);
        _personnelNeedRepository.Add(own);

        var otherCompanyPersonnelNeed = CreatePersonnelNeed(Guid.NewGuid());
        _personnelNeedRepository.Add(otherCompanyPersonnelNeed);

        var result = await CreateHandler().Handle(new GetMyPersonnelNeedsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value);
        Assert.Equal(own.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetMyPersonnelNeedsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
