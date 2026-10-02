using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetBlockTypes;

public sealed class GetBlockTypesQueryHandler(IBlockTypeRegistry blockTypeRegistry)
    : IRequestHandler<GetBlockTypesQuery, Result<IReadOnlyList<BlockTypeResponse>>>
{
    public Task<Result<IReadOnlyList<BlockTypeResponse>>> Handle(GetBlockTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<BlockTypeResponse> response = blockTypeRegistry.GetAll()
            .Select(definition => new BlockTypeResponse(
                definition.Key,
                definition.AllowedTargets.Select(t => t.ToString()).ToList(),
                definition.DescribeSettingsFields(),
                definition.DescribeTextsFields()))
            .ToList();

        return Task.FromResult(Result.Success(response));
    }
}
