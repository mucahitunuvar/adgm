using FluentValidation;

namespace GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;

public sealed class RejectCandidateSuggestionCommandValidator : AbstractValidator<RejectCandidateSuggestionCommand>
{
    public RejectCandidateSuggestionCommandValidator()
    {
        RuleFor(c => c.CandidateSuggestionId).NotEmpty();
    }
}
