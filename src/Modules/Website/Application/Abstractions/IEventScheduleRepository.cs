using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IEventScheduleRepository
{
    Task<EventSchedule?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    // Cross-aggregate check for UpdateContentTypeCommandHandler's "SupportsEvent cannot be turned off
    // while a content item of this type still has a calendar" guard (ADR-024 §11.1 / Faz 4 Görev 1) -
    // EventSchedule has no direct ContentTypeId of its own, so this joins through ContentItemId.
    Task<bool> ExistsForContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    void Add(EventSchedule eventSchedule);

    void Remove(EventSchedule eventSchedule);
}
