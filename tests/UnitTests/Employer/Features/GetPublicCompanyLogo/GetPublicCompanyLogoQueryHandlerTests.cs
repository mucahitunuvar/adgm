using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyLogo;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicCompanyLogo;

public class GetPublicCompanyLogoQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeFileStorageService _fileStorageService = new();

    private GetPublicCompanyLogoQueryHandler CreateHandler() => new(_companyRepository, _fileStorageService);

    private static Company CreateCompany() =>
        Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(),
            "1234567890", false, null, DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithApprovedCompany_ConsentGiven_AndLogoPresent_ReturnsFileBytes()
    {
        var company = CreateCompany();
        company.SetLogo(FileAttachment.Create(
            "employer-logos/2026/09/21/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "Company", company.Id));
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        company.SetShowLogoOnWebsite(true);
        _companyRepository.Add(company);
        _fileStorageService.ReadResult = [1, 2, 3];

        var result = await CreateHandler().Handle(new GetPublicCompanyLogoQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Value.Content);
        Assert.Equal("image/png", result.Value.ContentType);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetPublicCompanyLogoQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithCompanyNotApproved_ReturnsNotFound()
    {
        var company = CreateCompany();
        company.SetLogo(FileAttachment.Create(
            "employer-logos/2026/09/21/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "Company", company.Id));
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new GetPublicCompanyLogoQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithShowLogoOnWebsiteDisabled_ReturnsNotFound()
    {
        var company = CreateCompany();
        company.SetLogo(FileAttachment.Create(
            "employer-logos/2026/09/21/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "Company", company.Id));
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new GetPublicCompanyLogoQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithoutLogo_ReturnsNotFound()
    {
        var company = CreateCompany();
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new GetPublicCompanyLogoQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
