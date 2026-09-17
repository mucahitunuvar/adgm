using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;

public sealed record AddCandidateReferenceCommand(
    Guid CandidateCvId,
    Guid ReferenceTypeId,
    Guid ReferenceLanguageId,
    string FirstName,
    string LastName,
    string? Company,
    string? Position,
    string? Email,
    string? PhoneNumber) : IRequest<Result<Guid>>;
