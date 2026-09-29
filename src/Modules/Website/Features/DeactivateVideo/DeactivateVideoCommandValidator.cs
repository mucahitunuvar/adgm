using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateVideo;

public sealed class DeactivateVideoCommandValidator : AbstractValidator<DeactivateVideoCommand>
{
    public DeactivateVideoCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
