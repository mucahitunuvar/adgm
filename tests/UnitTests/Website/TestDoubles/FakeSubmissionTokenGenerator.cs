using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeSubmissionTokenGenerator : ISubmissionTokenGenerator
{
    private static readonly Error InvalidTokenError = Error.Validation(
        "PublicSubmission.InvalidToken", "Invalid or expired submission token.");

    public Result<DateTimeOffset>? NextValidateTokenResult { get; set; }

    public string GenerateToken(DateTimeOffset issuedAtUtc, TimeSpan duration) =>
        $"{issuedAtUtc:O}|{duration}";

    public Result<DateTimeOffset> ValidateToken(string? token) =>
        NextValidateTokenResult ?? Result.Failure<DateTimeOffset>(InvalidTokenError);
}
