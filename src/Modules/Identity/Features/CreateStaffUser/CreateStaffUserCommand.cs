using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.CreateStaffUser;

public sealed record CreateStaffUserCommand(
    string Email, string Password, string FirstName, string LastName, string? PhoneNumber, string Role)
    : IRequest<Result<CreateStaffUserResponse>>;
