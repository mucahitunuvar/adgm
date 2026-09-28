using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentType;

public sealed class ActivateContentTypeCommandValidator : AbstractValidator<ActivateContentTypeCommand>
{
    public ActivateContentTypeCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
