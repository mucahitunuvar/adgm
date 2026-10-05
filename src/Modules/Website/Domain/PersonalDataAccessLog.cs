using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2/§14 (Faz 3 Görev 5). "Identity'deki merkezi audit log başka modüllerce
// kullanılamadığı için Website kendi PersonalDataAccessLog tablosunu tutar: kim, ne zaman, hangi
// kayda, hangi işlem (görüntüleme, dosya indirme, dışa aktarma). Kişisel veri içeren her okuma ucu bu
// kaydı yazar." Written by a dedicated command after a read succeeds (RecordPersonalDataAccessCommand),
// never by the read query itself - the same split RecordNotFoundPathCommandHandler already uses for
// NotFoundLog (AGENTS.md §13: a query must not write). EntityId is null for a bulk export with no
// single owning record (e.g. the newsletter CSV export).
public sealed class PersonalDataAccessLog : AggregateRoot
{
    public const int MaxDetailLength = 1000;

    public Guid UserId { get; private set; }

    public DateTime AccessedAtUtc { get; private set; }

    public PersonalDataEntityType EntityType { get; private set; }

    public Guid? EntityId { get; private set; }

    public PersonalDataAccessAction Action { get; private set; }

    public string? Detail { get; private set; }

    private PersonalDataAccessLog(
        Guid id, Guid userId, DateTime accessedAtUtc, PersonalDataEntityType entityType, Guid? entityId, PersonalDataAccessAction action,
        string? detail)
        : base(id)
    {
        UserId = userId;
        AccessedAtUtc = accessedAtUtc;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        Detail = detail;
    }

    private PersonalDataAccessLog()
    {
    }

    public static Result<PersonalDataAccessLog> Create(
        Guid userId, DateTime accessedAtUtc, PersonalDataEntityType entityType, Guid? entityId, PersonalDataAccessAction action,
        string? detail)
    {
        var trimmedDetail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
        if (trimmedDetail is { Length: > MaxDetailLength })
        {
            return Result.Failure<PersonalDataAccessLog>(Error.Validation(
                "PersonalDataAccessLog.DetailTooLong", $"Detail must be at most {MaxDetailLength} characters."));
        }

        return Result.Success(new PersonalDataAccessLog(Guid.NewGuid(), userId, accessedAtUtc, entityType, entityId, action, trimmedDetail));
    }
}
