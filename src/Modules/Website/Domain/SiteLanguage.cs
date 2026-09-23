using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §3. Invariants (enforced partly here, partly by the application-layer SetDefault flow
// since "exactly one default across the whole table" cannot be checked from a single instance):
//   - The default language can never be deactivated (Deactivate guards on IsDefault).
//   - An inactive language can never become the default (MarkAsDefault guards on IsActive).
//   - Exactly one default exists system-wide: Create() never produces a default language, so the
//     only path to IsDefault = true is the SetDefaultSiteLanguage command, which atomically calls
//     UnmarkAsDefault() on the previous default and MarkAsDefault() on the new one in one
//     SaveChangesAsync - see SetDefaultSiteLanguageCommandHandler.
public sealed class SiteLanguage : AggregateRoot
{
    public LanguageCode Code { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private SiteLanguage(Guid id, LanguageCode code, string name, int sortOrder, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        IsDefault = false;
        IsActive = true;
        SortOrder = sortOrder;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private SiteLanguage()
    {
    }

    public static SiteLanguage Create(LanguageCode code, string name, int sortOrder, Guid createdByUserId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), code, name.Trim(), sortOrder, createdByUserId, createdAtUtc);

    public Result Rename(string name, int sortOrder, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("SiteLanguage.NameRequired", "Name is required."));
        }

        Name = name.Trim();
        SortOrder = sortOrder;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result Activate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = true;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result Deactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (IsDefault)
        {
            return Result.Failure(Error.Conflict(
                "SiteLanguage.CannotDeactivateDefault", "The default site language cannot be deactivated."));
        }

        IsActive = false;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    // Only called by SetDefaultSiteLanguageCommandHandler, on the language being promoted.
    public Result MarkAsDefault(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict(
                "SiteLanguage.InactiveCannotBeDefault", "An inactive language cannot be made the default."));
        }

        IsDefault = true;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    // Only called by SetDefaultSiteLanguageCommandHandler, on the previous default, after
    // MarkAsDefault() has already succeeded on the new one in the same transaction.
    public void UnmarkAsDefault(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsDefault = false;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
    }
}
