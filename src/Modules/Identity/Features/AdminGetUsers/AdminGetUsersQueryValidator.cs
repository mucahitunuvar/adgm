using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed class AdminGetUsersQueryValidator : AbstractValidator<AdminGetUsersQuery>
{
    public AdminGetUsersQueryValidator()
    {
        // Page/PageSize are no longer validated here: PagedRequest (SharedKernel) clamps
        // out-of-range values silently at construction instead of failing validation.
        RuleFor(q => q.Role)
            .Must(role => role is null || Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be a valid role name.");

        RuleFor(q => q.Status)
            .Must(status => status is null || Enum.TryParse<UserStatus>(status, ignoreCase: true, out _))
            .WithMessage("Status must be a valid status name.");
    }
}
