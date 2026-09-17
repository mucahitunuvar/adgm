using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

public sealed record RegisterCandidateCommand(string Email, string Password, string FirstName, string LastName, string? PhoneNumber)
    : IRequest<Result<RegisterCandidateResponse>>;
