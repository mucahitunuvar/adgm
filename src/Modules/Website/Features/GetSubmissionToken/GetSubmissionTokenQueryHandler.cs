using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;

// ADR-024 §12.3 (Faz 3 Görev 1): issues the signed, time-limited token IPublicSubmissionGuard later
// validates - no database row is created, the token carries everything the guard needs (its own
// issued time) inside its signed payload.
public sealed class GetSubmissionTokenQueryHandler(ISubmissionTokenGenerator submissionTokenGenerator, TimeProvider timeProvider)
    : IRequestHandler<GetSubmissionTokenQuery, Result<GetSubmissionTokenResponse>>
{
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);

    public Task<Result<GetSubmissionTokenResponse>> Handle(GetSubmissionTokenQuery request, CancellationToken cancellationToken)
    {
        var token = submissionTokenGenerator.GenerateToken(timeProvider.GetUtcNow(), TokenLifetime);
        return Task.FromResult(Result.Success(new GetSubmissionTokenResponse(token)));
    }
}
