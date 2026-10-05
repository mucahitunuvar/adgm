namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// DeleteFormDefinitionCommandHandler's guard - the same "scan every referencing aggregate before
// allowing deletion" shape ILegalDocumentUsageChecker/IMediaUsageChecker already use.
public interface IFormDefinitionUsageChecker
{
    Task<IReadOnlyList<FormDefinitionUsage>> GetUsagesAsync(Guid formDefinitionId, CancellationToken cancellationToken = default);
}
