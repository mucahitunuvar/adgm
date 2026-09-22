using FluentValidation;

namespace GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;

public sealed class CreateCandidateSuggestionCommandValidator : AbstractValidator<CreateCandidateSuggestionCommand>
{
    public CreateCandidateSuggestionCommandValidator()
    {
        RuleFor(c => c.PersonnelNeedId).NotEmpty();
        RuleFor(c => c.CandidateCvId).NotEmpty();
    }
}
