using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.Forms;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

// ADR-024 §12.2 (Faz 3 Görev 4). Not excluded from the module-wide cache-invalidation convention by
// accident: a submission never changes any publicly cached response (the form/content it was submitted
// against are unaffected), so this handler is listed in PublicContentCacheInvalidationTests'
// ExcludedHandlers rather than calling WebsiteCacheInvalidator.
public sealed partial class SubmitFormSubmissionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ILegalDocumentRepository legalDocumentRepository,
    IFormSubmissionRepository formSubmissionRepository,
    IFormSubmissionSequenceRepository formSubmissionSequenceRepository,
    IContentItemRepository contentItemRepository,
    ContentPathCascadeService contentPathCascadeService,
    ISiteLanguageRepository siteLanguageRepository,
    ISiteSettingsRepository siteSettingsRepository,
    IPublicSubmissionGuard publicSubmissionGuard,
    IFileStorageService fileStorageService,
    IWebsiteEmailSender websiteEmailSender,
    ICurrentUserContext currentUserContext,
    IConfiguration configuration,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitFormSubmissionCommand, Result<SubmitFormSubmissionResponse>>
{
    private const int MaxReservationAttempts = 20;
    private const string OwnerEntityType = "FormSubmission";

    private static readonly Error NotFoundError = Error.NotFound("FormDefinition.NotFound", "This form could not be found.");

    public async Task<Result<SubmitFormSubmissionResponse>> Handle(SubmitFormSubmissionCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await publicSubmissionGuard.VerifyAsync(
            new PublicSubmissionGuardRequest(request.SubmissionToken, request.TurnstileToken, request.Website), request.RemoteIpAddress,
            cancellationToken);
        if (guardResult.IsFailure)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(guardResult.Error);
        }

        var keyResult = FormDefinitionKey.Create(request.FormKey);
        if (keyResult.IsFailure)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(NotFoundError);
        }

        var form = await formDefinitionRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (form is null || !form.IsActive)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var translation = form.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        if (translation is null)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(NotFoundError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (request.ContentItemId is not null)
        {
            var contentItemCheck = await ValidateContentItemAsync(request.ContentItemId.Value, form.Id, now, cancellationToken);
            if (contentItemCheck.IsFailure)
            {
                return Result.Failure<SubmitFormSubmissionResponse>(contentItemCheck.Error);
            }
        }

        var legalResult = await ValidateLegalConsentsAsync(form, request.AcceptedPrivacyNoticeVersion, request.ExplicitConsents, now, cancellationToken);
        if (legalResult.IsFailure)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(legalResult.Error);
        }

        var fieldsResult = ValidateAnswers(form, request.Answers, request.Files);
        if (fieldsResult.IsFailure)
        {
            return Result.Failure<SubmitFormSubmissionResponse>(fieldsResult.Error);
        }

        var (responses, firstEmailValue) = fieldsResult.Value;

        var submissionId = Guid.NewGuid();
        var uploadedFiles = new List<(string FieldKey, FileAttachment File)>();

        foreach (var field in form.Fields.Where(f => f.Type == FormFieldType.File))
        {
            var fileInput = request.Files.FirstOrDefault(f => f.FieldKey == field.Key);
            if (fileInput is null)
            {
                continue;
            }

            var uploadResult = await UploadFieldFileAsync(field, fileInput, submissionId, cancellationToken);
            if (uploadResult.IsFailure)
            {
                await DeleteUploadedFilesAsync(uploadedFiles, cancellationToken);
                return Result.Failure<SubmitFormSubmissionResponse>(uploadResult.Error);
            }

            uploadedFiles.Add((field.Key, uploadResult.Value));
        }

        var siteSettings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var referenceNumberResult = await ReserveReferenceNumberAsync(now.Year, siteSettings.SubmissionReferencePrefix, cancellationToken);
        if (referenceNumberResult.IsFailure)
        {
            await DeleteUploadedFilesAsync(uploadedFiles, cancellationToken);
            return Result.Failure<SubmitFormSubmissionResponse>(referenceNumberResult.Error);
        }

        var referenceNumber = referenceNumberResult.Value;
        var fileAttachmentEntities = uploadedFiles.Select(f => FormSubmissionFileAttachment.Create(f.FieldKey, f.File)).ToList();
        var responsesJson = JsonSerializer.Serialize(responses);
        var snapshotJson = BuildFieldSnapshotJson(form, resolvedLanguage.Code);

        var createResult = FormSubmission.Create(
            submissionId, form.Id, form.DefinitionVersion, snapshotJson, referenceNumber, resolvedLanguage.Code, now,
            currentUserContext.UserId, request.ContentItemId, responsesJson, fileAttachmentEntities, legalResult.Value);
        if (createResult.IsFailure)
        {
            await DeleteUploadedFilesAsync(uploadedFiles, cancellationToken);
            return Result.Failure<SubmitFormSubmissionResponse>(createResult.Error);
        }

        formSubmissionRepository.Add(createResult.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await DeleteUploadedFilesAsync(uploadedFiles, cancellationToken);
            return Result.Failure<SubmitFormSubmissionResponse>(Error.Failure(
                "FormSubmission.SaveFailed", "The submission could not be saved. Please try again."));
        }

        await SendEmailsAsync(form, translation, firstEmailValue, referenceNumber, cancellationToken);

        return Result.Success(new SubmitFormSubmissionResponse(referenceNumber, translation.SuccessMessage));
    }

    private async Task<Result> ValidateContentItemAsync(Guid contentItemId, Guid formDefinitionId, DateTime now, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(contentItemId, cancellationToken);
        if (contentItem is null
            || contentItem.FormDefinitionId != formDefinitionId
            || !await contentPathCascadeService.IsVisibleWithAncestorsAsync(contentItem, now, cancellationToken))
        {
            return Result.Failure(Error.Validation(
                "FormSubmission.ContentItemInvalid", "The linked content item could not be found, is not visible, or is not linked to this form."));
        }

        return Result.Success();
    }

    private async Task<Result<IReadOnlyList<FormSubmissionAcceptedLegalVersion>>> ValidateLegalConsentsAsync(
        FormDefinition form,
        int? acceptedPrivacyNoticeVersion,
        IReadOnlyList<SubmitFormSubmissionExplicitConsentInput> explicitConsents,
        DateTime now,
        CancellationToken cancellationToken)
    {
        static Result<IReadOnlyList<FormSubmissionAcceptedLegalVersion>> VersionChanged() =>
            Result.Failure<IReadOnlyList<FormSubmissionAcceptedLegalVersion>>(Error.Conflict(
                "PublicSubmission.LegalVersionChanged",
                "The legal document you accepted has since changed. Please review and accept the current version."));

        var accepted = new List<FormSubmissionAcceptedLegalVersion>();

        var privacyDocument = await legalDocumentRepository.GetByKeyAsync(form.PrivacyNoticeKey, cancellationToken);
        var privacyEffective = privacyDocument is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(privacyDocument.Versions, now);
        if (privacyEffective is null || acceptedPrivacyNoticeVersion != privacyEffective.VersionNumber)
        {
            return VersionChanged();
        }

        accepted.Add(FormSubmissionAcceptedLegalVersion.Create(form.PrivacyNoticeKey, privacyEffective.VersionNumber, isPrivacyNotice: true));

        foreach (var requirement in form.ExplicitConsents)
        {
            var submitted = explicitConsents.FirstOrDefault(
                c => string.Equals((c.Key ?? string.Empty).Trim(), requirement.LegalDocumentKey.Value, StringComparison.OrdinalIgnoreCase));

            if (submitted is null || !submitted.Accepted)
            {
                if (requirement.IsRequired)
                {
                    return Result.Failure<IReadOnlyList<FormSubmissionAcceptedLegalVersion>>(Error.Validation(
                        "FormSubmission.RequiredConsentNotAccepted",
                        $"The required consent '{requirement.LegalDocumentKey.Value}' must be accepted."));
                }

                continue;
            }

            var document = await legalDocumentRepository.GetByKeyAsync(requirement.LegalDocumentKey, cancellationToken);
            var effective = document is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
            if (effective is null || submitted.Version != effective.VersionNumber)
            {
                return VersionChanged();
            }

            accepted.Add(FormSubmissionAcceptedLegalVersion.Create(requirement.LegalDocumentKey, effective.VersionNumber, isPrivacyNotice: false));
        }

        return Result.Success<IReadOnlyList<FormSubmissionAcceptedLegalVersion>>(accepted);
    }

    private static Result<(Dictionary<string, object> Responses, string? FirstEmailValue)> ValidateAnswers(
        FormDefinition form, IReadOnlyDictionary<string, IReadOnlyList<string>> answers, IReadOnlyList<SubmitFormSubmissionFileInput> files)
    {
        var knownKeys = form.Fields.Select(f => f.Key).ToHashSet();

        var unknownAnswerKey = answers.Keys.FirstOrDefault(k => !knownKeys.Contains(k));
        if (unknownAnswerKey is not null)
        {
            return UnknownFieldFailure(unknownAnswerKey);
        }

        if (files.GroupBy(f => f.FieldKey).Any(g => g.Count() > 1))
        {
            return Result.Failure<(Dictionary<string, object>, string?)>(Error.Validation(
                "FormSubmission.MultipleFilesForField", "At most one file can be uploaded per field."));
        }

        var unknownFileKey = files.Select(f => f.FieldKey).FirstOrDefault(k => !knownKeys.Contains(k));
        if (unknownFileKey is not null)
        {
            return UnknownFieldFailure(unknownFileKey);
        }

        var responses = new Dictionary<string, object>();
        string? firstEmailValue = null;

        foreach (var field in form.Fields.OrderBy(f => f.SortOrder))
        {
            if (field.Type == FormFieldType.File)
            {
                var hasFile = files.Any(f => f.FieldKey == field.Key);
                if (field.IsRequired && !hasFile)
                {
                    return RequiredFailure(field);
                }

                continue;
            }

            answers.TryGetValue(field.Key, out var rawValues);
            var values = (rawValues ?? []).Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
            var hasValue = values.Count > 0;

            if (!hasValue)
            {
                if (field.IsRequired)
                {
                    return RequiredFailure(field);
                }

                continue;
            }

            var normalizeResult = NormalizeAnswer(field, values);
            if (normalizeResult.IsFailure)
            {
                return Result.Failure<(Dictionary<string, object>, string?)>(normalizeResult.Error);
            }

            responses[field.Key] = normalizeResult.Value;

            if (field.Type == FormFieldType.Email && firstEmailValue is null)
            {
                firstEmailValue = (string)normalizeResult.Value;
            }
        }

        return Result.Success((responses, firstEmailValue));
    }

    private static Result<object> NormalizeAnswer(FormField field, IReadOnlyList<string> values)
    {
        switch (field.Type)
        {
            case FormFieldType.Text:
            case FormFieldType.Textarea:
            {
                var text = values[0];
                if (field.MinLength is not null && text.Length < field.MinLength)
                {
                    return LengthFailure(field);
                }

                if (field.MaxLength is not null && text.Length > field.MaxLength)
                {
                    return LengthFailure(field);
                }

                return Result.Success<object>(text);
            }

            case FormFieldType.Email:
            {
                var email = values[0];
                return EmailPattern().IsMatch(email)
                    ? Result.Success<object>(email)
                    : FormatFailure(field, "email");
            }

            case FormFieldType.Phone:
            {
                var phone = values[0];
                return PhonePattern().IsMatch(phone)
                    ? Result.Success<object>(phone)
                    : FormatFailure(field, "phone");
            }

            case FormFieldType.Select:
            {
                var selected = values[0];
                return field.Options.Any(o => o.Key == selected)
                    ? Result.Success<object>(selected)
                    : OptionFailure(field);
            }

            case FormFieldType.MultiSelect:
            {
                var selected = values.Distinct().ToList();
                return selected.Any(v => field.Options.All(o => o.Key != v))
                    ? OptionFailure(field)
                    : Result.Success<object>(selected);
            }

            case FormFieldType.Checkbox:
            {
                if (!bool.TryParse(values[0], out var isChecked))
                {
                    return FormatFailure(field, "boolean");
                }

                if (field.IsRequired && !isChecked)
                {
                    return Result.Failure<object>(Error.Validation("FormSubmission.FieldRequired", $"Field '{field.Key}' is required."));
                }

                return Result.Success<object>(isChecked);
            }

            case FormFieldType.Date:
            {
                if (!DateTime.TryParse(
                        values[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out var date))
                {
                    return FormatFailure(field, "date");
                }

                if (field.DateMin is not null && date < field.DateMin)
                {
                    return RangeFailure(field);
                }

                if (field.DateMax is not null && date > field.DateMax)
                {
                    return RangeFailure(field);
                }

                return Result.Success<object>(date.ToString("O"));
            }

            default:
                return FormatFailure(field, "value");
        }
    }

    private async Task<Result<FileAttachment>> UploadFieldFileAsync(
        FormField field, SubmitFormSubmissionFileInput fileInput, Guid submissionId, CancellationToken cancellationToken)
    {
        await using var contentStream = fileInput.Content;
        using var buffer = new MemoryStream();
        await contentStream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var policy = BuildFilePolicy(field);
        var policyResult = policy.Validate(fileInput.FileName, fileInput.ContentType, bytes.LongLength);
        if (policyResult.IsFailure)
        {
            return Result.Failure<FileAttachment>(policyResult.Error);
        }

        if (!field.AllowedFileTypes.Any(t => FormFieldAllowedFileTypeMapper.HasValidSignature(t, bytes)))
        {
            return Result.Failure<FileAttachment>(Error.Validation(
                "FormSubmission.InvalidFileSignature", $"The uploaded file for field '{field.Key}' does not match an allowed file type."));
        }

        using var uploadStream = new MemoryStream(bytes);
        return await fileStorageService.UploadAsync(
            uploadStream, fileInput.FileName, fileInput.ContentType, FileCategory.WebsiteFormAttachment, OwnerEntityType, submissionId, policy,
            cancellationToken);
    }

    private static FileValidationPolicy BuildFilePolicy(FormField field)
    {
        var extensions = new List<string>();
        var contentTypes = new List<string>();

        foreach (var allowedType in field.AllowedFileTypes)
        {
            extensions.Add(FormFieldAllowedFileTypeMapper.Extension(allowedType));
            contentTypes.Add(FormFieldAllowedFileTypeMapper.ContentType(allowedType));
            if (allowedType == FormFieldAllowedFileType.Jpg)
            {
                extensions.Add("jpeg");
            }
        }

        var maxSizeMb = field.MaxSizeMb ?? FormField.MaxFileSizeMb;
        return FileValidationPolicy.Create(extensions, contentTypes, maxSizeMb * 1024L * 1024L);
    }

    private async Task DeleteUploadedFilesAsync(IReadOnlyList<(string FieldKey, FileAttachment File)> uploadedFiles, CancellationToken cancellationToken)
    {
        foreach (var (_, file) in uploadedFiles)
        {
            await fileStorageService.DeleteAsync(file.FileKey, cancellationToken);
        }
    }

    // §12.2 "Sıra yıl bazında, eşzamanlı gönderimlerde çakışmayacak şekilde bir sayaç tablosuyla
    // (iyimser eşzamanlılık + yeniden deneme) üretilir": FormSubmissionSequence is a simple entity with
    // no owned collections, so detach-and-retry on a concurrency conflict is safe here, the same shape
    // RecordNotFoundPathCommandHandler already uses for its own unique-constraint race.
    private async Task<Result<string>> ReserveReferenceNumberAsync(int year, string prefix, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxReservationAttempts; attempt++)
        {
            var sequence = await formSubmissionSequenceRepository.GetByYearAsync(year, cancellationToken);
            var isNew = sequence is null;
            sequence ??= FormSubmissionSequence.CreateForYear(year);
            var reservedValue = sequence.Reserve();

            if (isNew)
            {
                formSubmissionSequenceRepository.Add(sequence);
            }

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success($"{prefix}-{year}-{reservedValue:D6}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                formSubmissionSequenceRepository.DetachFailedReservation(sequence);
            }
        }

        return Result.Failure<string>(Error.Failure(
            "FormSubmission.ReferenceNumberGenerationFailed", "Could not generate a unique reference number. Please try again."));
    }

    private static string BuildFieldSnapshotJson(FormDefinition form, LanguageCode languageCode)
    {
        var snapshot = form.Fields
            .OrderBy(f => f.SortOrder)
            .Select(f =>
            {
                var translation = f.Translations.FirstOrDefault(t => t.LanguageCode == languageCode) ?? f.Translations.FirstOrDefault();
                var label = translation?.Label ?? f.Key;

                IReadOnlyList<FormSubmissionFieldSnapshotOption>? options = f.Options.Count == 0
                    ? null
                    : f.Options
                        .Select(o =>
                        {
                            var optionTranslation = o.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)
                                ?? o.Translations.FirstOrDefault();
                            return new FormSubmissionFieldSnapshotOption(o.Key, optionTranslation?.Label ?? o.Key);
                        })
                        .ToList();

                return new FormSubmissionFieldSnapshot(f.Key, f.Type.ToString(), label, options);
            })
            .ToList();

        return JsonSerializer.Serialize(snapshot);
    }

    // §12.2 "Başvurana ... referans numarası, form adı ve başarı mesajı, dile göre. Yanıtların
    // kendisi e-postaya konmaz." / "Formun NotificationEmails adreslerine: form adı, referans numarası
    // ve admin panelindeki başvuru detayına link. Başvurunun içeriği ve başvuranın bilgileri e-postada
    // yer almaz." - IWebsiteEmailSender.SendEmailAsync already swallows/records its own failures
    // (NotificationModuleContract.SendEmailAsync's try/catch), so no try/catch is needed here, the
    // same direct post-commit call shape AddCandidateNoteCommandHandler/SubmitPersonnelNeedCommandHandler
    // already use for their own best-effort notifications (ADR-024 §11).
    private async Task SendEmailsAsync(
        FormDefinition form, FormDefinitionTranslation translation, string? submitterEmail, string referenceNumber,
        CancellationToken cancellationToken)
    {
        if (submitterEmail is not null)
        {
            var subject = $"{translation.Title} - {referenceNumber}";
            var body = $"<p>{System.Net.WebUtility.HtmlEncode(translation.SuccessMessage)}</p>"
                + $"<p>Reference number: <strong>{System.Net.WebUtility.HtmlEncode(referenceNumber)}</strong></p>";
            await websiteEmailSender.SendEmailAsync(submitterEmail, subject, body, cancellationToken);
        }

        if (form.NotificationEmails.Count == 0)
        {
            return;
        }

        var adminPanelBaseUrl = (configuration["Website:AdminPanelBaseUrl"] ?? string.Empty).TrimEnd('/');
        var detailUrl = $"{adminPanelBaseUrl}/forms/{form.Id}/submissions?reference={Uri.EscapeDataString(referenceNumber)}";
        var notificationSubject = $"New submission: {translation.Title} ({referenceNumber})";
        var notificationBody = $"<p>A new submission was received for \"{System.Net.WebUtility.HtmlEncode(translation.Title)}\".</p>"
            + $"<p>Reference number: <strong>{System.Net.WebUtility.HtmlEncode(referenceNumber)}</strong></p>"
            + $"<p><a href=\"{detailUrl}\">View in admin panel</a></p>";

        foreach (var notificationEmail in form.NotificationEmails)
        {
            await websiteEmailSender.SendEmailAsync(notificationEmail, notificationSubject, notificationBody, cancellationToken);
        }
    }

    private static Result<(Dictionary<string, object>, string?)> UnknownFieldFailure(string fieldKey) =>
        Result.Failure<(Dictionary<string, object>, string?)>(Error.Validation(
            "FormSubmission.UnknownField", $"Unknown field '{fieldKey}'."));

    private static Result<(Dictionary<string, object>, string?)> RequiredFailure(FormField field) =>
        Result.Failure<(Dictionary<string, object>, string?)>(Error.Validation(
            "FormSubmission.FieldRequired", $"Field '{field.Key}' is required."));

    private static Result<object> LengthFailure(FormField field) =>
        Result.Failure<object>(Error.Validation("FormSubmission.FieldLengthInvalid", $"Field '{field.Key}' has an invalid length."));

    private static Result<object> FormatFailure(FormField field, string expectedFormat) =>
        Result.Failure<object>(Error.Validation(
            "FormSubmission.FieldFormatInvalid", $"Field '{field.Key}' must be a valid {expectedFormat}."));

    private static Result<object> OptionFailure(FormField field) =>
        Result.Failure<object>(Error.Validation(
            "FormSubmission.FieldOptionInvalid", $"Field '{field.Key}' contains an option that is not allowed."));

    private static Result<object> RangeFailure(FormField field) =>
        Result.Failure<object>(Error.Validation("FormSubmission.FieldRangeInvalid", $"Field '{field.Key}' is outside the allowed range."));

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"^[0-9+()\-\s]{7,20}$")]
    private static partial Regex PhonePattern();
}
