using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    UserRole? Role { get; }
}
