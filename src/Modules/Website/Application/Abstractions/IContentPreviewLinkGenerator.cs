using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.5 (Faz 1b Görev 6): signed, time-limited preview link tokens - Infrastructure implements
// this with ASP.NET Core Data Protection's ITimeLimitedDataProtector so the Application layer never
// depends on that library directly.
public interface IContentPreviewLinkGenerator
{
    string GenerateToken(Guid contentItemId, string? languageCode, TimeSpan duration);

    // A single failure mode ("ContentPreview.InvalidToken") covers a malformed, tampered-with AND an
    // expired token alike - GetContentPreviewQueryHandler turns any failure into the same
    // ayrıntı vermeyen 404 the master prompt requires, so the two cases need no distinction here.
    Result<ContentPreviewToken> ValidateToken(string token);
}
