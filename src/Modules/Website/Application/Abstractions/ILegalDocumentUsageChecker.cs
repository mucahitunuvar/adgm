namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// §12.1 "bir form tarafından kullanılıyorsa silinemez (Görev 3 bu kontrolü bağlar:
// ILegalDocumentUsageChecker portu bu görevde kurulur)": DeleteLegalDocumentCommandHandler's guard,
// the same "port now, real implementation once the referencing aggregate exists" shape
// ISliderUsageChecker used before PageLayout's hero-slider block existed.
public interface ILegalDocumentUsageChecker
{
    Task<IReadOnlyList<LegalDocumentUsage>> GetUsagesAsync(Guid legalDocumentId, CancellationToken cancellationToken = default);
}
