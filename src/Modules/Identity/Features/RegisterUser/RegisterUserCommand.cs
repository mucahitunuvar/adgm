using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

public sealed record RegisterUserCommand(
    string Email, string Password, string FirstName, string LastName, string? PhoneNumber, string Role)
    : IRequest<Result<RegisterUserResponse>>;
