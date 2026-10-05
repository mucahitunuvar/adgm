using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormDefinitionTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly LegalDocumentKey PrivacyNoticeKey = LegalDocumentKey.Create("kvkk-contact").Value;
    private static readonly LegalDocumentKey ExplicitConsentKey = LegalDocumentKey.Create("kvkk-marketing").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private static FormDefinition CreateForm(
        string key = "contact-form", int retentionDays = 730, IReadOnlyList<string>? notificationEmails = null,
        IReadOnlyList<FormExplicitConsentRequirement>? explicitConsents = null) =>
        FormDefinition.Create(
            key, retentionDays, notificationEmails ?? [], PrivacyNoticeKey, explicitConsents ?? [], Tr, "İletişim Formu", "Açıklama",
            "Teşekkürler", "Gönder", UserId, Now).Value;

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = FormDefinition.Create(
            "contact-form", 730, ["notify@example.com"], PrivacyNoticeKey,
            [FormExplicitConsentRequirement.Create(ExplicitConsentKey, true)], Tr, "İletişim Formu", "Açıklama", "Teşekkürler", "Gönder",
            UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("contact-form", result.Value.Key.Value);
        Assert.False(result.Value.IsActive);
        Assert.Equal(730, result.Value.RetentionDays);
        Assert.Single(result.Value.Translations);
        Assert.Empty(result.Value.Fields);
        Assert.Equal(0, result.Value.DefinitionVersion);
    }

    [Fact]
    public void Create_WithInvalidKey_Fails()
    {
        var result = FormDefinition.Create(
            "Invalid Key", 730, [], PrivacyNoticeKey, [], Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinitionKey.InvalidFormat", result.Error.Code);
    }

    [Theory]
    [InlineData(29)]
    [InlineData(3651)]
    public void Create_WithRetentionDaysOutOfRange_Fails(int retentionDays)
    {
        var result = FormDefinition.Create(
            "contact-form", retentionDays, [], PrivacyNoticeKey, [], Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.RetentionDaysInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooManyNotificationEmails_Fails()
    {
        var emails = Enumerable.Range(0, FormDefinition.MaxNotificationEmails + 1).Select(i => $"user{i}@example.com").ToList();

        var result = FormDefinition.Create(
            "contact-form", 730, emails, PrivacyNoticeKey, [], Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.TooManyNotificationEmails", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidNotificationEmail_Fails()
    {
        var result = FormDefinition.Create(
            "contact-form", 730, ["not-an-email"], PrivacyNoticeKey, [], Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.InvalidNotificationEmail", result.Error.Code);
    }

    [Fact]
    public void Create_WithTooManyExplicitConsents_Fails()
    {
        var consents = Enumerable.Range(0, FormDefinition.MaxExplicitConsents + 1)
            .Select(i => FormExplicitConsentRequirement.Create(LegalDocumentKey.Create($"consent-{i}").Value, true))
            .ToList();

        var result = FormDefinition.Create(
            "contact-form", 730, [], PrivacyNoticeKey, consents, Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.TooManyExplicitConsents", result.Error.Code);
    }

    [Fact]
    public void Create_WithDuplicateExplicitConsent_Fails()
    {
        var consents = new List<FormExplicitConsentRequirement>
        {
            FormExplicitConsentRequirement.Create(ExplicitConsentKey, true),
            FormExplicitConsentRequirement.Create(ExplicitConsentKey, false),
        };

        var result = FormDefinition.Create(
            "contact-form", 730, [], PrivacyNoticeKey, consents, Tr, "Title", "Desc", "Success", "Submit", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.DuplicateExplicitConsent", result.Error.Code);
    }

    [Fact]
    public void Update_WithValidInput_Succeeds()
    {
        var form = CreateForm();
        var newPrivacyNoticeKey = LegalDocumentKey.Create("kvkk-other").Value;

        var result = form.Update(365, ["notify@example.com"], newPrivacyNoticeKey, [], UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(365, form.RetentionDays);
        Assert.Equal(newPrivacyNoticeKey, form.PrivacyNoticeKey);
        Assert.Single(form.NotificationEmails);
    }

    [Fact]
    public void SetTranslation_AddsNewLanguageTranslation()
    {
        var form = CreateForm();

        var result = form.SetTranslation(En, "Contact Form", "Description", "Thanks", "Submit", UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, form.Translations.Count);
    }

    [Fact]
    public void SetTranslation_UpdatesExistingLanguageTranslation()
    {
        var form = CreateForm();

        var result = form.SetTranslation(Tr, "Yeni Başlık", "Açıklama", "Teşekkürler", "Gönder", UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Yeni Başlık", form.Translations.Single().Title);
    }

    [Fact]
    public void RemoveTranslation_DefaultLanguage_Fails()
    {
        var form = CreateForm();

        var result = form.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_NonDefaultLanguage_Succeeds()
    {
        var form = CreateForm();
        form.SetTranslation(En, "Contact Form", "Description", "Thanks", "Submit", UserId, Now);

        var result = form.RemoveTranslation(En, Tr, UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Single(form.Translations);
    }

    private static FormField CreateField(string key, int sortOrder = 0, FormFieldType type = FormFieldType.Text) =>
        FormField.Create(
            key, type, true, sortOrder, null, null, [], null, null, [], null,
            [FormFieldTranslation.Create(Tr, key, null, null).Value]).Value;

    [Fact]
    public void SetFields_WithValidFields_IncrementsDefinitionVersion()
    {
        var form = CreateForm();

        var result = form.SetFields([CreateField("full_name")], UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, form.DefinitionVersion);
        Assert.Single(form.Fields);
    }

    [Fact]
    public void SetFields_CalledTwice_IncrementsDefinitionVersionEachTime()
    {
        var form = CreateForm();
        form.SetFields([CreateField("full_name")], UserId, Now);

        var result = form.SetFields([CreateField("full_name"), CreateField("email")], UserId, Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, form.DefinitionVersion);
        Assert.Equal(2, form.Fields.Count);
    }

    [Fact]
    public void SetFields_ExceedingMaxFields_Fails()
    {
        var form = CreateForm();
        var fields = Enumerable.Range(0, FormDefinition.MaxFields + 1).Select(i => CreateField($"field_{i}", i)).ToList();

        var result = form.SetFields(fields, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.TooManyFields", result.Error.Code);
    }

    [Fact]
    public void SetFields_WithDuplicateKeys_Fails()
    {
        var form = CreateForm();

        var result = form.SetFields([CreateField("full_name"), CreateField("full_name")], UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.DuplicateFieldKey", result.Error.Code);
    }

    [Fact]
    public void SetFields_ExceedingMaxFileFields_Fails()
    {
        var form = CreateForm();
        var fileFields = Enumerable.Range(0, FormDefinition.MaxFileFields + 1)
            .Select(i => FormField.Create(
                    $"file_{i}", FormFieldType.File, true, i, null, null, [], null, null, [FormFieldAllowedFileType.Pdf], 5,
                    [FormFieldTranslation.Create(Tr, $"file_{i}", null, null).Value])
                .Value)
            .ToList();

        var result = form.SetFields(fileFields, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.TooManyFileFields", result.Error.Code);
    }

    [Fact]
    public void Activate_WithLegalRequirementsSatisfied_Succeeds()
    {
        var form = CreateForm();

        var result = form.Activate(legalRequirementsSatisfied: true, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(form.IsActive);
    }

    [Fact]
    public void Activate_WithoutLegalRequirementsSatisfied_Fails()
    {
        var form = CreateForm();

        var result = form.Activate(legalRequirementsSatisfied: false, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormDefinition.LegalRequirementsNotMet", result.Error.Code);
        Assert.False(form.IsActive);
    }

    [Fact]
    public void Deactivate_WhenActive_Succeeds()
    {
        var form = CreateForm();
        form.Activate(true, UserId, Now);

        var result = form.Deactivate(UserId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.False(form.IsActive);
    }

    [Fact]
    public void Touch_RegeneratesRowVersion()
    {
        var form = CreateForm();
        var originalRowVersion = form.RowVersion;

        form.SetTranslation(Tr, "Yeni", "Açıklama", "Teşekkürler", "Gönder", UserId, Now.AddDays(1));

        Assert.NotEqual(originalRowVersion, form.RowVersion);
    }
}
