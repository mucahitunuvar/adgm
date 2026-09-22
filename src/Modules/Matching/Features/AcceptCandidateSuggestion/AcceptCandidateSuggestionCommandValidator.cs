using FluentValidation;

namespace GenclikMerkezi.Modules.Matching.Features.AcceptCandidateSuggestion;

public sealed class AcceptCandidateSuggestionCommandValidator : AbstractValidator<AcceptCandidateSuggestionCommand>
{
    public AcceptCandidateSuggestionCommandValidator()
    {
        RuleFor(c => c.CandidateSuggestionId).NotEmpty();
    }
}
