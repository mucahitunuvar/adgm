using FluentValidation;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed class UpdateTaxOfficeCommandValidator : AbstractValidator<UpdateTaxOfficeCommand>
{
    public UpdateTaxOfficeCommandValidator()
    {
        RuleFor(c => c.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
