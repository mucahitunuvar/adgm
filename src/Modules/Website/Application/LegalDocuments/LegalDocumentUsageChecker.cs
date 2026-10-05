using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.LegalDocuments;

// §12.1: always-empty stub, exactly like SliderUsageChecker's own pre-Görev-4 state - no aggregate
// references a LegalDocument yet (FormDefinition does not exist until Görev 3, which replaces this
// with a real implementation). Until then, DeleteLegalDocument's usage check always allows deletion
// (HasEverBeenPublished is the only guard that applies in this Görev).
public sealed class LegalDocumentUsageChecker : ILegalDocumentUsageChecker
{
    public Task<IReadOnlyList<LegalDocumentUsage>> GetUsagesAsync(Guid legalDocumentId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LegalDocumentUsage>>([]);
}
