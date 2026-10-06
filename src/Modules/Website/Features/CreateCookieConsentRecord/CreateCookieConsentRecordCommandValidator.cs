using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;

public sealed class CreateCookieConsentRecordCommandValidator : AbstractValidator<CreateCookieConsentRecordCommand>
{
    public CreateCookieConsentRecordCommandValidator()
    {
        RuleFor(c => c.ConsentId).NotEmpty();
        RuleFor(c => c.Action).NotEmpty();
    }
}
