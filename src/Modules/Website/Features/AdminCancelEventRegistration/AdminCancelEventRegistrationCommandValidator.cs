using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.AdminCancelEventRegistration;

public sealed class AdminCancelEventRegistrationCommandValidator : AbstractValidator<AdminCancelEventRegistrationCommand>
{
    public AdminCancelEventRegistrationCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
