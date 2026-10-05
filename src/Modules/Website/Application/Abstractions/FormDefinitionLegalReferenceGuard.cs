using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.1/§12.2 (Faz 3 Görev 3): a FormDefinition's PrivacyNoticeKey must reference an existing
// LegalDocument of Kind PrivacyNotice, and each ExplicitConsents entry an existing LegalDocument of Kind
// ExplicitConsent ("Aydınlatma ile açık rıza aynı onay kutusunda birleştirilmez") - checked here once,
// called by both CreateFormDefinition and UpdateFormDefinition instead of duplicating the same lookup
// and Kind check in both handlers (mirrors MediaImageReferenceGuard's shape).
public static class FormDefinitionLegalReferenceGuard
{
    public static async Task<Result<LegalDocumentKey>> ResolvePrivacyNoticeKeyAsync(
        string? privacyNoticeKey, ILegalDocumentRepository legalDocumentRepository, CancellationToken cancellationToken)
    {
        var keyResult = LegalDocumentKey.Create(privacyNoticeKey);
        if (keyResult.IsFailure)
        {
            return Result.Failure<LegalDocumentKey>(keyResult.Error);
        }

        var document = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (document is null)
        {
            return Result.Failure<LegalDocumentKey>(Error.NotFound(
                "LegalDocument.NotFound", $"Legal document '{keyResult.Value}' could not be found."));
        }

        if (document.Kind != LegalDocumentKind.PrivacyNotice)
        {
            return Result.Failure<LegalDocumentKey>(Error.Validation(
                "FormDefinition.PrivacyNoticeKindInvalid", "The privacy notice document must have kind 'PrivacyNotice'."));
        }

        return Result.Success(keyResult.Value);
    }

    public static async Task<Result<IReadOnlyList<FormExplicitConsentRequirement>>> ResolveExplicitConsentsAsync(
        IReadOnlyList<(string? LegalDocumentKey, bool IsRequired)> inputs,
        ILegalDocumentRepository legalDocumentRepository,
        CancellationToken cancellationToken)
    {
        var consents = new List<FormExplicitConsentRequirement>();

        foreach (var input in inputs)
        {
            var keyResult = LegalDocumentKey.Create(input.LegalDocumentKey);
            if (keyResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<FormExplicitConsentRequirement>>(keyResult.Error);
            }

            var document = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
            if (document is null)
            {
                return Result.Failure<IReadOnlyList<FormExplicitConsentRequirement>>(Error.NotFound(
                    "LegalDocument.NotFound", $"Legal document '{keyResult.Value}' could not be found."));
            }

            if (document.Kind != LegalDocumentKind.ExplicitConsent)
            {
                return Result.Failure<IReadOnlyList<FormExplicitConsentRequirement>>(Error.Validation(
                    "FormDefinition.ExplicitConsentKindInvalid", "An explicit consent document must have kind 'ExplicitConsent'."));
            }

            consents.Add(FormExplicitConsentRequirement.Create(keyResult.Value, input.IsRequired));
        }

        return Result.Success<IReadOnlyList<FormExplicitConsentRequirement>>(consents);
    }
}
