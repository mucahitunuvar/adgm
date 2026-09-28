using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;

public sealed class ArchiveContentItemCommandValidator : AbstractValidator<ArchiveContentItemCommand>
{
    public ArchiveContentItemCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
