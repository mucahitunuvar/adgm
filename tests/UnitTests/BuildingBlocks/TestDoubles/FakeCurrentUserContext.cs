using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;

public sealed class FakeCurrentUserContext(Guid? userId) : ICurrentUserContext
{
    public Guid? UserId { get; } = userId;
}
