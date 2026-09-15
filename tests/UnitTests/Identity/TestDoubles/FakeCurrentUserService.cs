using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated => UserId is not null;

    public Guid? UserId { get; set; }

    public UserRole? Role { get; set; }
}
