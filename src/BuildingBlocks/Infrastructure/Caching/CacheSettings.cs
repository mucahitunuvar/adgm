namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;

public sealed class CacheSettings
{
    public const string SectionName = "Caching";

    public int DefaultTtlMinutes { get; init; } = 60;
}
