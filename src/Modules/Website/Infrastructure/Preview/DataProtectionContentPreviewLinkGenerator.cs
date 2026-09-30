using System.Security.Cryptography;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.AspNetCore.DataProtection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Preview;

// ADR-024 §4.5 (Faz 1b Görev 6): ITimeLimitedDataProtector.Unprotect throws CryptographicException for
// both a tampered AND an expired payload - a single catch is enough, since GetContentPreviewQuery only
// ever needs to tell "valid" from "not valid", never which way it failed (never leaking that detail to
// an anonymous caller is the whole point of a signed token).
public sealed class DataProtectionContentPreviewLinkGenerator : IContentPreviewLinkGenerator
{
    private const string Purpose = "Website.ContentPreviewLink.v1";

    private readonly ITimeLimitedDataProtector _protector;

    public DataProtectionContentPreviewLinkGenerator(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
    }

    public string GenerateToken(Guid contentItemId, string? languageCode, TimeSpan duration) =>
        _protector.Protect($"{contentItemId:N}|{languageCode}", duration);

    public Result<ContentPreviewToken> ValidateToken(string token)
    {
        try
        {
            var payload = _protector.Unprotect(token);
            var parts = payload.Split('|', 2);
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var contentItemId))
            {
                return Result.Failure<ContentPreviewToken>(Error.NotFound("ContentPreview.InvalidToken", "Invalid preview token."));
            }

            var languageCode = parts[1].Length == 0 ? null : parts[1];
            return Result.Success(new ContentPreviewToken(contentItemId, languageCode));
        }
        catch (CryptographicException)
        {
            return Result.Failure<ContentPreviewToken>(Error.NotFound("ContentPreview.InvalidToken", "Invalid preview token."));
        }
        catch (FormatException)
        {
            return Result.Failure<ContentPreviewToken>(Error.NotFound("ContentPreview.InvalidToken", "Invalid preview token."));
        }
    }
}
