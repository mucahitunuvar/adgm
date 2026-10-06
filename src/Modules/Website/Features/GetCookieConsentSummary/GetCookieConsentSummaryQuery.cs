using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetCookieConsentSummary;

public sealed record GetCookieConsentSummaryQuery(DateTime? From, DateTime? To) : IRequest<Result<CookieConsentSummaryResponse>>;
