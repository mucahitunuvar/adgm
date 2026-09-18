using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;

public sealed record CreateCareerAdvisorCommand(string Email, string Password, string FirstName, string LastName, string? PhoneNumber)
    : IRequest<Result<CreateCareerAdvisorResponse>>;
