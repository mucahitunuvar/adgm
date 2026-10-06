using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class EventScheduleRepository(WebsiteDbContext dbContext) : IEventScheduleRepository
{
    public Task<EventSchedule?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        dbContext.EventSchedules.FirstOrDefaultAsync(es => es.ContentItemId == contentItemId, cancellationToken);

    public Task<bool> ExistsForContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default)
    {
        var contentItemIds = dbContext.ContentItems.Where(ci => ci.ContentTypeId == contentTypeId).Select(ci => ci.Id);
        return dbContext.EventSchedules.AnyAsync(es => contentItemIds.Contains(es.ContentItemId), cancellationToken);
    }

    public void Add(EventSchedule eventSchedule) => dbContext.EventSchedules.Add(eventSchedule);

    public void Remove(EventSchedule eventSchedule) => dbContext.EventSchedules.Remove(eventSchedule);
}
