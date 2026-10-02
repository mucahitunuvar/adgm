using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.Partners;

public class PartnerPublicQueryServiceTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakePartnerRepository _partnerRepository = new();
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();

    private PartnerPublicQueryService CreateService() => new(_partnerRepository, _mediaAssetRepository, _fileStorageService);

    private Guid SeedLogo()
    {
        var id = Guid.NewGuid();
        var file = FileAttachment.Create($"website-images/{id}.jpg", "logo.jpg", "image/jpeg", 1024, Now, "MediaAsset", id);
        var image = MediaAsset.Create(
            id, MediaAssetKind.Image, file, [], 400, 200, MediaFolder.Create("partners").Value, null, null, false, UserId, Now).Value;

        _mediaAssetRepository.Seed(image);
        return id;
    }

    private Partner SeedPartner(Guid logoMediaId, bool isActive = true, LanguageCode? translationLanguage = null, int sortOrder = 1)
    {
        var partner = Partner.Create(
            logoMediaId, null, sortOrder, translationLanguage ?? Tr, "Örnek Partner", null, UserId, Now).Value;
        if (!isActive)
        {
            partner.Deactivate(UserId, Now);
        }

        _partnerRepository.Seed(partner);
        return partner;
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActivePartnersOrderedBySortOrder()
    {
        var logo = SeedLogo();
        SeedPartner(logo, sortOrder: 2);
        SeedPartner(logo, sortOrder: 1);

        var result = await CreateService().GetActiveAsync(Tr);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].SortOrder <= result[1].SortOrder);
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesInactivePartners()
    {
        var logo = SeedLogo();
        SeedPartner(logo, isActive: false);

        var result = await CreateService().GetActiveAsync(Tr);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesPartnersWithoutTranslationInRequestedLanguage()
    {
        var logo = SeedLogo();
        SeedPartner(logo, translationLanguage: Tr);

        var result = await CreateService().GetActiveAsync(En);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAsync_IncludesResolvedLogoVariantUrls()
    {
        var logo = SeedLogo();
        SeedPartner(logo);

        var result = await CreateService().GetActiveAsync(Tr);

        var partner = Assert.Single(result);
        Assert.NotEmpty(partner.Logo.Original);
    }
}
