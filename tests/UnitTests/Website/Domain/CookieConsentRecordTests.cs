using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class CookieConsentRecordTests
{
    private static readonly LegalDocumentKey PolicyKey = LegalDocumentKey.Create("cookie-policy").Value;
    private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_AlwaysIncludesNecessaryCategory_EvenWhenNotRequested()
    {
        var result = CookieConsentRecord.Create(
            Guid.NewGuid(), [ThirdPartyScriptCategory.Marketing], PolicyKey, 1, CookieConsentAction.Custom, Now);

        Assert.True(result.IsSuccess);
        Assert.Contains(ThirdPartyScriptCategory.Necessary, result.Value.Categories);
        Assert.Contains(ThirdPartyScriptCategory.Marketing, result.Value.Categories);
    }

    [Fact]
    public void Create_WithNullCategories_StillIncludesNecessary()
    {
        var result = CookieConsentRecord.Create(Guid.NewGuid(), null, PolicyKey, 1, CookieConsentAction.RejectAll, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Categories, ThirdPartyScriptCategory.Necessary);
    }

    [Fact]
    public void Create_WithDuplicateCategories_DeduplicatesThem()
    {
        var result = CookieConsentRecord.Create(
            Guid.NewGuid(), [ThirdPartyScriptCategory.Necessary, ThirdPartyScriptCategory.Necessary, ThirdPartyScriptCategory.Analytics],
            PolicyKey, 1, CookieConsentAction.AcceptAll, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Categories.Count);
    }

    [Fact]
    public void Create_WithEmptyConsentId_Fails()
    {
        var result = CookieConsentRecord.Create(Guid.Empty, [], PolicyKey, 1, CookieConsentAction.AcceptAll, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CookieConsentRecord.ConsentIdRequired", result.Error.Code);
    }

    [Fact]
    public void Create_StoresPolicyKeyVersionActionAndRecordedAtUtc()
    {
        var consentId = Guid.NewGuid();

        var result = CookieConsentRecord.Create(consentId, [], PolicyKey, 3, CookieConsentAction.AcceptAll, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(consentId, result.Value.ConsentId);
        Assert.Equal(PolicyKey, result.Value.PolicyKey);
        Assert.Equal(3, result.Value.PolicyVersion);
        Assert.Equal(CookieConsentAction.AcceptAll, result.Value.Action);
        Assert.Equal(Now, result.Value.RecordedAtUtc);
    }
}
