using FluentValidation;
using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed class UpdateLookupItemCommandValidator<TLookup> : AbstractValidator<UpdateLookupItemCommand<TLookup>>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>
{
    public UpdateLookupItemCommandValidator()
    {
        RuleFor(c => c.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
