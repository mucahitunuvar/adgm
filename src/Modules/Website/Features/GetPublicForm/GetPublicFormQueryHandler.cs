using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicForm;

// ADR-024 §12.2 (Faz 3 Görev 4): the active form's definition in the requested language. 404s
// whenever nothing complete can be shown - unknown key, inactive, no translation for that language, or
// (via PublicFormDefinitionResolver) the privacy notice/a required explicit consent has no version
// currently in force - never a partial response, same "never partial" rule GetPublicLegalDocument uses.
public sealed class GetPublicFormQueryHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ISiteLanguageRepository siteLanguageRepository,
    PublicFormDefinitionResolver publicFormDefinitionResolver,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicFormQuery, Result<PublicFormDefinitionResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound("FormDefinition.NotFound", "This form could not be found.");

    public async Task<Result<PublicFormDefinitionResponse>> Handle(GetPublicFormQuery request, CancellationToken cancellationToken)
    {
        var keyResult = FormDefinitionKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<PublicFormDefinitionResponse>(NotFoundError);
        }

        var form = await formDefinitionRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (form is null || !form.IsActive)
        {
            return Result.Failure<PublicFormDefinitionResponse>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cacheKey = WebsiteCacheKeys.PublicForm(form.Key.Value, resolvedLanguage.Code.Value);

        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            ct => publicFormDefinitionResolver.ResolveAsync(form, resolvedLanguage.Code, now, ct),
            ContentCacheTtlCalculator.DefaultTtl,
            cancellationToken);

        return response is null ? Result.Failure<PublicFormDefinitionResponse>(NotFoundError) : Result.Success(response);
    }
}
