using FluentValidation;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed class CreateTaxOfficeCommandValidator : AbstractValidator<CreateTaxOfficeCommand>
{
    public CreateTaxOfficeCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.ProvinceId)
            .NotEmpty();
    }
}
