using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateVideo;

public sealed class ActivateVideoCommandValidator : AbstractValidator<ActivateVideoCommand>
{
    public ActivateVideoCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
