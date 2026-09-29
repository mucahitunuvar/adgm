using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteTag;

// ADR-024 §4.1 (Faz 1b Görev 3): unlike DeleteContentCategory (blocked while in use), deleting a tag
// removes it from every content item that references it - tags are a lightweight, freely-editable
// taxonomy, not a structural relationship worth protecting.
public sealed class DeleteTagCommandHandler(
    IContentTagRepository contentTagRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTagCommand, Result>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await contentTagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(Error.NotFound("ContentTag.NotFound", $"Tag '{request.Id}' could not be found."));
        }

        var affectedItems = await contentItemRepository.GetByTagIdAsync(tag.Id, cancellationToken);
        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var item in affectedItems)
        {
            foreach (var translation in item.Translations.Where(t => t.TagIds.Contains(tag.Id)))
            {
                var remainingTagIds = translation.TagIds.Where(id => id != tag.Id).ToList();
                var setResult = item.SetTranslationTags(translation.LanguageCode, remainingTagIds, userId, now);
                if (setResult.IsFailure)
                {
                    return setResult;
                }
            }
        }

        contentTagRepository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
