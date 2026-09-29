using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateTag;

public sealed class UpdateTagCommandHandler(
    IContentTagRepository contentTagRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTagCommand, Result>
{
    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await contentTagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(Error.NotFound("ContentTag.NotFound", $"Tag '{request.Id}' could not be found."));
        }

        var renameResult = tag.Rename(request.Name, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (renameResult.IsFailure)
        {
            return renameResult;
        }

        var collision = await contentTagRepository.GetBySlugAsync(tag.LanguageCode, tag.Slug, cancellationToken);
        if (collision is not null && collision.Id != tag.Id)
        {
            return Result.Failure(Error.Conflict(
                "ContentTag.SlugAlreadyExists", $"'{tag.Slug}' is already used by another tag in language '{tag.LanguageCode}'."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
