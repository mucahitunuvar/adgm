using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Parsing;

namespace GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        RuleFor(c => c.NewRole)
            .NotEmpty()
            .Must(role => EnumParsing.TryParseDefined<UserRole>(role, out _))
            .WithMessage("NewRole must be a valid role.");
    }
}
