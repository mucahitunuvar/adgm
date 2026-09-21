using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetMyCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetMyCompany;

public class GetMyCompanyQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();

    private GetMyCompanyQueryHandler CreateHandler(Guid? currentUserId) =>
        new(_companyRepository, new FakeCurrentUserContext(currentUserId));

    [Fact]
    public async Task Handle_WithCompanyOwnedByCurrentUser_ReturnsMappedResponse()
    {
        var userId = Guid.NewGuid();
        var company = Company.Create(
            userId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler(userId).Handle(new GetMyCompanyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id, result.Value.Id);
    }

    [Fact]
    public async Task Handle_WithNoCompanyForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(new GetMyCompanyQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
