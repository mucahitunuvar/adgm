using System.Globalization;
using System.Security.Cryptography;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.AspNetCore.DataProtection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Newsletter;

// ADR-024 §14 (Faz 3 Görev 6): same shape as DataProtectionSubmissionTokenGenerator/
// DataProtectionContentPreviewLinkGenerator - a single catch covers both a tampered and an expired
// payload, since the caller only ever needs "valid" vs. "not valid".
public sealed class DataProtectionNewsletterConfirmationLinkGenerator : INewsletterConfirmationLinkGenerator
{
    private const string Purpose = "Website.NewsletterConfirmation.v1";

    private readonly ITimeLimitedDataProtector _protector;

    public DataProtectionNewsletterConfirmationLinkGenerator(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
    }

    public string GenerateToken(Guid subscriberId, TimeSpan duration) =>
        _protector.Protect(subscriberId.ToString("N", CultureInfo.InvariantCulture), duration);

    public Result<Guid> ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<Guid>(InvalidTokenError);
        }

        try
        {
            var payload = _protector.Unprotect(token);
            return Guid.TryParse(payload, out var subscriberId)
                ? Result.Success(subscriberId)
                : Result.Failure<Guid>(InvalidTokenError);
        }
        catch (CryptographicException)
        {
            return Result.Failure<Guid>(InvalidTokenError);
        }
        catch (FormatException)
        {
            return Result.Failure<Guid>(InvalidTokenError);
        }
    }

    private static readonly Error InvalidTokenError = Error.Validation(
        "Newsletter.InvalidConfirmationToken", "Invalid or expired confirmation token.");
}
