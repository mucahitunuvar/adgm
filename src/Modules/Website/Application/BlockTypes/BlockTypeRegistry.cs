namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1 "Blok tipleri kodla tanımlanır, veritabanında tutulmaz" - every IBlockTypeDefinition is
// registered in DI (WebsiteModuleServiceCollectionExtensions) and fanned into this single lookup, the
// same "many small implementations behind one registry" shape IMediaUsageProvider/
// CompositeMediaUsageChecker already uses.
public sealed class BlockTypeRegistry : IBlockTypeRegistry
{
    private readonly IReadOnlyDictionary<string, IBlockTypeDefinition> _definitionsByKey;

    public BlockTypeRegistry(IEnumerable<IBlockTypeDefinition> definitions)
    {
        _definitionsByKey = definitions.ToDictionary(d => d.Key, StringComparer.Ordinal);
    }

    public IBlockTypeDefinition? TryGet(string key) => _definitionsByKey.GetValueOrDefault(key);

    public IReadOnlyList<IBlockTypeDefinition> GetAll() => _definitionsByKey.Values.OrderBy(d => d.Key, StringComparer.Ordinal).ToList();
}
