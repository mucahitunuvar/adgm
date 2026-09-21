using System.Text;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.UploadCompanyLogo;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.UploadCompanyLogo;

public class UploadCompanyLogoCommandHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeFileStorageService _fileStorageService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UploadCompanyLogoCommandHandler CreateHandler(Guid? currentUserId) =>
        new(_companyRepository, new FakeCurrentUserContext(currentUserId), _fileStorageService, _unitOfWork);

    private Company AddCompanyForUser(Guid userId)
    {
        var company = Company.Create(
            userId, "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890",
            false, null, DateTime.UtcNow);
        _companyRepository.Add(company);
        return company;
    }

    private static MemoryStream CreateContent() => new(Encoding.UTF8.GetBytes("fake-image-bytes"));

    [Fact]
    public async Task Handle_AsOwner_UploadsLogo_AndSaves_AndReturnsUrl()
    {
        var ownerUserId = Guid.NewGuid();
        var company = AddCompanyForUser(ownerUserId);
        using var content = CreateContent();

        var result = await CreateHandler(ownerUserId).Handle(
            new UploadCompanyLogoCommand(company.Id, content, "logo.png", "image/png"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_fileStorageService.UrlToReturn, result.Value);
        Assert.Equal(_fileStorageService.UploadResult.Value, company.Logo);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.NotNull(_fileStorageService.UploadCall);
        Assert.Equal(FileCategory.EmployerLogo, _fileStorageService.UploadCall!.Value.Category);
        Assert.Equal("Company", _fileStorageService.UploadCall.Value.OwnerEntityType);
        Assert.Equal(company.Id, _fileStorageService.UploadCall.Value.OwnerEntityId);
    }

    [Fact]
    public async Task Handle_WithExistingLogo_DeletesThePreviousFile_AfterSaving()
    {
        var ownerUserId = Guid.NewGuid();
        var company = AddCompanyForUser(ownerUserId);
        var previousLogo = FileAttachment.Create(
            "employer-logos/2026/09/01/old.png", "old.png", "image/png", 512, DateTime.UtcNow, "Company", company.Id);
        company.SetLogo(previousLogo);
        using var content = CreateContent();

        var result = await CreateHandler(ownerUserId).Handle(
            new UploadCompanyLogoCommand(company.Id, content, "logo.png", "image/png"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var deletedKey = Assert.Single(_fileStorageService.DeletedFileKeys);
        Assert.Equal(previousLogo.FileKey, deletedKey);
    }

    [Fact]
    public async Task Handle_AsNonOwner_ReturnsForbidden_AndDoesNotSave()
    {
        var ownerUserId = Guid.NewGuid();
        var company = AddCompanyForUser(ownerUserId);
        using var content = CreateContent();

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new UploadCompanyLogoCommand(company.Id, content, "logo.png", "image/png"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Null(_fileStorageService.UploadCall);
    }

    [Fact]
    public async Task Handle_WithUnknownCompanyId_ReturnsNotFound()
    {
        using var content = CreateContent();

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new UploadCompanyLogoCommand(Guid.NewGuid(), content, "logo.png", "image/png"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenUploadFails_ReturnsFailure_AndDoesNotSave()
    {
        var ownerUserId = Guid.NewGuid();
        var company = AddCompanyForUser(ownerUserId);
        _fileStorageService.UploadResult = Result.Failure<FileAttachment>(
            Error.Validation("FileStorage.InvalidExtension", "File extension is not allowed."));
        using var content = CreateContent();

        var result = await CreateHandler(ownerUserId).Handle(
            new UploadCompanyLogoCommand(company.Id, content, "logo.exe", "application/octet-stream"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Null(company.Logo);
    }
}
