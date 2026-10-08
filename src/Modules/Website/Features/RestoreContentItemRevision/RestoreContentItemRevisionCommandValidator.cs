using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;

public sealed class RestoreContentItemRevisionCommandValidator : AbstractValidator<RestoreContentItemRevisionCommand>
{
    public RestoreContentItemRevisionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.LanguageCode).NotEmpty();
    }
}
