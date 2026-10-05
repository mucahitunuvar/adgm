using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;

public sealed class CreateLegalDocumentCommandHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLegalDocumentCommand, Result<CreateLegalDocumentResponse>>
{
    public async Task<Result<CreateLegalDocumentResponse>> Handle(CreateLegalDocumentCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateLegalDocumentResponse>(
                Error.Failure("LegalDocument.NoDefaultLanguage", "No default site language is configured."));
        }

        if (!Enum.TryParse<LegalDocumentKind>(request.Kind, ignoreCase: true, out var kind))
        {
            return Result.Failure<CreateLegalDocumentResponse>(Error.Validation(
                "LegalDocument.InvalidKind", $"'{request.Kind}' is not a recognized legal document kind."));
        }

        var keyResult = LegalDocumentKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<CreateLegalDocumentResponse>(keyResult.Error);
        }

        var existing = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CreateLegalDocumentResponse>(Error.Conflict(
                "LegalDocument.KeyAlreadyExists", $"A legal document with key '{keyResult.Value}' already exists."));
        }

        var documentResult = LegalDocument.Create(
            request.Key, kind, defaultLanguage.Code, request.DefaultLanguageTitle,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (documentResult.IsFailure)
        {
            return Result.Failure<CreateLegalDocumentResponse>(documentResult.Error);
        }

        legalDocumentRepository.Add(documentResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreateLegalDocumentResponse(documentResult.Value.Id, defaultLanguage.Code.Value));
    }
}
