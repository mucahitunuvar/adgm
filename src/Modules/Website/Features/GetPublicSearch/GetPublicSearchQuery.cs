using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSearch;

public sealed record GetPublicSearchQuery(
    string Q,
    string? Lang,
    string? Type,
    string? Source) : PagedRequest, IRequest<Result<PublicSearchResponse>>;
