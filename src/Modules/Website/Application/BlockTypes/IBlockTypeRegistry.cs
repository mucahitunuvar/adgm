namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1: the one place every block type is looked up by key - backs both the admin block-types catalog
// endpoint and every PageLayout draft-block validation.
public interface IBlockTypeRegistry
{
    IBlockTypeDefinition? TryGet(string key);

    IReadOnlyList<IBlockTypeDefinition> GetAll();
}
