using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPopups;

public sealed class GetPopupsQueryHandler(IPopupRepository popupRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetPopupsQuery, Result<PagedResult<PopupSummaryResponse>>>
{
    public async Task<Result<PagedResult<PopupSummaryResponse>>> Handle(GetPopupsQuery request, CancellationToken cancellationToken)
    {
        PopupDisplayMode? displayMode = null;
        if (!string.IsNullOrWhiteSpace(request.DisplayMode))
        {
            if (!Enum.TryParse<PopupDisplayMode>(request.DisplayMode, ignoreCase: true, out var parsed))
            {
                return Result.Failure<PagedResult<PopupSummaryResponse>>(
                    Error.Validation("Popup.DisplayModeInvalid", $"Unknown display mode '{request.DisplayMode}'."));
            }

            displayMode = parsed;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<PagedResult<PopupSummaryResponse>>(
                Error.Failure("Popup.NoDefaultLanguage", "No default site language is configured."));
        }

        var paged = await popupRepository.SearchAsync(displayMode, request.IsActive, request, cancellationToken);

        var items = paged.Items
            .Select(p => ToSummary(p, defaultLanguage.Code))
            .ToList();

        return Result.Success(new PagedResult<PopupSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private static PopupSummaryResponse ToSummary(Popup popup, LanguageCode defaultLanguageCode)
    {
        var title = popup.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Title;
        return new PopupSummaryResponse(
            popup.Id, popup.DisplayMode.ToString(), title, popup.IsActive, popup.Priority, popup.PublishAtUtc, popup.UnpublishAtUtc,
            popup.RowVersion);
    }
}
