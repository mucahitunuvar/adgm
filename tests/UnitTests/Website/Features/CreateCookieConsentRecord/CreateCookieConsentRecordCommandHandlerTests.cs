using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.CreateCookieConsentRecord;

public class CreateCookieConsentRecordCommandHandlerTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime PublishedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();
    private readonly FakeLegalDocumentRepository _legalDocumentRepository = new();
    private readonly FakeCookieConsentRecordRepository _cookieConsentRecordRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateCookieConsentRecordCommandHandler CreateHandler()
    {
        var document = LegalDocument.Create("cerez-politikasi", LegalDocumentKind.CookiePolicy, Tr, "Çerez Politikası", UserId, PublishedAt).Value;
        document.CreateDraft(null, UserId, PublishedAt);
        document.UpdateDraftBody(Tr, "Politika metni", UserId, PublishedAt);
        document.PublishDraft(Tr, PublishedAt, UserId, PublishedAt);
        _legalDocumentRepository.Seed(document);

        var settings = SiteSettings.CreateDefault();
        settings.UpdateCookiePolicyKey(document.Key, UserId, PublishedAt);
        _siteSettingsRepository.Seed(settings);

        return new CreateCookieConsentRecordCommandHandler(
            _siteSettingsRepository, _legalDocumentRepository, _cookieConsentRecordRepository, TimeProvider.System, _unitOfWork);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("AcceptAll,RejectAll")] // 0|1 = 1 = RejectAll - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedAction_Fails(string action)
    {
        var result = await CreateHandler().Handle(
            new CreateCookieConsentRecordCommand(Guid.NewGuid(), [], 1, action), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CookieConsent.ActionInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("Necessary,Analytics")] // 0|1 = 1 = Analytics - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedCategory_Fails(string category)
    {
        var result = await CreateHandler().Handle(
            new CreateCookieConsentRecordCommand(Guid.NewGuid(), [category], 1, "Custom"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CookieConsent.CategoryInvalid", result.Error.Code);
    }
}
