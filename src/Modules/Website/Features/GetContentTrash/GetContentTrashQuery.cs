using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTrash;

public sealed record GetContentTrashQuery(string? LanguageCode) : PagedRequest, IRequest<Result<PagedResult<ContentItemTrashSummaryResponse>>>;
