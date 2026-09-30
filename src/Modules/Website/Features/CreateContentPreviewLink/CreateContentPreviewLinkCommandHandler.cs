using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentPreviewLink;

// ADR-024 §4.5 (Faz 1b Görev 6): the token itself carries everything GetContentPreviewQuery needs
// (content item id, language) - no database row is created, so there is nothing to revoke early
// short of rotating the Data Protection key (out of scope here).
public sealed class CreateContentPreviewLinkCommandHandler(
    IContentItemRepository contentItemRepository, IContentPreviewLinkGenerator previewLinkGenerator, TimeProvider timeProvider)
    : IRequestHandler<CreateContentPreviewLinkCommand, Result<CreateContentPreviewLinkResponse>>
{
    public const int DefaultDurationHours = 72;
    public const int MaxDurationHours = 24 * 7;

    public async Task<Result<CreateContentPreviewLinkResponse>> Handle(CreateContentPreviewLinkCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<CreateContentPreviewLinkResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        string? languageCode = null;
        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<CreateContentPreviewLinkResponse>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value.Value;
        }

        var duration = TimeSpan.FromHours(request.DurationHours ?? DefaultDurationHours);
        var token = previewLinkGenerator.GenerateToken(contentItem.Id, languageCode, duration);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return Result.Success(new CreateContentPreviewLinkResponse(token, $"/api/v1/public/preview/{token}", now.Add(duration)));
    }
}
