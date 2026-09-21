using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetCompany;

public class GetCompanyQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();

    private GetCompanyQueryHandler CreateHandler() => new(_companyRepository);

    [Fact]
    public async Task Handle_WithExistingCompany_ReturnsMappedResponse()
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), 2010, 50, "https://acme.example.com", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Adres", "<p>Hakkımızda</p>", "Ayşe", "Kaya", "firma@example.com",
            "05550000000", Guid.NewGuid(), "1234567890", true, null, DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new GetCompanyQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id, result.Value.Id);
        Assert.Equal("Acme A.Ş.", result.Value.Name);
        Assert.Equal(CompanyStatus.PendingApproval, result.Value.Status);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetCompanyQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
