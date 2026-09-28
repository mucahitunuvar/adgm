using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateRedirect;

public sealed class UpdateRedirectCommandValidator : AbstractValidator<UpdateRedirectCommand>
{
    public UpdateRedirectCommandValidator()
    {
        RuleFor(c => c.TargetKind).NotEmpty();
        RuleFor(c => c.StatusCode).NotEmpty();
    }
}
