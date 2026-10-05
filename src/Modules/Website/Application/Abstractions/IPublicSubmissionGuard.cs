using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.3 (Faz 3 Görev 1): the single port every anonymous write endpoint (form submission -
// Görev 4, newsletter subscription - Görev 6, cookie consent - Görev 7) calls before doing anything
// else. Checking the submission token, minimum fill time, honeypot and (when enabled) the Turnstile
// challenge in one place means the four checks and their single, detail-free rejection can never drift
// between those three features. An interface (rather than a plain Application service, see
// RouteResolutionService) so each of those features' own handler unit tests can fake this out instead
// of re-exercising every guard path themselves - PublicSubmissionGuardTests is the only place that does.
public interface IPublicSubmissionGuard
{
    Task<Result> VerifyAsync(PublicSubmissionGuardRequest request, string? remoteIpAddress, CancellationToken cancellationToken = default);
}
