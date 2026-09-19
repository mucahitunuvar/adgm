using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;

public sealed class SendBulkCandidateNotificationCommandValidator : AbstractValidator<SendBulkCandidateNotificationCommand>
{
    public SendBulkCandidateNotificationCommandValidator()
    {
        RuleFor(c => c.Subject).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Message).NotEmpty().MaximumLength(4000);
    }
}
