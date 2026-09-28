using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;

public sealed class ConvertNotFoundPathToRedirectCommandValidator : AbstractValidator<ConvertNotFoundPathToRedirectCommand>
{
    public ConvertNotFoundPathToRedirectCommandValidator()
    {
        RuleFor(c => c.TargetKind).NotEmpty();
        RuleFor(c => c.StatusCode).NotEmpty();
    }
}
