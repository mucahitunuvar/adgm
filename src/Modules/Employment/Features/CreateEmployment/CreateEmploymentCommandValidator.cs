using FluentValidation;

namespace GenclikMerkezi.Modules.Employment.Features.CreateEmployment;

public sealed class CreateEmploymentCommandValidator : AbstractValidator<CreateEmploymentCommand>
{
    public CreateEmploymentCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.PositionId).NotEmpty();
        RuleFor(c => c.StartDateUtc).NotEmpty();
    }
}
