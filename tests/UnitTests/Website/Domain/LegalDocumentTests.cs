using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class LegalDocumentTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private static LegalDocument CreateDocument(string key = "kvkk-contact") =>
        LegalDocument.Create(key, LegalDocumentKind.PrivacyNotice, Tr, "KVKK Aydınlatma Metni", UserId, Now).Value;

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = LegalDocument.Create("kvkk-contact", LegalDocumentKind.PrivacyNotice, Tr, "KVKK Aydınlatma Metni", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("kvkk-contact", result.Value.Key.Value);
        Assert.Equal(LegalDocumentKind.PrivacyNotice, result.Value.Kind);
        Assert.Single(result.Value.Translations);
        Assert.Empty(result.Value.Versions);
        Assert.False(result.Value.HasDraft);
        Assert.False(result.Value.HasEverBeenPublished);
    }

    [Fact]
    public void Create_WithInvalidKey_Fails()
    {
        var result = LegalDocument.Create("invalid key", LegalDocumentKind.PrivacyNotice, Tr, "Title", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocumentKey.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyDefaultLanguageTitle_Fails()
    {
        var result = LegalDocument.Create("kvkk-contact", LegalDocumentKind.PrivacyNotice, Tr, "  ", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocumentTranslation.TitleInvalid", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_AddsTranslation()
    {
        var document = CreateDocument();

        var result = document.SetTranslation(En, "Privacy Notice", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, document.Translations.Count);
    }

    [Fact]
    public void SetTranslation_ForExistingLanguage_UpdatesInPlace()
    {
        var document = CreateDocument();

        var result = document.SetTranslation(Tr, "Yeni Başlık", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(document.Translations);
        Assert.Equal("Yeni Başlık", document.Translations[0].Title);
    }

    [Fact]
    public void CreateDraft_WhenNoDraftExists_CreatesVersionOne()
    {
        var document = CreateDocument();

        var result = document.CreateDraft(null, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(document.HasDraft);
        Assert.Single(document.Versions);
        Assert.Equal(1, document.Versions[0].VersionNumber);
        Assert.Equal(LegalDocumentVersionStatus.Draft, document.Versions[0].Status);
        Assert.Empty(document.Versions[0].Translations);
    }

    [Fact]
    public void CreateDraft_WhenDraftAlreadyExists_Fails()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var result = document.CreateDraft(null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocument.DraftAlreadyExists", result.Error.Code);
    }

    [Fact]
    public void CreateDraft_AfterAPublishedVersion_CopiesBodyFromTheEffectiveVersion()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>İlk metin</p>", UserId, Now);
        document.PublishDraft(Tr, null, UserId, Now);

        var result = document.CreateDraft("İkinci sürüm", UserId, Now);

        Assert.True(result.IsSuccess);
        var draft = document.Versions.Single(v => v.Status == LegalDocumentVersionStatus.Draft);
        Assert.Equal(2, draft.VersionNumber);
        Assert.Single(draft.Translations);
        Assert.Equal("<p>İlk metin</p>", draft.Translations[0].Body);
    }

    [Fact]
    public void UpdateDraftBody_ForNewLanguage_AddsTranslation()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var result = document.UpdateDraftBody(Tr, "<p>Gövde</p>", UserId, Now);

        Assert.True(result.IsSuccess);
        var draft = document.Versions.Single();
        Assert.Single(draft.Translations);
        Assert.Equal("<p>Gövde</p>", draft.Translations[0].Body);
    }

    [Fact]
    public void UpdateDraftBody_WithEmptyBody_Fails()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var result = document.UpdateDraftBody(Tr, "   ", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocumentVersionTranslation.BodyRequired", result.Error.Code);
    }

    [Fact]
    public void UpdateDraftBody_WhenNoDraft_Fails()
    {
        var document = CreateDocument();

        var result = document.UpdateDraftBody(Tr, "<p>Gövde</p>", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocument.DraftNotFound", result.Error.Code);
    }

    [Fact]
    public void PublishDraft_WithoutDefaultLanguageBody_Fails()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var result = document.PublishDraft(Tr, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocumentVersion.DefaultLanguageBodyRequired", result.Error.Code);
    }

    [Fact]
    public void PublishDraft_WhenNoDraft_Fails()
    {
        var document = CreateDocument();

        var result = document.PublishDraft(Tr, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocument.DraftNotFound", result.Error.Code);
    }

    [Fact]
    public void PublishDraft_WithoutExplicitEffectiveDate_UsesNow()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>Gövde</p>", UserId, Now);

        var result = document.PublishDraft(Tr, null, UserId, Now);

        Assert.True(result.IsSuccess);
        var version = document.Versions.Single();
        Assert.Equal(LegalDocumentVersionStatus.Published, version.Status);
        Assert.Equal(Now, version.EffectiveAtUtc);
        Assert.Equal(Now, version.PublishedAtUtc);
        Assert.Equal(UserId, version.PublishedByUserId);
        Assert.False(document.HasDraft);
        Assert.True(document.HasEverBeenPublished);
    }

    [Fact]
    public void PublishDraft_SupersedesThePreviouslyPublishedVersion()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>v1</p>", UserId, Now);
        document.PublishDraft(Tr, null, UserId, Now);

        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>v2</p>", UserId, Now);
        var result = document.PublishDraft(Tr, null, UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        var v1 = document.Versions.Single(v => v.VersionNumber == 1);
        var v2 = document.Versions.Single(v => v.VersionNumber == 2);
        Assert.Equal(LegalDocumentVersionStatus.Superseded, v1.Status);
        Assert.Equal(LegalDocumentVersionStatus.Published, v2.Status);
    }

    // ADR-024 §12.1: "Gelecek tarihli EffectiveAtUtc ile yayınlanan sürüm, o tarihe kadar yürürlükte
    // olan sürümün yerini almaz" - fixed clock, exercised through LegalDocumentEffectiveVersionResolver
    // exactly the way the public query handlers call it.
    [Fact]
    public void PublishDraft_WithFutureEffectiveDate_DoesNotReplaceTheCurrentlyEffectiveVersionYet()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>v1</p>", UserId, Now);
        document.PublishDraft(Tr, null, UserId, Now);

        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>v2</p>", UserId, Now);
        var futureEffectiveDate = Now.AddDays(7);
        document.PublishDraft(Tr, futureEffectiveDate, UserId, Now);

        var effectiveBeforeFutureDate = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, Now);
        Assert.NotNull(effectiveBeforeFutureDate);
        Assert.Equal(1, effectiveBeforeFutureDate!.VersionNumber);

        var effectiveAfterFutureDate = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, futureEffectiveDate);
        Assert.NotNull(effectiveAfterFutureDate);
        Assert.Equal(2, effectiveAfterFutureDate!.VersionNumber);
    }

    [Fact]
    public void EffectiveVersionResolver_WithNoVersions_ReturnsNull()
    {
        var document = CreateDocument();

        var effective = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, Now);

        Assert.Null(effective);
    }

    [Fact]
    public void EffectiveVersionResolver_WithOnlyADraft_ReturnsNull()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var effective = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, Now);

        Assert.Null(effective);
    }

    [Fact]
    public void DeleteDraft_RemovesTheDraftVersion()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);

        var result = document.DeleteDraft(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(document.HasDraft);
        Assert.Empty(document.Versions);
    }

    [Fact]
    public void DeleteDraft_WhenNoDraft_Fails()
    {
        var document = CreateDocument();

        var result = document.DeleteDraft(UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("LegalDocument.DraftNotFound", result.Error.Code);
    }

    [Fact]
    public void DeleteDraft_AfterAPublishedVersion_LeavesThePublishedVersionIntact()
    {
        var document = CreateDocument();
        document.CreateDraft(null, UserId, Now);
        document.UpdateDraftBody(Tr, "<p>v1</p>", UserId, Now);
        document.PublishDraft(Tr, null, UserId, Now);
        document.CreateDraft(null, UserId, Now);

        var result = document.DeleteDraft(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(document.Versions);
        Assert.Equal(LegalDocumentVersionStatus.Published, document.Versions[0].Status);
    }

    [Fact]
    public void Touch_RegeneratesRowVersion()
    {
        var document = CreateDocument();
        var originalRowVersion = document.RowVersion;

        document.SetTranslation(Tr, "Yeni Başlık", UserId, Now);

        Assert.NotEqual(originalRowVersion, document.RowVersion);
    }
}
