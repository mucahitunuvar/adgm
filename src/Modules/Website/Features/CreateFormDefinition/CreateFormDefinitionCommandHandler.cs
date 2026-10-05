using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;

public sealed class CreateFormDefinitionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFormDefinitionCommand, Result<CreateFormDefinitionResponse>>
{
    public async Task<Result<CreateFormDefinitionResponse>> Handle(CreateFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateFormDefinitionResponse>(
                Error.Failure("FormDefinition.NoDefaultLanguage", "No default site language is configured."));
        }

        var keyResult = FormDefinitionKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<CreateFormDefinitionResponse>(keyResult.Error);
        }

        var existing = await formDefinitionRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CreateFormDefinitionResponse>(Error.Conflict(
                "FormDefinition.KeyAlreadyExists", $"A form with key '{keyResult.Value}' already exists."));
        }

        var privacyNoticeResult = await FormDefinitionLegalReferenceGuard.ResolvePrivacyNoticeKeyAsync(
            request.PrivacyNoticeKey, legalDocumentRepository, cancellationToken);
        if (privacyNoticeResult.IsFailure)
        {
            return Result.Failure<CreateFormDefinitionResponse>(privacyNoticeResult.Error);
        }

        var explicitConsentsResult = await FormDefinitionLegalReferenceGuard.ResolveExplicitConsentsAsync(
            request.ExplicitConsents.Select(c => (c.LegalDocumentKey, c.IsRequired)).ToList(), legalDocumentRepository, cancellationToken);
        if (explicitConsentsResult.IsFailure)
        {
            return Result.Failure<CreateFormDefinitionResponse>(explicitConsentsResult.Error);
        }

        var sanitizedDescription = htmlContentSanitizer.Sanitize(request.DefaultLanguageDescription ?? string.Empty);
        var retentionDays = request.RetentionDays ?? FormDefinition.DefaultRetentionDays;

        var createResult = FormDefinition.Create(
            request.Key, retentionDays, request.NotificationEmails, privacyNoticeResult.Value, explicitConsentsResult.Value,
            defaultLanguage.Code, request.DefaultLanguageTitle, sanitizedDescription, request.DefaultLanguageSuccessMessage,
            request.DefaultLanguageSubmitButtonLabel, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (createResult.IsFailure)
        {
            return Result.Failure<CreateFormDefinitionResponse>(createResult.Error);
        }

        formDefinitionRepository.Add(createResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreateFormDefinitionResponse(createResult.Value.Id, defaultLanguage.Code.Value));
    }
}
