using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateReference;

public sealed record UpdateCandidateReferenceCommand(
    Guid CandidateCvId,
    Guid CandidateReferenceId,
    Guid ReferenceTypeId,
    Guid ReferenceLanguageId,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber) : IRequest<Result>;
