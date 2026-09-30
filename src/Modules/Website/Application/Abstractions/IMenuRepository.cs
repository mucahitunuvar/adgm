using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Faz 2 Görev 1: exactly three Menu rows exist (one per MenuLocation), seeded by migration and never
// created or deleted through an admin action - so, unlike every other aggregate repository, there is
// no Add/Remove here.
public interface IMenuRepository
{
    Task<Menu?> GetByLocationAsync(MenuLocation location, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Menu>> GetAllAsync(CancellationToken cancellationToken = default);

    // ADR-024 §1.2 "kullanım koruması": every Menu containing at least one item whose LinkTarget
    // points at contentItemId - ContentItemPermanentDeletionService's cascade step.
    Task<IReadOnlyList<Menu>> GetByLinkedContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);
}
