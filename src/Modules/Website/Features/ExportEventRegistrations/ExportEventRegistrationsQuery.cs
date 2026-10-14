using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

public sealed record ExportEventRegistrationsQuery(Guid ContentItemId, string? Status)
    : IRequest<Result<IReadOnlyList<EventRegistrationExportItem>>>;
