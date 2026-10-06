using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;

public sealed record GetEventRegistrationByIdQuery(Guid ContentItemId, Guid Id) : IRequest<Result<EventRegistrationDetailResponse>>;
