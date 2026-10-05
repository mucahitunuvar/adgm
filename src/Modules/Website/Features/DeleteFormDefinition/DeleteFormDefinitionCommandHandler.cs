using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteFormDefinition;

// ADR-024 §12.2 "başvurusu olan form silinemez, yalnızca pasife alınır" - mirrors
// DeleteLegalDocumentCommandHandler's usage-checker guard shape.
public sealed class DeleteFormDefinitionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    IFormDefinitionUsageChecker formDefinitionUsageChecker,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteFormDefinitionCommand, Result>
{
    public async Task<Result> Handle(DeleteFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        var usages = await formDefinitionUsageChecker.GetUsagesAsync(form.Id, cancellationToken);
        if (usages.Count > 0)
        {
            var usageDescriptions = string.Join(", ", usages.Select(u => u.Description));
            return Result.Failure(Error.Conflict(
                "FormDefinition.InUse", $"This form is in use and cannot be deleted: {usageDescriptions}."));
        }

        formDefinitionRepository.Remove(form);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
