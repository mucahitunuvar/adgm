using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.SendBulkNotification;

public sealed class SendBulkNotificationCommandValidator : AbstractValidator<SendBulkNotificationCommand>
{
    public SendBulkNotificationCommandValidator()
    {
        RuleFor(c => c.CandidateUserIds).NotEmpty();
        RuleFor(c => c.Subject).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Message).NotEmpty().MaximumLength(4000);
    }
}
