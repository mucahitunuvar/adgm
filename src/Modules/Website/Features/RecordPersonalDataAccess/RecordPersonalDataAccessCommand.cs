using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;

public sealed record RecordPersonalDataAccessCommand(
    PersonalDataEntityType EntityType, Guid? EntityId, PersonalDataAccessAction Action, string? Detail) : IRequest<Result>;
