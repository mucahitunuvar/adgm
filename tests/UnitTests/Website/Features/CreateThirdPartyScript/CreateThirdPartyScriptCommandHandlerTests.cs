using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.CreateThirdPartyScript;

public class CreateThirdPartyScriptCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeThirdPartyScriptRepository _thirdPartyScriptRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private CreateThirdPartyScriptCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        return new CreateThirdPartyScriptCommandHandler(
            _thirdPartyScriptRepository, _siteLanguageRepository, Options.Create(new WebsiteScriptSettings()),
            new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService, new FakeUnitOfWork());
    }

    private static CreateThirdPartyScriptCommand CreateCommand(string? category = "Analytics", string? placement = "Head") =>
        new(
            "GoogleAnalytics4", "G-ABC123", null, null, null, false, false, category, placement, 1, "Google Analytics",
            "İstatistik");

    [Theory]
    [InlineData("999")]
    [InlineData("Necessary,Analytics")] // 0|1 = 1 = Analytics - would otherwise silently alias.
    [InlineData("necessary")] // case-sensitive: lowercase must not match 'Necessary'.
    public async Task Handle_WithNumericCombinedOrWrongCaseCategory_Fails(string category)
    {
        var result = await CreateHandler().Handle(CreateCommand(category), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScript.CategoryInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Head,BodyEnd")] // 0|1 = 1 = BodyEnd - would otherwise silently alias.
    [InlineData("head")] // case-sensitive: lowercase must not match 'Head'.
    public async Task Handle_WithNumericCombinedOrWrongCasePlacement_Fails(string placement)
    {
        var result = await CreateHandler().Handle(CreateCommand(placement: placement), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScript.PlacementInvalid", result.Error.Code);
    }
}
