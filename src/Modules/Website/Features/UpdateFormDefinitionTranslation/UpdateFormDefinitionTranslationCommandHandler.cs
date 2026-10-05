using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;

public sealed class UpdateFormDefinitionTranslationCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFormDefinitionTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdateFormDefinitionTranslationCommand request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(form.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.ConcurrencyConflict", "The form was changed by someone else. Reload and try again."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var sanitizedDescription = htmlContentSanitizer.Sanitize(request.Description ?? string.Empty);

        var setResult = form.SetTranslation(
            languageCodeResult.Value, request.Title, sanitizedDescription, request.SuccessMessage, request.SubmitButtonLabel,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
