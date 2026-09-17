namespace GenclikMerkezi.Modules.Candidate.Domain;

// Domain service (AGENTS.md §10): the completion percentage naturally spans two separate aggregates
// (CandidateCv + CandidateCvContent), so it does not belong on either one alone. Stateless/pure -
// no infrastructure dependency, easy to unit test directly. Criteria and equal weighting per the
// Candidate module design ADR, Decision 3 (first version - extensible later).
public static class CandidateCvCompletionCalculator
{
    public static int Calculate(CandidateCv candidateCv, CandidateCvContent? candidateCvContent)
    {
        bool[] criteria =
        [
            candidateCv.Photo is not null,
            !string.IsNullOrWhiteSpace(candidateCv.Address),
            candidateCv.SocialMediaLinks.Count > 0,
            !string.IsNullOrWhiteSpace(candidateCvContent?.Summary),
            candidateCvContent?.Experiences.Count > 0,
            candidateCvContent?.Educations.Count > 0,
            candidateCvContent?.Languages.Count > 0,
            candidateCvContent?.CvFile is not null,
        ];

        var metCount = criteria.Count(met => met);

        return (int)Math.Round(metCount * 100.0 / criteria.Length);
    }
}
