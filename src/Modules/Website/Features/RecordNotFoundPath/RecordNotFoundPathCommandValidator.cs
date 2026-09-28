using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.RecordNotFoundPath;

public sealed class RecordNotFoundPathCommandValidator : AbstractValidator<RecordNotFoundPathCommand>
{
    public RecordNotFoundPathCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.Path).NotEmpty();
    }
}
