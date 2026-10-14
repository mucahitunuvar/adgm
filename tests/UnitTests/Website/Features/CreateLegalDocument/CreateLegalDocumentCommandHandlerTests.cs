using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Features.CreateLegalDocument;

public class CreateLegalDocumentCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeLegalDocumentRepository _legalDocumentRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly ICacheService _cacheService =
        new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings()));

    private CreateLegalDocumentCommandHandler CreateHandler()
    {
        var defaultLanguage = SiteLanguage.Create(Tr, "Türkçe", 1, UserId, Now);
        defaultLanguage.MarkAsDefault(UserId, Now);
        _siteLanguageRepository.Seed(defaultLanguage);

        return new CreateLegalDocumentCommandHandler(
            _legalDocumentRepository, _siteLanguageRepository, new FakeCurrentUserContext(UserId), TimeProvider.System, _cacheService,
            new FakeUnitOfWork());
    }

    [Theory]
    [InlineData("999")]
    [InlineData("PrivacyNotice,ExplicitConsent")] // 0|1 = 1 = ExplicitConsent - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedKind_Fails(string kind)
    {
        var result = await CreateHandler().Handle(
            new CreateLegalDocumentCommand("gizlilik-bildirimi", kind, "Gizlilik Bildirimi"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocument.InvalidKind", result.Error.Code);
    }
}
