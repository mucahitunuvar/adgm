using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;

public sealed class AdminGetUsersQueryValidator : AbstractValidator<AdminGetUsersQuery>
{
    public AdminGetUsersQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(q => q.Role)
            .Must(role => role is null || Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be a valid role name.");

        RuleFor(q => q.Status)
            .Must(status => status is null || Enum.TryParse<UserStatus>(status, ignoreCase: true, out _))
            .WithMessage("Status must be a valid status name.");
    }
}
