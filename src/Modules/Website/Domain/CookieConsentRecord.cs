using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): one cookie-banner decision. Write-once and anonymous - unlike every
// other Website aggregate there is no RowVersion/UpdatedBy (nothing ever updates a row after it is
// recorded) and no CreatedByUserId (IP/User-Agent/identity are deliberately never stored - "IP,
// User-Agent, kullanıcı kimliği tutulmaz"). ConsentId is the visitor's own client-generated id, used
// only to link a browser's later preference changes together for analytics - it is NOT unique, since
// the same visitor may record several CookieConsentRecord rows (one per banner interaction) over time.
public sealed class CookieConsentRecord : AggregateRoot
{
    private readonly List<ThirdPartyScriptCategory> _categories = [];

    public Guid ConsentId { get; private set; }

    public IReadOnlyList<ThirdPartyScriptCategory> Categories => _categories.AsReadOnly();

    public LegalDocumentKey PolicyKey { get; private set; } = null!;

    public int PolicyVersion { get; private set; }

    public CookieConsentAction Action { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    private CookieConsentRecord(
        Guid id, Guid consentId, IReadOnlyList<ThirdPartyScriptCategory> categories, LegalDocumentKey policyKey, int policyVersion,
        CookieConsentAction action, DateTime recordedAtUtc)
        : base(id)
    {
        ConsentId = consentId;
        _categories.AddRange(categories);
        PolicyKey = policyKey;
        PolicyVersion = policyVersion;
        Action = action;
        RecordedAtUtc = recordedAtUtc;
    }

    private CookieConsentRecord()
    {
    }

    // §13 "Necessary kategorisi her zaman onaylı kabul edilir; istemci göndermese de kayda eklenir" -
    // enforced here, not merely by the caller, so no path into this aggregate can ever persist a record
    // missing it.
    public static Result<CookieConsentRecord> Create(
        Guid consentId,
        IReadOnlyList<ThirdPartyScriptCategory>? requestedCategories,
        LegalDocumentKey policyKey,
        int policyVersion,
        CookieConsentAction action,
        DateTime recordedAtUtc)
    {
        if (consentId == Guid.Empty)
        {
            return Result.Failure<CookieConsentRecord>(Error.Validation("CookieConsentRecord.ConsentIdRequired", "Consent id is required."));
        }

        var categories = (requestedCategories ?? [])
            .Append(ThirdPartyScriptCategory.Necessary)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return Result.Success(new CookieConsentRecord(Guid.NewGuid(), consentId, categories, policyKey, policyVersion, action, recordedAtUtc));
    }
}
