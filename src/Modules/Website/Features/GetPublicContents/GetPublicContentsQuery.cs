using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

public sealed record GetPublicContentsQuery(
    string Type,
    string? Lang,
    string? Category,
    string? Tag,
    string? Search,
    DateTime? From,
    DateTime? To,
    bool? Featured) : PagedRequest, IRequest<Result<PublicContentListResponse>>;
