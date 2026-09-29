namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Mirrors MediaUsage's shape for the same reason (ADR-024 §5 Faz 1b Görev 2): one place a Video is
// referenced from. Only ContentItem will ever reference a Video (Görev 4), so unlike MediaUsage there
// is no provider/composite fan-out - IVideoUsageChecker is the single, direct check.
public sealed record VideoUsage(string SourceKey, Guid SourceId, string Description, string? Url);
