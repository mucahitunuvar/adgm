using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.RejectEventRegistration;

public sealed class RejectEventRegistrationCommandValidator : AbstractValidator<RejectEventRegistrationCommand>
{
    public const int MaxReasonLength = 500;

    public RejectEventRegistrationCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Reason).MaximumLength(MaxReasonLength);
    }
}
