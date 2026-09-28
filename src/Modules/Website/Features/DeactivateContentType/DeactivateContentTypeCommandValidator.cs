using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateContentType;

public sealed class DeactivateContentTypeCommandValidator : AbstractValidator<DeactivateContentTypeCommand>
{
    public DeactivateContentTypeCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
