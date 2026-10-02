using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakePartnerRepository : IPartnerRepository
{
    private readonly List<Partner> _partners = [];

    public void Seed(Partner partner) => _partners.Add(partner);

    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_partners.FirstOrDefault(p => p.Id == id));

    public Task<PagedResult<Partner>> SearchAsync(
        bool? isActive, string? search, LanguageCode defaultLanguageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var items = _partners.OrderBy(p => p.SortOrder).ToList();
        return Task.FromResult(new PagedResult<Partner>(items, items.Count, pagedRequest.Page, pagedRequest.PageSize));
    }

    public Task<IReadOnlyList<Partner>> SearchPublicAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Partner>>(
            _partners.Where(p => p.IsActive && p.Translations.Any(t => t.LanguageCode == languageCode)).OrderBy(p => p.SortOrder).ToList());

    public Task<IReadOnlyList<Partner>> SearchByLogoMediaIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Partner>>(_partners.Where(p => p.LogoMediaId == mediaAssetId).ToList());

    public void Add(Partner partner) => _partners.Add(partner);

    public void Remove(Partner partner) => _partners.RemoveAll(p => p.Id == partner.Id);
}
