using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

public sealed record GetEventRegistrationsQuery(
    Guid ContentItemId, string? Status, string? Search) : PagedRequest, IRequest<Result<GetEventRegistrationsResponse>>;
