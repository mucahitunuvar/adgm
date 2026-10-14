using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Parsing;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed class AdminGetUsersQueryValidator : AbstractValidator<AdminGetUsersQuery>
{
    public AdminGetUsersQueryValidator()
    {
        // Page/PageSize are no longer validated here: PagedRequest (SharedKernel) clamps
        // out-of-range values silently at construction instead of failing validation.
        RuleFor(q => q.Role)
            .Must(role => role is null || EnumParsing.TryParseDefined<UserRole>(role, out _))
            .WithMessage("Role must be a valid role name.");

        RuleFor(q => q.Status)
            .Must(status => status is null || EnumParsing.TryParseDefined<UserStatus>(status, out _))
            .WithMessage("Status must be a valid status name.");
    }
}
