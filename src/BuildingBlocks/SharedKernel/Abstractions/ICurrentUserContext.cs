namespace GenclikMerkezi.SharedKernel.Abstractions;

// ADR-017 Decision 2. Deliberately minimal and independent from any module's own "current user"
// abstraction (e.g. Identity.Application.Abstractions.ICurrentUserService, which SharedKernel/
// BuildingBlocks.Infrastructure cannot depend on - that would be a circular module dependency).
// Exists only so cross-cutting, module-independent code (currently: IUserScopedCacheService) can
// key by "the calling user" without depending on any specific module.
public interface ICurrentUserContext
{
    Guid? UserId { get; }
}
