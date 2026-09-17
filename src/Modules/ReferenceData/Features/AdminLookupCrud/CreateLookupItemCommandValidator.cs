using FluentValidation;
using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed class CreateLookupItemCommandValidator<TLookup> : AbstractValidator<CreateLookupItemCommand<TLookup>>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>
{
    public CreateLookupItemCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
