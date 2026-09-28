using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateRedirect;

public sealed class CreateRedirectCommandValidator : AbstractValidator<CreateRedirectCommand>
{
    public CreateRedirectCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.FromPath).NotEmpty();
        RuleFor(c => c.TargetKind).NotEmpty();
        RuleFor(c => c.StatusCode).NotEmpty();
    }
}
