using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

public sealed record ExportEventRegistrationsQuery(Guid ContentItemId, EventRegistrationStatus? Status)
    : IRequest<Result<IReadOnlyList<EventRegistrationExportItem>>>;
