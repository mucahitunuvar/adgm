using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeSliderRepository : ISliderRepository
{
    private readonly List<Slider> _sliders = [];

    public void Seed(Slider slider) => _sliders.Add(slider);

    public Task<Slider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sliders.FirstOrDefault(s => s.Id == id));

    public Task<Slider?> GetByKeyAsync(SliderKey key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sliders.FirstOrDefault(s => s.Key == key));

    public Task<IReadOnlyList<Slider>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Slider>>(_sliders.ToList());

    public Task<IReadOnlyList<Slider>> SearchByImageIdAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Slider>>(
            _sliders.Where(s => s.Slides.Any(sl => sl.DesktopImageMediaId == mediaAssetId || sl.MobileImageMediaId == mediaAssetId)).ToList());

    public Task<DateTime?> GetEarliestUpcomingSlideTransitionAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        var upcoming = _sliders
            .SelectMany(s => s.Slides)
            .Where(sl => sl.IsActive)
            .SelectMany(sl => new[] { sl.PublishAtUtc, sl.UnpublishAtUtc })
            .Where(t => t is not null && t.Value > now)
            .Select(t => t!.Value)
            .OrderBy(t => t)
            .Cast<DateTime?>()
            .FirstOrDefault();

        return Task.FromResult(upcoming);
    }

    public void Add(Slider slider) => _sliders.Add(slider);

    public void Remove(Slider slider) => _sliders.RemoveAll(s => s.Id == slider.Id);
}
