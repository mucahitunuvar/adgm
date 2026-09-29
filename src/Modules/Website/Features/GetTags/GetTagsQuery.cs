using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetTags;

public sealed record GetTagsQuery(string? LanguageCode, string? Search) : PagedRequest, IRequest<Result<PagedResult<TagResponse>>>;
