using System.Globalization;
using System.Security.Cryptography;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.AspNetCore.DataProtection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.PublicSubmissions;

// ADR-024 §12.3 (Faz 3 Görev 1): the issued time is embedded directly in the signed payload (not
// derived from Data Protection's own expiration/duration bookkeeping) so IPublicSubmissionGuard can
// read it back regardless of which duration GenerateToken was called with - the same reasoning
// DataProtectionContentPreviewLinkGenerator applies to its own payload. ITimeLimitedDataProtector
// still separately enforces the duration itself: Unprotect throws CryptographicException once it has
// elapsed, same as a tampered payload - a single catch covers both, since the caller only ever needs
// "valid" vs. "not valid".
public sealed class DataProtectionSubmissionTokenGenerator : ISubmissionTokenGenerator
{
    private const string Purpose = "Website.PublicSubmissionToken.v1";

    private readonly ITimeLimitedDataProtector _protector;

    public DataProtectionSubmissionTokenGenerator(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
    }

    public string GenerateToken(DateTimeOffset issuedAtUtc, TimeSpan duration) =>
        _protector.Protect(issuedAtUtc.ToString("O", CultureInfo.InvariantCulture), duration);

    public Result<DateTimeOffset> ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<DateTimeOffset>(InvalidTokenError);
        }

        try
        {
            var payload = _protector.Unprotect(token);
            if (!DateTimeOffset.TryParse(
                    payload, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var issuedAtUtc))
            {
                return Result.Failure<DateTimeOffset>(InvalidTokenError);
            }

            return Result.Success(issuedAtUtc);
        }
        catch (CryptographicException)
        {
            return Result.Failure<DateTimeOffset>(InvalidTokenError);
        }
        catch (FormatException)
        {
            return Result.Failure<DateTimeOffset>(InvalidTokenError);
        }
    }

    private static readonly Error InvalidTokenError = Error.Validation(
        "PublicSubmission.InvalidToken", "Invalid or expired submission token.");
}
