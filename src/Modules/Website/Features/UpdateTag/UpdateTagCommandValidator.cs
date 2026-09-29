using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateTag;

public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
    }
}
