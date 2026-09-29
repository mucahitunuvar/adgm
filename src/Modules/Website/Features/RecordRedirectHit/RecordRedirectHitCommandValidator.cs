using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.RecordRedirectHit;

public sealed class RecordRedirectHitCommandValidator : AbstractValidator<RecordRedirectHitCommand>
{
    public RecordRedirectHitCommandValidator()
    {
        RuleFor(c => c.RedirectId).NotEmpty();
    }
}
