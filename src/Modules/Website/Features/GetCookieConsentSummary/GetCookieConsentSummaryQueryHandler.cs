using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetCookieConsentSummary;

public sealed class GetCookieConsentSummaryQueryHandler(ICookieConsentRecordRepository cookieConsentRecordRepository)
    : IRequestHandler<GetCookieConsentSummaryQuery, Result<CookieConsentSummaryResponse>>
{
    public async Task<Result<CookieConsentSummaryResponse>> Handle(GetCookieConsentSummaryQuery request, CancellationToken cancellationToken)
    {
        var items = await cookieConsentRecordRepository.GetForSummaryAsync(request.From, request.To, cancellationToken);

        var byCategory = items
            .SelectMany(i => i.Categories)
            .GroupBy(c => c.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var byAction = items
            .GroupBy(i => i.Action.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return Result.Success(new CookieConsentSummaryResponse(items.Count, byCategory, byAction));
    }
}
